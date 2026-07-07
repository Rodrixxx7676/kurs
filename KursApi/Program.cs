using KursApi.Data;
using KursApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ── OpenAPI / Swagger ────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── Servicios de seguridad ───────────────────────────────────────────────────
builder.Services.AddSingleton<LoginThrottleService>();

// ── Rate Limiting — máx. 5 intentos de login por minuto por IP ──────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiter =>
    {
        limiter.Window      = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 5;
        limiter.QueueLimit  = 0;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── PostgreSQL — Entity Framework Core ──────────────────────────────────────
// Acepta formato clave=valor o URI (postgres://…), que es como lo entregan
// proveedores como Render y Neon.
var connectionString = builder.Configuration.GetConnectionString("Default")!;
if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
    connectionString = ConvertirUriPostgres(connectionString);

builder.Services.AddDbContext<KursDbContext>(options =>
    options.UseNpgsql(connectionString));

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero   // sin margen extra de expiración
        };
    });

builder.Services.AddAuthorization();

// ── Controladores ────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── CORS — permite peticiones desde el frontend ──────────────────────────────
// En producción el frontend se sirve desde esta misma API (mismo origen), así
// que CORS solo aplica en desarrollo o si se configura Cors:AllowedOrigins.
string[] devOrigins =
[
    "https://localhost:7090",    // Blazor WASM (HTTPS dev)
    "http://localhost:5090",     // Blazor WASM (HTTP dev)
    "http://127.0.0.1:3000",     // HTML frontend (Live Server)
    "http://localhost:3000",     // HTML frontend (Live Server alt)
    "http://127.0.0.1:5500",     // HTML frontend (VS Code Live Server)
    "http://localhost:5500"      // HTML frontend (VS Code Live Server alt)
];
string[] extraOrigins = builder.Configuration["Cors:AllowedOrigins"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("KursFrontend", policy =>
    {
        policy
            .WithOrigins([.. devOrigins, .. extraOrigins])
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// ── Esquema de base de datos ─────────────────────────────────────────────────
// Crea tablas y secuencias a partir del modelo si la BD está vacía.
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<KursDbContext>().Database.EnsureCreated();

// ── Pipeline HTTP ────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
    app.MapOpenApi();           // Documentación en /openapi/v1.json

// Render/Railway terminan TLS en su proxy; sin esto la app vería todo como
// http y UseHttpsRedirection entraría en bucle de redirección.
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownNetworks.Clear();   // el proxy del PaaS no tiene IP fija conocida
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

app.UseHttpsRedirection();

// ── Frontend Blazor WASM ─────────────────────────────────────────────────────
// En producción el Dockerfile copia el publish de KursFront a wwwroot.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("KursFrontend");
app.UseRateLimiter();
app.UseAuthentication();        // ← ANTES de UseAuthorization
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

// Convierte postgres://usuario:pass@host:puerto/bd al formato clave=valor de Npgsql.
static string ConvertirUriPostgres(string uri)
{
    var u = new Uri(uri);
    var userInfo = u.UserInfo.Split(':', 2);
    return $"Host={u.Host};" +
           $"Port={(u.Port > 0 ? u.Port : 5432)};" +
           $"Database={u.AbsolutePath.TrimStart('/')};" +
           $"Username={Uri.UnescapeDataString(userInfo[0])};" +
           $"Password={Uri.UnescapeDataString(userInfo[1])};" +
           "Ssl Mode=Require";
}

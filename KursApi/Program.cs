using KursApi.Data;
using KursApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// ── Oracle — Entity Framework Core ──────────────────────────────────────────
builder.Services.AddDbContext<KursDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleKurs")));

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
builder.Services.AddCors(options =>
{
    options.AddPolicy("KursFrontend", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7090",    // Blazor WASM (HTTPS dev)
                "http://localhost:5090",     // Blazor WASM (HTTP dev)
                "http://127.0.0.1:3000",     // HTML frontend (Live Server)
                "http://localhost:3000",     // HTML frontend (Live Server alt)
                "http://127.0.0.1:5500",     // HTML frontend (VS Code Live Server)
                "http://localhost:5500"      // HTML frontend (VS Code Live Server alt)
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// ── Pipeline HTTP ────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
    app.MapOpenApi();           // Documentación en /openapi/v1.json

app.UseHttpsRedirection();
app.UseCors("KursFrontend");
app.UseRateLimiter();
app.UseAuthentication();        // ← ANTES de UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();

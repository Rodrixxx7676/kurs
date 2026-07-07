using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using KursFront;
using KursFront.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── HttpClient apuntando a KursApi ──────────────────────────────────────────
// En desarrollo la URL viene de wwwroot/appsettings.Development.json; en
// producción el frontend se sirve desde la propia API, así que usa su origen.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// ── Servicios de la app ──────────────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();

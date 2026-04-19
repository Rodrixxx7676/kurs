using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using KursFront;
using KursFront.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── HttpClient apuntando a KursApi ──────────────────────────────────────────
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5212/")
});

// ── Servicios de la app ──────────────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();

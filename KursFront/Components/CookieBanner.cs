using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KursFront.Components;

/// <summary>
/// Banner de política de cookies.
/// Aparece solo al ingresar — se guarda en localStorage.
/// Si el usuario ya aceptó/rechazó, no vuelve a aparecer.
/// </summary>
public partial class CookieBanner : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private bool _visible = false;
    private bool _show    = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        // Chequear localStorage — si ya decidió, no mostrar
        var decision = await JS.InvokeAsync<string?>("localStorage.getItem", "kurs_cookies");
        if (!string.IsNullOrEmpty(decision)) return;

        _visible = true;
        StateHasChanged();

        // Pequeño delay para la animación de entrada
        await Task.Delay(800);
        _show = true;
        StateHasChanged();
    }

    private async Task Aceptar()
    {
        await JS.InvokeVoidAsync("localStorage.setItem", "kurs_cookies", "accepted");
        _show    = false;
        await Task.Delay(450);
        _visible = false;
        StateHasChanged();
    }

    private async Task Rechazar()
    {
        await JS.InvokeVoidAsync("localStorage.setItem", "kurs_cookies", "rejected");
        _show    = false;
        await Task.Delay(450);
        _visible = false;
        StateHasChanged();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

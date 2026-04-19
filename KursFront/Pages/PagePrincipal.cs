using Microsoft.AspNetCore.Components;

namespace KursFront.Pages;

/// <summary>
/// Code-behind de la página principal de KURS.
/// Toda la lógica C# vive aquí; el markup está en PagePrincipal.razor.
/// </summary>
public partial class PagePrincipal : ComponentBase, IDisposable
{
    // ── Valores actuales de los contadores ──
    private int _projects     = 0;
    private int _satisfaction = 0;
    private int _years        = 0;
    private int _experts      = 0;

    // ── Metas finales ──
    private const int TargetProjects     = 200;
    private const int TargetSatisfaction = 98;
    private const int TargetYears        = 15;
    private const int TargetExperts      = 50;

    // ── Control del timer ──
    private System.Threading.Timer? _timer;
    private int   _step       = 0;
    private const int TotalSteps = 80;   // fotogramas de la animación
    private const int FrameMs   = 18;    // ~55 fps
    private const int StartDelay = 600;  // ms antes de empezar (página ya renderizada)

    /// <summary>
    /// Arranca el contador animado en el primer render (sin JS).
    /// Usa ease-out cúbico para una desaceleración natural.
    /// </summary>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        _timer = new System.Threading.Timer(_ =>
        {
            _step++;

            // Ease-out cúbico: f(t) = 1 - (1-t)³
            double t    = (double)_step / TotalSteps;
            double ease = 1 - Math.Pow(1 - t, 3);

            _projects     = (int)(TargetProjects     * ease);
            _satisfaction = (int)(TargetSatisfaction * ease);
            _years        = (int)(TargetYears        * ease);
            _experts      = (int)(TargetExperts      * ease);

            // Forzar re-render en el hilo de Blazor
            InvokeAsync(StateHasChanged);

            // Detener el timer al llegar al final
            if (_step >= TotalSteps)
                _timer?.Change(System.Threading.Timeout.Infinite,
                               System.Threading.Timeout.Infinite);
        },
        state:   null,
        dueTime: StartDelay,
        period:  FrameMs);
    }

    public void Dispose() => _timer?.Dispose();
}

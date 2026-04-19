using KursFront.Services;
using Microsoft.AspNetCore.Components;

namespace KursFront.Pages;

/// <summary>
/// Code-behind de la página de inicio de sesión.
/// Llama a POST /api/auth/login y guarda el JWT en localStorage.
/// </summary>
public partial class Login : ComponentBase
{
    // ── Campos del formulario ──
    private string _email    = string.Empty;
    private string _password = string.Empty;

    // ── Estado de la UI ──
    private bool   _mostrarPw = false;
    private bool   _cargando  = false;
    private string _error     = string.Empty;

    [Inject] private NavigationManager Nav     { get; set; } = default!;
    [Inject] private AuthService       AuthSvc { get; set; } = default!;

    private async Task Ingresar()
    {
        _error = string.Empty;

        // ── Validación básica ────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(_email) ||
            !_email.Contains('@') || !_email.Contains('.'))
        {
            _error = "Ingresa un correo electrónico válido.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_password) || _password.Length < 6)
        {
            _error = "La contraseña debe tener al menos 6 caracteres.";
            return;
        }

        // ── Llamada al API ───────────────────────────────────────────────────
        _cargando = true;
        var (ok, error) = await AuthSvc.LoginAsync(_email.Trim(), _password);
        _cargando = false;

        if (ok)
            Nav.NavigateTo("/");
        else
            _error = error;
    }
}

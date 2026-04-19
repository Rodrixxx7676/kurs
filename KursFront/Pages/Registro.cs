using KursFront.Services;
using Microsoft.AspNetCore.Components;

namespace KursFront.Pages;

/// <summary>
/// Code-behind de la página de registro.
/// Llama a POST /api/auth/registro (con contraseña hasheada en el backend).
/// </summary>
public partial class Registro : ComponentBase
{
    // ── Campos del formulario ──
    private string _nombre   = string.Empty;
    private string _empresa  = string.Empty;
    private string _email    = string.Empty;
    private string _password = string.Empty;
    private string _confirm  = string.Empty;

    // ── Estado de la UI ──
    private bool   _mostrarPw  = false;
    private bool   _enviando   = false;
    private bool   _registrado = false;
    private string _error      = string.Empty;

    // ── Reglas de contraseña ──
    private bool PwTieneMayuscula => _password.Any(char.IsUpper);
    private bool PwTieneNumero    => _password.Any(char.IsDigit);
    private bool PwTieneEspecial  => _password.Any(c => "!@#$%^&*()_+-=[]{}|;':\",./<>?".Contains(c));
    private bool PwEsSegura       => _password.Length >= 7 && PwTieneMayuscula && PwTieneNumero && PwTieneEspecial;

    [Inject] private NavigationManager Nav     { get; set; } = default!;
    [Inject] private AuthService       AuthSvc { get; set; } = default!;

    private async Task Registrar()
    {
        _error = string.Empty;

        // ── Validación ──────────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(_nombre))
        { _error = "Ingresa tu nombre completo."; return; }

        if (string.IsNullOrWhiteSpace(_email) ||
            !_email.Contains('@') || !_email.Contains('.'))
        { _error = "Ingresa un correo electrónico válido."; return; }

        if (!PwEsSegura)
        { _error = "La contraseña debe tener mínimo 7 caracteres, una mayúscula, un número y un carácter especial."; return; }

        if (_password != _confirm)
        { _error = "Las contraseñas no coinciden."; return; }

        // ── Llamada al API → POST /api/auth/registro ─────────────────────────
        _enviando = true;
        var (ok, error) = await AuthSvc.RegistroAsync(
            _nombre.Trim(),
            _email.Trim(),
            _password,
            string.IsNullOrWhiteSpace(_empresa) ? null : _empresa.Trim()
        );
        _enviando = false;

        if (ok)
        {
            _registrado = true;
            await Task.Delay(1500);      // Muestra el check animado 1.5 seg
            Nav.NavigateTo("/login");
        }
        else
        {
            _error = error;
        }
    }
}

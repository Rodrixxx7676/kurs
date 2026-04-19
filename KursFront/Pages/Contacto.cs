using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace KursFront.Pages;

/// <summary>
/// Code-behind de la página de contacto.
/// Envía el formulario al API (POST /api/contacto) y guarda en Oracle.
/// </summary>
public partial class Contacto : ComponentBase
{
    // ── Campos del formulario ──
    private string _nombre  = string.Empty;
    private string _email   = string.Empty;
    private string _asunto  = string.Empty;
    private string _mensaje = string.Empty;

    // ── Estado de la UI ──
    private bool   _enviado  = false;
    private bool   _enviando = false;   // muestra spinner mientras espera
    private string _error    = string.Empty;

    [Inject] private HttpClient Http { get; set; } = default!;

    /// <summary>
    /// Valida, llama al API y guarda el mensaje en Oracle.
    /// </summary>
    private async Task Enviar()
    {
        _error = string.Empty;

        // ── Validación ──────────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(_nombre)  ||
            string.IsNullOrWhiteSpace(_email)   ||
            string.IsNullOrWhiteSpace(_asunto)  ||
            string.IsNullOrWhiteSpace(_mensaje))
        {
            _error = "Por favor completa todos los campos antes de enviar.";
            return;
        }

        if (!_email.Contains('@') || !_email.Contains('.'))
        {
            _error = "Ingresa un correo electrónico válido.";
            return;
        }

        // ── Llamada al API ──────────────────────────────────────────────────
        _enviando = true;

        try
        {
            var payload = new
            {
                nombre  = _nombre,
                email   = _email,
                asunto  = _asunto,
                mensaje = _mensaje
            };

            var response = await Http.PostAsJsonAsync("api/contacto", payload);

            if (response.IsSuccessStatusCode)
            {
                _enviado = true;
            }
            else
            {
                _error = "Ocurrió un problema al enviar. Inténtalo de nuevo.";
            }
        }
        catch
        {
            _error = "No se pudo conectar con el servidor. Revisa tu conexión.";
        }
        finally
        {
            _enviando = false;
        }
    }
}

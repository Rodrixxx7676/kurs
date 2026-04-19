using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace KursFront.Pages;

/// <summary>
/// Code-behind del Portal de Proveedores.
/// Envía la solicitud al API → POST /api/proveedores (pendiente de implementar).
/// </summary>
public partial class Proveedores : ComponentBase
{
    // ── Campos ──
    private string _razonSocial   = string.Empty;
    private string _ruc           = string.Empty;
    private string _representante = string.Empty;
    private string _email         = string.Empty;
    private string _telefono      = string.Empty;
    private string _categoria     = string.Empty;
    private string _descripcion   = string.Empty;
    private string _web           = string.Empty;

    // ── Estado ──
    private bool   _enviando = false;
    private bool   _enviado  = false;
    private string _error    = string.Empty;

    [Inject] private HttpClient Http { get; set; } = default!;

    private async Task Enviar()
    {
        _error = string.Empty;

        if (string.IsNullOrWhiteSpace(_razonSocial))
        { _error = "Ingresa la razón social."; return; }

        if (string.IsNullOrWhiteSpace(_ruc) || _ruc.Trim().Length != 11)
        { _error = "El RUC debe tener 11 dígitos."; return; }

        if (string.IsNullOrWhiteSpace(_representante))
        { _error = "Ingresa el nombre del representante legal."; return; }

        if (string.IsNullOrWhiteSpace(_email) ||
            !_email.Contains('@') || !_email.Contains('.'))
        { _error = "Ingresa un correo corporativo válido."; return; }

        if (string.IsNullOrWhiteSpace(_telefono))
        { _error = "Ingresa un teléfono de contacto."; return; }

        if (string.IsNullOrWhiteSpace(_categoria))
        { _error = "Selecciona una categoría."; return; }

        _enviando = true;

        try
        {
            var payload = new
            {
                razonSocial   = _razonSocial.Trim(),
                ruc           = _ruc.Trim(),
                representante = _representante.Trim(),
                email         = _email.Trim(),
                telefono      = _telefono.Trim(),
                categoria     = _categoria,
                descripcion   = _descripcion.Trim(),
                web           = _web.Trim()
            };

            // TODO: implementar endpoint /api/proveedores en KursApi
            // var response = await Http.PostAsJsonAsync("api/proveedores", payload);
            // if (!response.IsSuccessStatusCode) { _error = "Error al enviar."; return; }

            await Task.Delay(800); // simula la llamada
            _enviado = true;
        }
        catch
        {
            _error = "No se pudo conectar con el servidor. Inténtalo de nuevo.";
        }
        finally
        {
            _enviando = false;
        }
    }
}

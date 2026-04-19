using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KursFront.Services;

/// <summary>
/// Servicio de autenticación para Blazor WASM.
/// Llama a /api/auth/* y guarda el JWT en localStorage del navegador.
/// </summary>
public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    private const string TokenKey      = "kurs_token";
    private const string UserKey       = "kurs_user";
    private const string ExpiraKey     = "kurs_expira";
    private const string ActividadKey  = "kurs_actividad";
    private const int    InactivoMinutos = 30;   // cerrar sesión tras 30 min sin actividad

    public AuthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js   = js;
    }

    // ── Registro ─────────────────────────────────────────────────────────────
    /// <summary>Crea una cuenta nueva. Devuelve (true, "") o (false, mensaje de error).</summary>
    public async Task<(bool Ok, string Error)> RegistroAsync(
        string nombre, string email, string password, string? empresa)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/registro",
                new { nombre, email, password, empresa });

            if (resp.IsSuccessStatusCode)
                return (true, string.Empty);

            if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
                return (false, "Ya existe una cuenta con ese correo.");

            return (false, "Error al crear la cuenta. Inténtalo de nuevo.");
        }
        catch
        {
            return (false, "No se pudo conectar con el servidor.");
        }
    }

    // ── Login ─────────────────────────────────────────────────────────────────
    /// <summary>Inicia sesión. Si tiene éxito guarda el JWT en localStorage.</summary>
    public async Task<(bool Ok, string Error)> LoginAsync(string email, string password)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/login",
                new { email, password });

            if (resp.IsSuccessStatusCode)
            {
                var data = await resp.Content.ReadFromJsonAsync<TokenResponse>();
                if (data is not null)
                {
                    // Guardar token, expiración y datos del usuario en localStorage
                    await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, data.Token);
                    await _js.InvokeVoidAsync("localStorage.setItem", ExpiraKey, data.Expira.ToString("O"));
                    await _js.InvokeVoidAsync("localStorage.setItem", UserKey,
                        JsonSerializer.Serialize(new UsuarioInfo(data.Nombre, data.Email, data.Nivel)));
                    return (true, string.Empty);
                }
            }

            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (false, "Correo o contraseña incorrectos.");

            return (false, "Error al iniciar sesión. Inténtalo de nuevo.");
        }
        catch
        {
            return (false, "No se pudo conectar con el servidor.");
        }
    }

    // ── Logout ────────────────────────────────────────────────────────────────
    /// <summary>Elimina el JWT y los datos de sesión del navegador.</summary>
    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", UserKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", ExpiraKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", ActividadKey);
    }

    // ── Estado de sesión ──────────────────────────────────────────────────────
    /// <summary>
    /// True si hay un token válido y la sesión no ha expirado por tiempo o inactividad.
    /// Cierra sesión automáticamente si detecta expiración.
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        if (string.IsNullOrEmpty(token)) return false;

        // Verificar expiración del JWT (2 horas desde el login)
        var expiraStr = await _js.InvokeAsync<string?>("localStorage.getItem", ExpiraKey);
        if (expiraStr is not null && DateTime.TryParse(expiraStr, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out var expira))
        {
            if (DateTime.UtcNow >= expira)
            {
                await LogoutAsync();
                return false;
            }
        }

        // Verificar inactividad (30 minutos sin interacción)
        var actStr = await _js.InvokeAsync<string?>("localStorage.getItem", ActividadKey);
        if (actStr is not null && long.TryParse(actStr, out var actMs))
        {
            var ultimaActividad = DateTimeOffset.FromUnixTimeMilliseconds(actMs).UtcDateTime;
            if ((DateTime.UtcNow - ultimaActividad).TotalMinutes > InactivoMinutos)
            {
                await LogoutAsync();
                return false;
            }
        }

        return true;
    }

    /// <summary>Devuelve el token JWT actual (o null si no hay sesión).</summary>
    public async Task<string?> GetTokenAsync()
        => await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);

    /// <summary>Devuelve nombre y email del usuario en sesión (o null).</summary>
    public async Task<UsuarioInfo?> GetUsuarioAsync()
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", UserKey);
        if (string.IsNullOrEmpty(json)) return null;
        return JsonSerializer.Deserialize<UsuarioInfo>(json);
    }

    // ── Modelos internos ──────────────────────────────────────────────────────
    private class TokenResponse
    {
        [JsonPropertyName("token")]  public string   Token  { get; set; } = string.Empty;
        [JsonPropertyName("expira")] public DateTime Expira { get; set; }
        [JsonPropertyName("nombre")] public string   Nombre { get; set; } = string.Empty;
        [JsonPropertyName("email")]  public string   Email  { get; set; } = string.Empty;
        [JsonPropertyName("nivel")]  public int      Nivel  { get; set; }
    }
}

/// <summary>Info básica del usuario autenticado, guardada en localStorage.</summary>
public record UsuarioInfo(string Nombre, string Email, int Nivel);

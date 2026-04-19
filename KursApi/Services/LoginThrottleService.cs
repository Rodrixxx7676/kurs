using System.Collections.Concurrent;

namespace KursApi.Services;

/// <summary>
/// Rastrea intentos fallidos de login en memoria.
/// Bloquea la cuenta 15 minutos después de 5 intentos fallidos consecutivos.
/// </summary>
public class LoginThrottleService
{
    private readonly ConcurrentDictionary<string, (int Intentos, DateTime? BloqueadoHasta)> _cache = new();

    private const int MaxIntentos = 5;
    private static readonly TimeSpan TiempoBloqueo = TimeSpan.FromMinutes(15);

    /// <summary>Indica si el email está bloqueado actualmente.</summary>
    public bool EstaBloqueado(string email)
    {
        if (_cache.TryGetValue(email, out var estado) &&
            estado.BloqueadoHasta.HasValue &&
            DateTime.UtcNow < estado.BloqueadoHasta.Value)
            return true;

        return false;
    }

    /// <summary>Devuelve hasta cuándo está bloqueado (o null si no lo está).</summary>
    public DateTime? GetBloqueadoHasta(string email) =>
        _cache.TryGetValue(email, out var e) ? e.BloqueadoHasta : null;

    /// <summary>Registra un intento fallido. Al llegar a 5 bloquea la cuenta.</summary>
    public void RegistrarFallo(string email)
    {
        _cache.AddOrUpdate(
            email,
            _ => (1, null),
            (_, prev) =>
            {
                var intentos  = prev.Intentos + 1;
                var bloqueado = intentos >= MaxIntentos
                    ? DateTime.UtcNow.Add(TiempoBloqueo)
                    : (DateTime?)null;
                return (intentos, bloqueado);
            });
    }

    /// <summary>Limpia el contador tras un login exitoso.</summary>
    public void ResetearIntentos(string email) => _cache.TryRemove(email, out _);
}

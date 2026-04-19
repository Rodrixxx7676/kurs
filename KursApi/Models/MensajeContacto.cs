using System.ComponentModel.DataAnnotations;

namespace KursApi.Models;

/// <summary>
/// Almacena los mensajes enviados a través del formulario de contacto.
/// Tabla Oracle: MENSAJES_CONTACTO
/// </summary>
public class MensajeContacto
{
    public long Id { get; set; }

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(320), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Asunto { get; set; } = string.Empty;

    [Required, MaxLength(4000)]
    public string Mensaje { get; set; } = string.Empty;

    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    public bool Leido { get; set; } = false;
}

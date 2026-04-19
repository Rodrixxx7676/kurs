using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KursApi.Models;

/// <summary>
/// Representa un cliente registrado en la plataforma KURS.
/// Tabla Oracle: CLIENTES
/// </summary>
public class Cliente
{
    public long Id { get; set; }

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(320), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Empresa { get; set; }

    [MaxLength(30)]
    public string? Telefono { get; set; }

    /// <summary>Hash BCrypt de la contraseña. Nunca se expone en respuestas.</summary>
    [JsonIgnore]
    [MaxLength(500)]
    public string? PasswordHash { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;

    /// <summary>Nivel de acceso: 1=Visitante, 2=Cliente, 3=Colaborador, 4=Administrador.</summary>
    public int Nivel { get; set; } = 2;
}

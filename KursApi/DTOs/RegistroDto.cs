using System.ComponentModel.DataAnnotations;

namespace KursApi.DTOs;

/// <summary>Datos que llegan del formulario de registro.</summary>
public class RegistroDto
{
    [Required(ErrorMessage = "El nombre es obligatorio."), MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio."), MaxLength(320), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Empresa { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria."), MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

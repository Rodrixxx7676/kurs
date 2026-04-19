using System.ComponentModel.DataAnnotations;

namespace KursApi.DTOs;

/// <summary>Credenciales de inicio de sesión.</summary>
public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

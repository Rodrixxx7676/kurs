namespace KursApi.DTOs;

/// <summary>Respuesta devuelta al hacer login exitoso.</summary>
public class TokenResponseDto
{
    public string   Token  { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
    public string   Nombre { get; set; } = string.Empty;
    public string   Email  { get; set; } = string.Empty;
    public int      Nivel  { get; set; }
}

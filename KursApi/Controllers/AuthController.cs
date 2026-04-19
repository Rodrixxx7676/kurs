using KursApi.Data;
using KursApi.DTOs;
using KursApi.Models;
using KursApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KursApi.Controllers;

/// <summary>
/// Autenticación: registro y login con contraseña hasheada + JWT.
/// POST /api/auth/registro
/// POST /api/auth/login
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly KursDbContext       _db;
    private readonly IConfiguration      _config;
    private readonly LoginThrottleService _throttle;

    public AuthController(KursDbContext db, IConfiguration config, LoginThrottleService throttle)
    {
        _db       = db;
        _config   = config;
        _throttle = throttle;
    }

    // ── POST /api/auth/registro ──────────────────────────────────────────────
    /// <summary>
    /// Crea una nueva cuenta.
    /// La contraseña se hashea con BCrypt antes de guardarse en Oracle.
    /// </summary>
    [HttpPost("registro")]
    public async Task<IActionResult> Registro([FromBody] RegistroDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Verificar email único (insensible a mayúsculas)
        var emailNorm = dto.Email.Trim().ToLower();
        bool existe = await _db.Clientes.AnyAsync(c => c.Email == emailNorm);
        if (existe)
            return Conflict(new { mensaje = "Ya existe una cuenta con ese correo electrónico." });

        var cliente = new Cliente
        {
            Nombre        = dto.Nombre.Trim(),
            Email         = emailNorm,
            Empresa       = string.IsNullOrWhiteSpace(dto.Empresa) ? null : dto.Empresa.Trim(),
            PasswordHash  = BCrypt.Net.BCrypt.HashPassword(dto.Password),  // 🔐 hash seguro
            FechaRegistro = DateTime.UtcNow,
            Activo        = true
        };

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Cuenta creada correctamente.", id = cliente.Id });
    }

    // ── POST /api/auth/login ─────────────────────────────────────────────────
    /// <summary>
    /// Valida credenciales y devuelve un JWT si son correctas.
    /// - Rate limit: 5 peticiones por minuto por IP.
    /// - Bloqueo de cuenta: 15 minutos tras 5 intentos fallidos.
    /// - Token expira en 2 horas (configurable en appsettings.json).
    /// </summary>
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var emailNorm = dto.Email.Trim().ToLower();

        // Verificar bloqueo por intentos fallidos
        if (_throttle.EstaBloqueado(emailNorm))
        {
            var hasta     = _throttle.GetBloqueadoHasta(emailNorm)!.Value;
            var restantes = (int)Math.Ceiling((hasta - DateTime.UtcNow).TotalMinutes);
            return StatusCode(429, new
            {
                mensaje = $"Cuenta bloqueada temporalmente. Intenta de nuevo en {restantes} minuto(s)."
            });
        }

        var cliente = await _db.Clientes
            .FirstOrDefaultAsync(c => c.Email == emailNorm && c.Activo);

        // Mensaje genérico → no revelar si el email existe o no
        if (cliente is null || cliente.PasswordHash is null ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, cliente.PasswordHash))
        {
            _throttle.RegistrarFallo(emailNorm);
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
        }

        // Login exitoso → limpiar contador
        _throttle.ResetearIntentos(emailNorm);

        var horas  = _config.GetValue<int>("Jwt:ExpiresHours", 2);
        var expira = DateTime.UtcNow.AddHours(horas);
        var token  = GenerarJwt(cliente, expira);

        return Ok(new TokenResponseDto
        {
            Token  = token,
            Expira = expira,
            Nombre = cliente.Nombre,
            Email  = cliente.Email,
            Nivel  = cliente.Nivel
        });
    }

    // ── Generador de JWT ─────────────────────────────────────────────────────
    private string GenerarJwt(Cliente cliente, DateTime expira)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   cliente.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, cliente.Email),
            new Claim(ClaimTypes.Name,               cliente.Nombre),
            new Claim("nivel",                        cliente.Nivel.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:            _config["Jwt:Issuer"],
            audience:          _config["Jwt:Audience"],
            claims:            claims,
            expires:           expira,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

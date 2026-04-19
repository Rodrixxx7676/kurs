using KursApi.Data;
using KursApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KursApi.Controllers;

/// <summary>
/// Gestiona los mensajes enviados desde el formulario de contacto.
/// POST /api/contacto      → guarda un mensaje nuevo en Oracle
/// GET  /api/contacto      → lista todos (para uso administrativo)
/// PUT  /api/contacto/{id}/leido → marca como leído
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ContactoController : ControllerBase
{
    private readonly KursDbContext _db;

    public ContactoController(KursDbContext db) => _db = db;

    // ── POST /api/contacto ───────────────────────────────────────────────────
    /// <summary>
    /// Recibe el formulario de contacto del frontend y lo persiste en Oracle.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Enviar([FromBody] MensajeContacto mensaje)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        mensaje.Id        = 0;
        mensaje.FechaEnvio = DateTime.UtcNow;
        mensaje.Leido     = false;

        _db.MensajesContacto.Add(mensaje);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Mensaje recibido. ¡Nos pondremos en contacto pronto!" });
    }

    // ── GET /api/contacto ────────────────────────────────────────────────────
    /// <summary>Lista todos los mensajes ordenados del más reciente al más antiguo.</summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMensajes()
    {
        var mensajes = await _db.MensajesContacto
            .OrderByDescending(m => m.FechaEnvio)
            .ToListAsync();

        return Ok(mensajes);
    }

    // ── PUT /api/contacto/{id}/leido ─────────────────────────────────────────
    /// <summary>Marca un mensaje como leído.</summary>
    [Authorize]
    [HttpPut("{id:long}/leido")]
    public async Task<IActionResult> MarcarLeido(long id)
    {
        var msg = await _db.MensajesContacto.FindAsync(id);
        if (msg is null)
            return NotFound(new { mensaje = "Mensaje no encontrado." });

        msg.Leido = true;
        await _db.SaveChangesAsync();
        return Ok(msg);
    }
}

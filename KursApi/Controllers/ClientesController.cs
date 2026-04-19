using KursApi.Data;
using KursApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KursApi.Controllers;

/// <summary>
/// CRUD de clientes.
/// Endpoints: GET /api/clientes, POST /api/clientes, PUT /api/clientes/{id}, DELETE /api/clientes/{id}
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly KursDbContext _db;

    public ClientesController(KursDbContext db) => _db = db;

    // ── GET /api/clientes ────────────────────────────────────────────────────
    /// <summary>Retorna todos los clientes activos.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var lista = await _db.Clientes
            .Where(c => c.Activo)
            .OrderByDescending(c => c.FechaRegistro)
            .ToListAsync();

        return Ok(lista);
    }

    // ── GET /api/clientes/{id} ───────────────────────────────────────────────
    /// <summary>Retorna un cliente por su ID.</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        return cliente is null ? NotFound(new { mensaje = "Cliente no encontrado." }) : Ok(cliente);
    }

    // ── POST /api/clientes ───────────────────────────────────────────────────
    /// <summary>Registra un nuevo cliente.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] Cliente cliente)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Verificar email único
        bool existe = await _db.Clientes.AnyAsync(c => c.Email == cliente.Email);
        if (existe)
            return Conflict(new { mensaje = "Ya existe un cliente con ese correo electrónico." });

        cliente.Id = 0;                           // EF asigna el Id via secuencia
        cliente.FechaRegistro = DateTime.UtcNow;
        cliente.Activo = true;

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
    }

    // ── PUT /api/clientes/{id} ───────────────────────────────────────────────
    /// <summary>Actualiza los datos de un cliente existente.</summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(long id, [FromBody] Cliente datos)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null)
            return NotFound(new { mensaje = "Cliente no encontrado." });

        // Email único (excluyendo el mismo registro)
        bool emailEnUso = await _db.Clientes
            .AnyAsync(c => c.Email == datos.Email && c.Id != id);
        if (emailEnUso)
            return Conflict(new { mensaje = "Ese correo ya está en uso por otro cliente." });

        cliente.Nombre   = datos.Nombre;
        cliente.Email    = datos.Email;
        cliente.Empresa  = datos.Empresa;
        cliente.Telefono = datos.Telefono;
        cliente.Activo   = datos.Activo;

        await _db.SaveChangesAsync();
        return Ok(cliente);
    }

    // ── DELETE /api/clientes/{id} ────────────────────────────────────────────
    /// <summary>Baja lógica (Activo = false). No elimina el registro de Oracle.</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null)
            return NotFound(new { mensaje = "Cliente no encontrado." });

        cliente.Activo = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

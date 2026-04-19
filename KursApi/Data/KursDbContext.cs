using KursApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KursApi.Data;

/// <summary>
/// Contexto principal de Entity Framework Core con proveedor Oracle.
/// Registra todas las entidades del dominio KURS.
/// </summary>
public class KursDbContext : DbContext
{
    public KursDbContext(DbContextOptions<KursDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<MensajeContacto> MensajesContacto { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── CLIENTES ──────────────────────────────────────────
        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("CLIENTES");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)             .HasColumnName("ID").UseHiLo("SEQ_CLIENTES");
            e.Property(x => x.Nombre)         .HasColumnName("NOMBRE").HasMaxLength(200).IsRequired();
            e.Property(x => x.Email)          .HasColumnName("EMAIL").HasMaxLength(320).IsRequired();
            e.Property(x => x.Empresa)        .HasColumnName("EMPRESA").HasMaxLength(200);
            e.Property(x => x.Telefono)       .HasColumnName("TELEFONO").HasMaxLength(30);
            e.Property(x => x.PasswordHash)   .HasColumnName("PASSWORD_HASH").HasMaxLength(500);
            e.Property(x => x.FechaRegistro)  .HasColumnName("FECHA_REGISTRO").HasColumnType("TIMESTAMP(3)");
            e.Property(x => x.Activo)         .HasColumnName("ACTIVO").HasColumnType("NUMBER(1)").HasConversion<int>();
            e.Property(x => x.Nivel)          .HasColumnName("NIVEL").HasColumnType("NUMBER(2)").HasDefaultValue(2);

            e.HasIndex(x => x.Email).IsUnique();
        });

        // ── MENSAJES_CONTACTO ─────────────────────────────────
        modelBuilder.Entity<MensajeContacto>(e =>
        {
            e.ToTable("MENSAJES_CONTACTO");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)         .HasColumnName("ID").UseHiLo("SEQ_MENSAJES");
            e.Property(x => x.Nombre)     .HasColumnName("NOMBRE").HasMaxLength(200).IsRequired();
            e.Property(x => x.Email)      .HasColumnName("EMAIL").HasMaxLength(320).IsRequired();
            e.Property(x => x.Asunto)     .HasColumnName("ASUNTO").HasMaxLength(300).IsRequired();
            e.Property(x => x.Mensaje)    .HasColumnName("MENSAJE").HasMaxLength(4000).IsRequired();
            e.Property(x => x.FechaEnvio) .HasColumnName("FECHA_ENVIO").HasColumnType("TIMESTAMP(3)");
            e.Property(x => x.Leido)      .HasColumnName("LEIDO").HasColumnType("NUMBER(1)").HasConversion<int>();
        });
    }
}

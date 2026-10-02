// RedVital — Servicio de Campañas
// T-312.1. Configuración EF Core de db_campana.

using Microsoft.EntityFrameworkCore;
using RedVital.Campanas.Dominio;

namespace RedVital.Campanas.Infraestructura.Persistencia;

public class CampanasDbContext : DbContext
{
    public CampanasDbContext(DbContextOptions<CampanasDbContext> options) : base(options) { }

    public DbSet<Campania> Campanias => Set<Campania>();
    public DbSet<ReservaCupo> ReservasCupo => Set<ReservaCupo>();
    public DbSet<RegistroAuditoriaCamp> RegistroAuditoriaCamp => Set<RegistroAuditoriaCamp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Campania>(e =>
        {
            e.ToTable("campania", table =>
            {
                table.HasCheckConstraint("ck_campania_fechas", "termina_en > inicia_en");
                table.HasCheckConstraint("ck_campania_estado",
                    "estado IN ('borrador', 'publicada', 'cerrada', 'cancelada')");
            });
            e.HasKey(c => c.Id);
            e.Property(c => c.InstitucionId).IsRequired();
            e.Property(c => c.TerritorioCodigo).HasMaxLength(5).IsRequired();
            e.Property(c => c.TerritorioRuta).HasMaxLength(32).IsRequired();
            e.Property(c => c.Nombre).HasMaxLength(160).IsRequired();
            e.Property(c => c.Descripcion).HasMaxLength(500);
            e.Property(c => c.Sede).HasMaxLength(200).IsRequired();
            e.Property(c => c.IniciaEn).IsRequired();
            e.Property(c => c.TerminaEn).IsRequired();
            e.Property(c => c.CupoReservado).IsRequired();
            e.Property(c => c.Estado).HasMaxLength(20).IsRequired();
            e.Property(c => c.CreadaPor).IsRequired();
            e.Property(c => c.CreadaEn).IsRequired();
            e.Property(c => c.ActualizadaEn).IsRequired();

            // Índice para el proceso de expiración de reservas (corre cada minuto):
            // necesita filtrar campañas publicadas por fecha con frecuencia.
            e.HasIndex(c => new { c.Estado, c.TerminaEn });
        });

        modelBuilder.Entity<ReservaCupo>(e =>
        {
            e.ToTable("reserva_cupo", table => table.HasCheckConstraint(
                "ck_reserva_estado",
                "estado IN ('pendiente', 'confirmada', 'liberada', 'cancelada')"));
            e.HasKey(r => r.Id);
            e.HasOne(r => r.Campania).WithMany(c => c.Reservas)
                .HasForeignKey(r => r.CampaniaId).OnDelete(DeleteBehavior.Restrict);
            e.Property(r => r.UsuarioId).IsRequired();
            e.Property(r => r.Estado).HasMaxLength(20).IsRequired();
            e.Property(r => r.CreadaEn).IsRequired();
            e.Property(r => r.ExpiraEn).IsRequired();

            // Índice crítico: el proceso de expiración de cada minuto filtra
            // exactamente por esta combinación. Sin este índice, ese proceso
            // hace un recorrido completo de la tabla cada sesenta segundos.
            e.HasIndex(r => new { r.Estado, r.ExpiraEn });
        });

        modelBuilder.Entity<RegistroAuditoriaCamp>(e =>
        {
            e.ToTable("registro_auditoria_camp", table =>
            {
                table.HasCheckConstraint("ck_auditoria_camp_actor_tipo", "actor_tipo IN ('usuario', 'sistema', 'anonimo')");
                table.HasCheckConstraint("ck_auditoria_camp_resultado", "resultado IN ('permitido', 'denegado')");
            });
            e.HasKey(r => r.Id);
            e.Property(r => r.ActorTipo).HasMaxLength(20).IsRequired();
            e.Property(r => r.ActorId).HasMaxLength(64);
            e.Property(r => r.Rol).HasMaxLength(40);
            e.Property(r => r.JurisdiccionSolicitada).HasMaxLength(80);
            e.Property(r => r.Operacion).HasMaxLength(80).IsRequired();
            e.Property(r => r.RecursoTipo).HasMaxLength(40).IsRequired();
            e.Property(r => r.Resultado).HasMaxLength(20).IsRequired();
            e.Property(r => r.CorrelacionId).HasMaxLength(36).IsRequired();
            e.Property(r => r.Origen).HasMaxLength(45);
            e.Property(r => r.OcurridoEn).IsRequired();
            e.HasIndex(r => r.CorrelacionId);
            e.HasIndex(r => r.OcurridoEn);
        });

        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
        {
            entidad.SetTableName(ASnakeCase(entidad.GetTableName()!));
            foreach (var propiedad in entidad.GetProperties())
                propiedad.SetColumnName(ASnakeCase(propiedad.GetColumnName()));
        }
    }

    private static string ASnakeCase(string texto) =>
        string.Concat(texto.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "_" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
}

/// <summary>
/// Ejemplo del patrón de actualización de cupo con bloqueo de fila, para que
/// quien implemente la confirmación de reserva (T-312.2 o posterior) no
/// reintroduzca una condición de carrera. Esto NO es parte de T-312.1 —
/// se deja aquí como referencia porque el índice y el campo que usa sí lo son.
/// </summary>
public static class EjemploPatronCupo
{
    // using var transaccion = await contexto.Database.BeginTransactionAsync();
    // var campania = await contexto.Campanias
    //     .FromSqlInterpolated($"SELECT * FROM campania WHERE id = {campaniaId} FOR UPDATE")
    //     .SingleAsync();
    // if (campania.CupoTotal is not null && campania.CupoReservado >= campania.CupoTotal)
    //     throw new CupoAgotadoException();
    // campania.CupoReservado++;
    // contexto.ReservasCupo.Add(nuevaReserva);
    // await contexto.SaveChangesAsync();
    // await transaccion.CommitAsync();
}

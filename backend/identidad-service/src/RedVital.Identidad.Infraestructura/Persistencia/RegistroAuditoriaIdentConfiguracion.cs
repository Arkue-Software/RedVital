// RedVital — Servicio de Identidad
// T-330.4. Configuración EF Core de registro_auditoria_ident.
//
// Aplicada desde IdentidadDbContext.OnModelCreating.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedVital.Identidad.Dominio;

namespace RedVital.Identidad.Infraestructura.Persistencia;

public class RegistroAuditoriaIdentConfiguracion : IEntityTypeConfiguration<RegistroAuditoriaIdent>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoriaIdent> e)
    {
        e.ToTable("registro_auditoria_ident", table =>
        {
            table.HasCheckConstraint("ck_auditoria_ident_actor_tipo", "actor_tipo IN ('usuario', 'sistema', 'anonimo')");
            table.HasCheckConstraint("ck_auditoria_ident_resultado", "resultado IN ('permitido', 'denegado')");
        });
        e.HasKey(r => r.Id);

        e.Property(r => r.ActorTipo).HasColumnName("actor_tipo").HasMaxLength(20).IsRequired();

        e.Property(r => r.ActorId).HasColumnName("actor_id").HasMaxLength(64);
        e.Property(r => r.Rol).HasColumnName("rol").HasMaxLength(40);
        e.Property(r => r.JurisdiccionSolicitada).HasColumnName("jurisdiccion_solicitada").HasMaxLength(80);

        e.Property(r => r.Operacion).HasColumnName("operacion").HasMaxLength(80).IsRequired();
        e.Property(r => r.RecursoTipo).HasColumnName("recurso_tipo").HasMaxLength(40).IsRequired();
        e.Property(r => r.RecursoId).HasColumnName("recurso_id");

        e.Property(r => r.Resultado).HasColumnName("resultado").HasMaxLength(20).IsRequired();

        e.Property(r => r.CorrelacionId).HasColumnName("correlacion_id").HasMaxLength(36).IsRequired();
        e.Property(r => r.Origen).HasColumnName("origen").HasMaxLength(45);
        e.Property(r => r.OcurridoEn).HasColumnName("ocurrido_en").IsRequired();

        // Índices para los dos patrones de consulta reales de esta tabla:
        // reconstrucción por correlación (une las cuatro series), y la
        // consulta compuesta del auditor ordenada por tiempo.
        e.HasIndex(r => r.CorrelacionId);
        e.HasIndex(r => r.OcurridoEn);

        // No se configura ninguna relación de navegación hacia Usuario: el DD
        // es explícito en que credencial_servicio y la serie de auditoría no
        // tienen relaciones declaradas. ActorId es un texto, no una clave foránea.
    }
}

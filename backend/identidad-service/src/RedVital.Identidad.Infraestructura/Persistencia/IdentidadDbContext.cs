// RedVital — Servicio de Identidad
// T-304.1. Configuración EF Core de db_identidad.
// Convención del proyecto: nombres de tabla y columna en snake_case, igual que el
// diccionario de datos del DD, para que una consulta SQL directa en una incidencia
// se lea igual que la documentación.

using Microsoft.EntityFrameworkCore;
using RedVital.Identidad.Dominio;

namespace RedVital.Identidad.Infraestructura.Persistencia;

public class IdentidadDbContext : DbContext
{
    public IdentidadDbContext(DbContextOptions<IdentidadDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioJurisdiccion> UsuarioJurisdicciones => Set<UsuarioJurisdiccion>();
    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<CredencialServicio> CredencialesServicio => Set<CredencialServicio>();
    public DbSet<RegistroAuditoriaIdent> RegistrosAuditoria => Set<RegistroAuditoriaIdent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---- usuario ----
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuario");
            e.HasKey(u => u.Id);
            e.Property(u => u.Correo).HasMaxLength(160).IsRequired();
            e.HasIndex(u => u.Correo).IsUnique();
            e.Property(u => u.Nombre).HasMaxLength(120);
            e.Property(u => u.CredencialHash).HasMaxLength(200).IsRequired();
            e.Property(u => u.RolCodigo).HasColumnName("rol_id").IsRequired();
            e.HasOne(u => u.Rol).WithMany().HasForeignKey(u => u.RolCodigo).OnDelete(DeleteBehavior.Restrict);
            e.Property(u => u.Activo).IsRequired();
            e.Property(u => u.CreadoEn).IsRequired();
            e.Property(u => u.ActualizadoEn).IsRequired();

            // Nota del DD: usuario NO tiene ninguna referencia al donante. La
            // correspondencia entre una cuenta y un perfil de donante vive solo
            // en db_donacion, en donante.usuario_id. No agregues esa relación aquí.
        });

        // ---- rol (catálogo administrado, Tabla 13 del DD) ----
        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("rol");
            e.HasKey(r => r.Codigo);
            e.Property(r => r.Codigo).HasMaxLength(20);
            e.Property(r => r.Perfil).HasMaxLength(80).IsRequired();
            e.Property(r => r.AmbitoAdmitido).HasMaxLength(80).IsRequired();

            // Semilla con los seis roles exactos del DD. El rol "servicio" de los
            // tokens de servicio NO se siembra aquí — no es una cuenta.
            e.HasData(
                new Rol { Codigo = "donante", Perfil = "U2 — Donante registrado", AmbitoAdmitido = "ninguno" },
                new Rol { Codigo = "operador", Perfil = "U3 — Operador de banco", AmbitoAdmitido = "institucion (exactamente una)" },
                new Rol { Codigo = "admin_banco", Perfil = "U4 — Administrador de banco", AmbitoAdmitido = "institucion (exactamente una)" },
                new Rol { Codigo = "coordinador", Perfil = "U5 — Coordinador territorial", AmbitoAdmitido = "territorio departamental o municipal, uno o varios" },
                new Rol { Codigo = "admin_nacional", Perfil = "U6 — Administrador nacional", AmbitoAdmitido = "territorio nacional" },
                new Rol { Codigo = "auditor", Perfil = "U7 — Auditor", AmbitoAdmitido = "territorio nacional, lectura limitada" }
            );
        });

        // ---- usuario_jurisdiccion ----
        modelBuilder.Entity<UsuarioJurisdiccion>(e =>
        {
            e.ToTable("usuario_jurisdiccion", table =>
                table.HasCheckConstraint("ck_usuario_jurisdiccion_ambito", "ambito IN ('territorio', 'institucion')"));
            e.HasKey(j => j.Id);
            e.HasOne(j => j.Usuario).WithMany(u => u.Jurisdicciones)
                .HasForeignKey(j => j.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            e.Property(j => j.Ambito).HasMaxLength(20).IsRequired();
            e.Property(j => j.TerritorioCodigo).HasMaxLength(5);
            e.Property(j => j.TerritorioRuta).HasMaxLength(32);
            // institucion_id es una referencia SIN clave foránea declarada: la
            // institución vive en el Servicio Institucional, en otra base. El DD
            // lo trata como "ref." (referencia lógica), no "FK" (clave foránea real).
            e.Property(j => j.AsignadaPor).IsRequired();
            e.Property(j => j.VigenteDesde).IsRequired();
        });

        // ---- sesion ----
        modelBuilder.Entity<Sesion>(e =>
        {
            e.ToTable("sesion", table => table.HasCheckConstraint(
                "ck_sesion_motivo_revocacion",
                "motivo_revocacion IS NULL OR motivo_revocacion IN " +
                "('cierre', 'desactivacion', 'cambio_rol', 'cambio_jurisdiccion', 'reutilizacion', 'rotacion_emergencia')"));
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Usuario).WithMany(u => u.Sesiones)
                .HasForeignKey(s => s.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            e.Property(s => s.SecretoHash).HasMaxLength(200).IsRequired();
            e.Property(s => s.EmitidaEn).IsRequired();
            e.Property(s => s.ExpiraEn).IsRequired();
            e.Property(s => s.MotivoRevocacion).HasMaxLength(30);
            e.Property(s => s.Origen).HasMaxLength(45);

            // Índice para el proceso de detección de reutilización: busca por Id
            // (ya es PK) y compara contra secreto_hash — no hace falta índice extra.
        });

        // ---- credencial_servicio ----
        modelBuilder.Entity<CredencialServicio>(e =>
        {
            e.ToTable("credencial_servicio", table => table.HasCheckConstraint(
                "ck_credencial_servicio_cliente",
                "cliente IN ('gateway', 'donacion', 'notificaciones')"));
            e.HasKey(c => c.Id);
            e.Property(c => c.Cliente).HasMaxLength(40).IsRequired();
            e.HasIndex(c => c.Cliente).IsUnique();
            e.Property(c => c.SecretoHash).HasMaxLength(200).IsRequired();
            e.Property(c => c.Activa).IsRequired();
            e.Property(c => c.CreadaEn).IsRequired();
        });

        modelBuilder.ApplyConfiguration(new RegistroAuditoriaIdentConfiguracion());

        // Convención del proyecto: todas las columnas y tablas en snake_case.
        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
        {
            entidad.SetTableName(ASnakeCase(entidad.GetTableName()!));
            foreach (var propiedad in entidad.GetProperties())
                propiedad.SetColumnName(ASnakeCase(propiedad.GetColumnName()));
        }
    }

    private static string ASnakeCase(string texto)
    {
        // Las entidades ya usan PascalCase en C# y los nombres de tabla/columna del
        // diccionario de datos son snake_case; esta función solo cubre los casos que
        // no se nombraron explícitamente arriba con ToTable/HasColumnName.
        return string.Concat(texto.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "_" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
    }
}

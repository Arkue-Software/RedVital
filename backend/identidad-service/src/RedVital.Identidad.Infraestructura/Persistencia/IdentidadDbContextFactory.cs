using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RedVital.Identidad.Infraestructura.Persistencia;

public sealed class IdentidadDbContextFactory : IDesignTimeDbContextFactory<IdentidadDbContext>
{
    private const string ConnectionStringEnvironmentVariable =
        "REDVITAL_IDENTIDAD_MIGRATIONS_CONNECTION";

    public IdentidadDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable)
            ?? throw new InvalidOperationException(
                $"Configure la variable de entorno {ConnectionStringEnvironmentVariable} para ejecutar migraciones.");

        var options = new DbContextOptionsBuilder<IdentidadDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new IdentidadDbContext(options);
    }
}

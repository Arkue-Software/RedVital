using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RedVital.Campanas.Infraestructura.Persistencia;

public sealed class CampanasDbContextFactory : IDesignTimeDbContextFactory<CampanasDbContext>
{
    private const string ConnectionStringEnvironmentVariable =
        "REDVITAL_CAMPANAS_MIGRATIONS_CONNECTION";

    public CampanasDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable)
            ?? throw new InvalidOperationException(
                $"Configure la variable de entorno {ConnectionStringEnvironmentVariable} para ejecutar migraciones.");

        var options = new DbContextOptionsBuilder<CampanasDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new CampanasDbContext(options);
    }
}

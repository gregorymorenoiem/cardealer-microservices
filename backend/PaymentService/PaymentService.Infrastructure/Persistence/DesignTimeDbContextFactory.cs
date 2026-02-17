using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PaymentService.Infrastructure.Persistence;

/// <summary>
/// Factory para crear AzulDbContext en tiempo de diseño (migraciones)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AzulDbContext>
{
    public AzulDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("../PaymentService.Api/appsettings.json", optional: true)
            .AddJsonFile("../PaymentService.Api/appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AzulDbContext>();

        // Por defecto usar PostgreSQL para migraciones
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=paymentservice_dev;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsHistoryTable("__EFMigrationsHistory", "public");
        });

        return new AzulDbContext(optionsBuilder.Options);
    }
}

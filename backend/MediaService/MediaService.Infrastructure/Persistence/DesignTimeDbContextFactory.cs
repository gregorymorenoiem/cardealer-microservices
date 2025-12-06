using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using CarDealer.Shared.MultiTenancy;

namespace MediaService.Infrastructure.Persistence;

/// <summary>
/// Factory para crear ApplicationDbContext en tiempo de diseño (migraciones)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Por defecto usar PostgreSQL para migraciones
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=mediaservice_dev;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        // Usar un TenantContext dummy para migraciones
        var tenantContext = new DesignTimeTenantContext();

        return new ApplicationDbContext(optionsBuilder.Options, tenantContext);
    }
}

/// <summary>
/// TenantContext dummy para tiempo de diseño (migraciones)
/// </summary>
public class DesignTimeTenantContext : ITenantContext
{
    public Guid? CurrentDealerId => null;
    public bool HasDealerContext => false;
}

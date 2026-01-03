using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using CarDealer.Shared.MultiTenancy;

namespace VehiclesSaleService.Infrastructure.Persistence;

/// <summary>
/// Factory para crear DbContext en tiempo de diseño (para migraciones)
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Connection string para desarrollo (migraciones)
        var connectionString = "Host=localhost;Port=5432;Database=productservice_db;Username=postgres;Password=";

        optionsBuilder.UseNpgsql(connectionString);

        // Design-time tenant context (no tenant filtering during migrations)
        var tenantContext = new DesignTimeTenantContext();

        return new ApplicationDbContext(optionsBuilder.Options, tenantContext);
    }

    /// <summary>
    /// Tenant context for design-time operations (migrations)
    /// </summary>
    private class DesignTimeTenantContext : ITenantContext
    {
        public Guid? CurrentDealerId => null;
        public bool HasDealerContext => false;
    }
}

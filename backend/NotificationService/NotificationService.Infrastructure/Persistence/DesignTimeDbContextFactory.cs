using CarDealer.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationService.Infrastructure.Persistence;

/// <summary>
/// Factory for creating ApplicationDbContext at design time (for EF migrations).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=notificationservice_dev;Username=postgres;Password=postgres");

        // Use a mock tenant context for design-time (migrations)
        var tenantContext = new DesignTimeTenantContext();

        return new ApplicationDbContext(optionsBuilder.Options, tenantContext);
    }

    /// <summary>
    /// Mock tenant context for design-time operations.
    /// </summary>
    private class DesignTimeTenantContext : ITenantContext
    {
        public Guid? CurrentDealerId => null;
        public bool HasDealerContext => false;
    }
}

using Microsoft.EntityFrameworkCore;
using CarDealer.Shared.MultiTenancy;

namespace MediaService.Infrastructure.Persistence;

// Shim wrapper so tests referencing MediaDbContext compile.
public class MediaDbContext : ApplicationDbContext
{
    public MediaDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }
}

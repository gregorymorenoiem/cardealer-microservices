using Microsoft.EntityFrameworkCore;
using ReviewService.Domain.Entities;
using ReviewService.Infrastructure.Persistence.Configurations;

namespace ReviewService.Infrastructure.Persistence;

/// &lt;summary&gt;
/// DbContext para ReviewService
/// &lt;/summary&gt;
public class ReviewDbContext : DbContext
{
    public ReviewDbContext(DbContextOptions&lt;ReviewDbContext&gt; options) : base(options)
    {
    }

    public DbSet&lt;Review&gt; Reviews { get; set; }
    public DbSet&lt;ReviewResponse&gt; ReviewResponses { get; set; }
    public DbSet&lt;ReviewSummary&gt; ReviewSummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones
        modelBuilder.ApplyConfiguration(new ReviewConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewResponseConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewSummaryConfiguration());

        // Configurar esquema
        modelBuilder.HasDefaultSchema("reviewservice");
    }
}
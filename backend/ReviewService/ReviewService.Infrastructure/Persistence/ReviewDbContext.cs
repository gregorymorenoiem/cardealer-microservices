using Microsoft.EntityFrameworkCore;
using ReviewService.Domain.Entities;
using ReviewService.Infrastructure.Persistence.Configurations;

namespace ReviewService.Infrastructure.Persistence;

/// <summary>
/// DbContext para ReviewService
/// </summary>
public class ReviewDbContext : DbContext
{
    public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options)
    {
    }

    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewResponse> ReviewResponses { get; set; }
    public DbSet<ReviewSummary> ReviewSummaries { get; set; }

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
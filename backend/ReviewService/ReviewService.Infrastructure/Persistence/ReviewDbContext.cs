using Microsoft.EntityFrameworkCore;
using ReviewService.Domain.Entities;
using ReviewService.Infrastructure.Persistence.Configurations;

namespace ReviewService.Infrastructure.Persistence;

/// <summary>
/// DbContext para ReviewService - Sprint 15 Extended
/// </summary>
public class ReviewDbContext : DbContext
{
    public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options)
    {
    }

    // Sprint 14 - Entidades básicas
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewResponse> ReviewResponses { get; set; }
    public DbSet<ReviewSummary> ReviewSummaries { get; set; }

    // Sprint 15 - Nuevas entidades
    public DbSet<ReviewHelpfulVote> ReviewHelpfulVotes { get; set; }
    public DbSet<SellerBadge> SellerBadges { get; set; }
    public DbSet<ReviewRequest> ReviewRequests { get; set; }
    public DbSet<FraudDetectionLog> FraudDetectionLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Sprint 14 - Configuraciones básicas
        modelBuilder.ApplyConfiguration(new ReviewConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewResponseConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewSummaryConfiguration());

        // Sprint 15 - Nuevas configuraciones
        modelBuilder.ApplyConfiguration(new ReviewHelpfulVoteConfiguration());
        modelBuilder.ApplyConfiguration(new SellerBadgeConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewRequestConfiguration());
        modelBuilder.ApplyConfiguration(new FraudDetectionLogConfiguration());

        // Configurar esquema
        modelBuilder.HasDefaultSchema("reviewservice");
    }
}
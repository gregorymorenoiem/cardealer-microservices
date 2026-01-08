using DealerAnalyticsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DealerAnalyticsService.Infrastructure.Persistence;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

    public DbSet<ProfileView> ProfileViews => Set<ProfileView>();
    public DbSet<ContactEvent> ContactEvents => Set<ContactEvent>();
    public DbSet<DailyAnalyticsSummary> DailyAnalyticsSummaries => Set<DailyAnalyticsSummary>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ProfileView configuration
        builder.Entity<ProfileView>(entity =>
        {
            entity.ToTable("profile_views");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.ViewedAt);
            entity.HasIndex(e => new { e.DealerId, e.ViewedAt });
            entity.HasIndex(e => e.ViewerIpAddress);
        });

        // ContactEvent configuration
        builder.Entity<ContactEvent>(entity =>
        {
            entity.ToTable("contact_events");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.ClickedAt);
            entity.HasIndex(e => new { e.DealerId, e.ClickedAt });
            entity.HasIndex(e => e.ContactType);
        });

        // DailyAnalyticsSummary configuration
        builder.Entity<DailyAnalyticsSummary>(entity =>
        {
            entity.ToTable("daily_analytics_summaries");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => new { e.DealerId, e.Date }).IsUnique();
        });
    }
}

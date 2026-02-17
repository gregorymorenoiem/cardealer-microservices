using Microsoft.EntityFrameworkCore;
using AlertService.Domain.Entities;

namespace AlertService.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<PriceAlert> PriceAlerts => Set<PriceAlert>();
    public DbSet<SavedSearch> SavedSearches => Set<SavedSearch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PriceAlert configuration
        modelBuilder.Entity<PriceAlert>(entity =>
        {
            entity.ToTable("price_alerts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("Id");

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnName("UserId");

            entity.Property(e => e.VehicleId)
                .IsRequired()
                .HasColumnName("VehicleId");

            entity.Property(e => e.TargetPrice)
                .HasPrecision(18, 2)
                .IsRequired()
                .HasColumnName("TargetPrice");

            entity.Property(e => e.Condition)
                .IsRequired()
                .HasColumnName("Condition")
                .HasConversion<int>();

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("IsActive")
                .HasDefaultValue(true);

            entity.Property(e => e.IsTriggered)
                .IsRequired()
                .HasColumnName("IsTriggered")
                .HasDefaultValue(false);

            entity.Property(e => e.TriggeredAt)
                .HasColumnName("TriggeredAt");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnName("CreatedAt")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnName("UpdatedAt")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_price_alerts_user");

            entity.HasIndex(e => e.VehicleId)
                .HasDatabaseName("idx_price_alerts_vehicle");

            entity.HasIndex(e => new { e.UserId, e.VehicleId })
                .HasDatabaseName("idx_price_alerts_user_vehicle")
                .IsUnique();

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_price_alerts_active");
        });

        // SavedSearch configuration
        modelBuilder.Entity<SavedSearch>(entity =>
        {
            entity.ToTable("saved_searches");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("Id");

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnName("UserId");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("Name");

            entity.Property(e => e.SearchCriteria)
                .IsRequired()
                .HasColumnType("jsonb")
                .HasColumnName("SearchCriteria");

            entity.Property(e => e.SendEmailNotifications)
                .IsRequired()
                .HasColumnName("SendEmailNotifications")
                .HasDefaultValue(true);

            entity.Property(e => e.Frequency)
                .IsRequired()
                .HasColumnName("Frequency")
                .HasConversion<int>()
                .HasDefaultValue(NotificationFrequency.Daily);

            entity.Property(e => e.LastNotificationSent)
                .HasColumnName("LastNotificationSent");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("IsActive")
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnName("CreatedAt")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnName("UpdatedAt")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_saved_searches_user");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_saved_searches_active");

            entity.HasIndex(e => e.LastNotificationSent)
                .HasDatabaseName("idx_saved_searches_last_notification");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is PriceAlert alert)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
            else if (entry.Entity is SavedSearch search)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using RecommendationService.Domain.Entities;

namespace RecommendationService.Infrastructure.Persistence;

public class RecommendationDbContext : DbContext
{
    public RecommendationDbContext(DbContextOptions<RecommendationDbContext> options) : base(options) { }

    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<VehicleInteraction> VehicleInteractions => Set<VehicleInteraction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Recommendation
        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.ToTable("recommendations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.VehicleId).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Score).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsRelevant).HasDefaultValue(true);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.VehicleId);
            entity.HasIndex(e => new { e.UserId, e.Type });
            entity.HasIndex(e => e.CreatedAt);
        });

        // UserPreference
        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.ToTable("user_preferences");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.PreferredMakes).HasColumnType("jsonb");
            entity.Property(e => e.PreferredModels).HasColumnType("jsonb");
            entity.Property(e => e.PreferredBodyTypes).HasColumnType("jsonb");
            entity.Property(e => e.PreferredFuelTypes).HasColumnType("jsonb");
            entity.Property(e => e.PreferredTransmissions).HasColumnType("jsonb");
            entity.Property(e => e.PreferredColors).HasColumnType("jsonb");
            entity.Property(e => e.PreferredFeatures).HasColumnType("jsonb");
            entity.Property(e => e.Confidence).HasDefaultValue(0.0);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // VehicleInteraction
        modelBuilder.Entity<VehicleInteraction>(entity =>
        {
            entity.ToTable("vehicle_interactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.VehicleId).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.DurationSeconds).HasDefaultValue(0);
            entity.Property(e => e.Source).HasMaxLength(100);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.VehicleId);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.VehicleId, e.CreatedAt });
        });
    }
}

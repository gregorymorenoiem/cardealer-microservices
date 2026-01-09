using Microsoft.EntityFrameworkCore;
using FeatureStoreService.Domain.Entities;

namespace FeatureStoreService.Infrastructure.Persistence;

public class FeatureStoreDbContext : DbContext
{
    public FeatureStoreDbContext(DbContextOptions<FeatureStoreDbContext> options) : base(options) { }

    public DbSet<UserFeature> UserFeatures { get; set; }
    public DbSet<VehicleFeature> VehicleFeatures { get; set; }
    public DbSet<FeatureDefinition> FeatureDefinitions { get; set; }
    public DbSet<FeatureBatch> FeatureBatches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserFeature>(entity =>
        {
            entity.ToTable("user_features");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.FeatureName }).IsUnique();
            entity.HasIndex(e => e.ComputedAt);
            entity.HasIndex(e => e.ExpiresAt);
        });

        modelBuilder.Entity<VehicleFeature>(entity =>
        {
            entity.ToTable("vehicle_features");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.VehicleId, e.FeatureName }).IsUnique();
            entity.HasIndex(e => e.ComputedAt);
            entity.HasIndex(e => e.ExpiresAt);
        });

        modelBuilder.Entity<FeatureDefinition>(entity =>
        {
            entity.ToTable("feature_definitions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FeatureName).IsUnique();
            entity.HasIndex(e => e.Category);
        });

        modelBuilder.Entity<FeatureBatch>(entity =>
        {
            entity.ToTable("feature_batches");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StartedAt);
            entity.Property(e => e.FeatureNames).HasColumnType("jsonb");
        });
    }
}

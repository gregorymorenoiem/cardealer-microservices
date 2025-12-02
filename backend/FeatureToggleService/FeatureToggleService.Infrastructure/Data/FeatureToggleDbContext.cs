using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FeatureToggleService.Infrastructure.Data;

public class FeatureToggleDbContext : DbContext
{
    public FeatureToggleDbContext(DbContextOptions<FeatureToggleDbContext> options) : base(options)
    {
    }

    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<RolloutStrategy> RolloutStrategies => Set<RolloutStrategy>();
    public DbSet<FeatureFlagHistory> FeatureFlagHistories => Set<FeatureFlagHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // FeatureFlag configuration
        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Environment);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Owner)
                .HasMaxLength(100);

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100);

            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100);

            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.TargetUserIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.TargetGroups)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.HasOne(e => e.RolloutStrategy)
                .WithOne(r => r.FeatureFlag)
                .HasForeignKey<RolloutStrategy>(r => r.FeatureFlagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.History)
                .WithOne(h => h.FeatureFlag)
                .HasForeignKey(h => h.FeatureFlagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RolloutStrategy configuration
        modelBuilder.Entity<RolloutStrategy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FeatureFlagId);

            entity.Property(e => e.TargetUserIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.TargetTenants)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.TargetGroups)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.TargetRegions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.HashSeed)
                .HasMaxLength(100);
        });

        // FeatureFlagHistory configuration
        modelBuilder.Entity<FeatureFlagHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FeatureFlagId);
            entity.HasIndex(e => e.ChangedAt);
            entity.HasIndex(e => e.ChangedBy);

            entity.Property(e => e.ChangeDescription)
                .HasMaxLength(500);

            entity.Property(e => e.ChangedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(50);

            entity.Property(e => e.Reason)
                .HasMaxLength(500);
        });
    }
}

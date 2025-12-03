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

    // A/B Testing entities
    public DbSet<ABExperiment> ABExperiments => Set<ABExperiment>();
    public DbSet<ExperimentVariant> ExperimentVariants => Set<ExperimentVariant>();
    public DbSet<ExperimentAssignment> ExperimentAssignments => Set<ExperimentAssignment>();
    public DbSet<ExperimentMetric> ExperimentMetrics => Set<ExperimentMetric>();

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

        // ABExperiment configuration
        modelBuilder.Entity<ABExperiment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.FeatureFlagId);
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.EndDate);

            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Hypothesis)
                .HasMaxLength(500);

            entity.Property(e => e.PrimaryMetric)
                .HasMaxLength(100);

            entity.Property(e => e.SecondaryMetrics)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.SegmentationRules)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());

            entity.Property(e => e.CancelReason)
                .HasMaxLength(500);

            entity.Property(e => e.CompletionNotes)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100);

            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100);

            entity.HasOne(e => e.FeatureFlag)
                .WithMany()
                .HasForeignKey(e => e.FeatureFlagId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Variants)
                .WithOne(v => v.Experiment)
                .HasForeignKey(v => v.ExperimentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Assignments)
                .WithOne(a => a.Experiment)
                .HasForeignKey(a => a.ExperimentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Metrics)
                .WithOne(m => m.Experiment)
                .HasForeignKey(m => m.ExperimentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ExperimentVariant configuration
        modelBuilder.Entity<ExperimentVariant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExperimentId);
            entity.HasIndex(e => e.IsControl);

            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.Payload)
                .HasColumnType("jsonb");

            entity.Property(e => e.Parameters)
                .HasColumnType("jsonb");

            entity.Property(e => e.StyleOverrides)
                .HasColumnType("jsonb");

            entity.Property(e => e.MockupUrl)
                .HasMaxLength(500);
        });

        // ExperimentAssignment configuration
        modelBuilder.Entity<ExperimentAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ExperimentId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.VariantId);
            entity.HasIndex(e => e.AssignedAt);
            entity.HasIndex(e => e.IsExposed);
            entity.HasIndex(e => e.HasConverted);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.SessionId)
                .HasMaxLength(100);

            entity.Property(e => e.DeviceId)
                .HasMaxLength(100);

            entity.Property(e => e.IpAddressHash)
                .HasMaxLength(100);

            entity.Property(e => e.Region)
                .HasMaxLength(100);

            entity.Property(e => e.ForcedReason)
                .HasMaxLength(500);

            entity.HasOne(e => e.Variant)
                .WithMany(v => v.Assignments)
                .HasForeignKey(e => e.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Metrics)
                .WithOne(m => m.Assignment)
                .HasForeignKey(m => m.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ExperimentMetric configuration
        modelBuilder.Entity<ExperimentMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExperimentId);
            entity.HasIndex(e => e.VariantId);
            entity.HasIndex(e => e.AssignmentId);
            entity.HasIndex(e => e.MetricKey);
            entity.HasIndex(e => e.RecordedAt);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.MetricKey)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.MetricType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb");

            entity.HasOne(e => e.Variant)
                .WithMany(v => v.Metrics)
                .HasForeignKey(e => e.VariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

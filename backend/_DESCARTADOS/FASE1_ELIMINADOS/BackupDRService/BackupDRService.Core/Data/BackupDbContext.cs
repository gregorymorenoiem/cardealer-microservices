using Microsoft.EntityFrameworkCore;
using BackupDRService.Core.Entities;
using System.Text.Json;

namespace BackupDRService.Core.Data;

public class BackupDbContext : DbContext
{
    public BackupDbContext(DbContextOptions<BackupDbContext> options) : base(options)
    {
    }

    public DbSet<BackupHistory> BackupHistories => Set<BackupHistory>();
    public DbSet<BackupSchedule> BackupSchedules => Set<BackupSchedule>();
    public DbSet<RetentionPolicy> RetentionPolicies => Set<RetentionPolicy>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // BackupHistory configuration
        modelBuilder.Entity<BackupHistory>(entity =>
        {
            entity.ToTable("backup_histories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BackupId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.JobId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.JobName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DatabaseName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BackupType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.StorageType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.Checksum).HasMaxLength(64);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);

            // Convert Dictionary to JSON
            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                )
                .HasColumnType("jsonb");

            entity.HasIndex(e => e.BackupId).IsUnique();
            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => e.DatabaseName);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.Schedule)
                .WithMany(s => s.BackupHistories)
                .HasForeignKey(e => e.ScheduleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // BackupSchedule configuration
        modelBuilder.Entity<BackupSchedule>(entity =>
        {
            entity.ToTable("backup_schedules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DatabaseName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConnectionString).IsRequired().HasMaxLength(500);
            entity.Property(e => e.BackupType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CronExpression).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StorageType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.EncryptionKey).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.DatabaseName);
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => e.NextRunAt);

            entity.HasOne(e => e.RetentionPolicy)
                .WithMany(rp => rp.BackupSchedules)
                .HasForeignKey(e => e.RetentionPolicyId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // RetentionPolicy configuration
        modelBuilder.Entity<RetentionPolicy>(entity =>
        {
            entity.ToTable("retention_policies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ArchiveStorageType).HasMaxLength(50);
            entity.Property(e => e.ArchivePath).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityId).HasMaxLength(50);
            entity.Property(e => e.EntityName).HasMaxLength(200);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UserEmail).HasMaxLength(255);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);

            // Convert Dictionary to JSON
            entity.Property(e => e.AdditionalData)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                )
                .HasColumnType("jsonb");

            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Status);
        });
    }
}

using Microsoft.EntityFrameworkCore;
using Vehicle360ProcessingService.Domain.Entities;

namespace Vehicle360ProcessingService.Infrastructure.Persistence;

public class Vehicle360ProcessingDbContext : DbContext
{
    public Vehicle360ProcessingDbContext(DbContextOptions<Vehicle360ProcessingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle360Job> Jobs => Set<Vehicle360Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vehicle360Job>(entity =>
        {
            entity.ToTable("vehicle_360_jobs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.VehicleId)
                .HasColumnName("vehicle_id")
                .IsRequired();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Progress)
                .HasColumnName("progress")
                .HasDefaultValue(0);

            entity.Property(e => e.ErrorMessage)
                .HasColumnName("error_message")
                .HasMaxLength(2000);

            entity.Property(e => e.ErrorCode)
                .HasColumnName("error_code")
                .HasMaxLength(100);

            entity.Property(e => e.RetryCount)
                .HasColumnName("retry_count")
                .HasDefaultValue(0);

            entity.Property(e => e.MaxRetries)
                .HasColumnName("max_retries")
                .HasDefaultValue(3);

            entity.Property(e => e.FrameCount)
                .HasColumnName("frame_count")
                .HasDefaultValue(6);

            entity.Property(e => e.OriginalVideoUrl)
                .HasColumnName("original_video_url")
                .HasMaxLength(2000);

            entity.Property(e => e.OriginalFileName)
                .HasColumnName("original_file_name")
                .HasMaxLength(500);

            entity.Property(e => e.FileSizeBytes)
                .HasColumnName("file_size_bytes");

            entity.Property(e => e.VideoContentType)
                .HasColumnName("video_content_type")
                .HasMaxLength(100);

            entity.Property(e => e.MediaUploadId)
                .HasColumnName("media_upload_id")
                .HasMaxLength(500);

            entity.Property(e => e.Video360JobId)
                .HasColumnName("video360_job_id");

            entity.Property(e => e.ClientIpAddress)
                .HasColumnName("client_ip_address")
                .HasMaxLength(50);

            entity.Property(e => e.UserAgent)
                .HasColumnName("user_agent")
                .HasMaxLength(500);

            entity.Property(e => e.CorrelationId)
                .HasColumnName("correlation_id")
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.StartedAt)
                .HasColumnName("started_at");

            entity.Property(e => e.CompletedAt)
                .HasColumnName("completed_at");

            // Options como JSON
            entity.Property(e => e.Options)
                .HasColumnName("options")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<ProcessingOptions>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new ProcessingOptions()
                );

            // ProcessedFrames como JSON
            entity.Property(e => e.ProcessedFrames)
                .HasColumnName("processed_frames")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<ProcessedFrame>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<ProcessedFrame>()
                );

            // BackgroundRemovalJobIds como JSON
            entity.Property(e => e.BackgroundRemovalJobIds)
                .HasColumnName("background_removal_job_ids")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>()
                );

            // Índices
            entity.HasIndex(e => e.VehicleId)
                .HasDatabaseName("ix_vehicle_360_jobs_vehicle_id");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_vehicle_360_jobs_user_id");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("ix_vehicle_360_jobs_status");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("ix_vehicle_360_jobs_created_at");

            entity.HasIndex(e => new { e.VehicleId, e.Status })
                .HasDatabaseName("ix_vehicle_360_jobs_vehicle_status");

            entity.HasIndex(e => e.CorrelationId)
                .HasDatabaseName("ix_vehicle_360_jobs_correlation_id");
        });
    }
}

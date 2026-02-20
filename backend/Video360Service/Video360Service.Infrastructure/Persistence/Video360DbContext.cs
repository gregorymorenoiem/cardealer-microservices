using Microsoft.EntityFrameworkCore;
using Video360Service.Domain.Entities;

namespace Video360Service.Infrastructure.Persistence;

/// <summary>
/// DbContext para Video360Service
/// </summary>
public class Video360DbContext : DbContext
{
    public Video360DbContext(DbContextOptions<Video360DbContext> options) : base(options)
    {
    }
    
    public DbSet<Video360Job> Video360Jobs => Set<Video360Job>();
    public DbSet<ExtractedFrame> ExtractedFrames => Set<ExtractedFrame>();
    public DbSet<ProviderConfiguration> ProviderConfigurations => Set<ProviderConfiguration>();
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Video360Job
        modelBuilder.Entity<Video360Job>(entity =>
        {
            entity.ToTable("video360_jobs");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(100);
            entity.Property(e => e.SourceVideoUrl).HasColumnName("source_video_url").HasMaxLength(2000);
            entity.Property(e => e.OriginalFileName).HasColumnName("original_file_name").HasMaxLength(500);
            entity.Property(e => e.OriginalFileSizeBytes).HasColumnName("original_file_size_bytes");
            entity.Property(e => e.OriginalContentType).HasColumnName("original_content_type").HasMaxLength(100);
            entity.Property(e => e.VideoDurationSeconds).HasColumnName("video_duration_seconds");
            entity.Property(e => e.VideoResolution).HasColumnName("video_resolution").HasMaxLength(50);
            entity.Property(e => e.VideoFps).HasColumnName("video_fps");
            entity.Property(e => e.Provider).HasColumnName("provider");
            entity.Property(e => e.FallbackProvider).HasColumnName("fallback_provider");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.OutputFormat).HasColumnName("output_format");
            entity.Property(e => e.OutputQuality).HasColumnName("output_quality");
            entity.Property(e => e.FrameCount).HasColumnName("frame_count");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
            entity.Property(e => e.ErrorCode).HasColumnName("error_code").HasMaxLength(100);
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");
            entity.Property(e => e.MaxRetries).HasColumnName("max_retries");
            entity.Property(e => e.ProcessingTimeMs).HasColumnName("processing_time_ms");
            entity.Property(e => e.CostUsd).HasColumnName("cost_usd").HasPrecision(10, 4);
            entity.Property(e => e.CallbackUrl).HasColumnName("callback_url").HasMaxLength(2000);
            entity.Property(e => e.CorrelationId).HasColumnName("correlation_id").HasMaxLength(100);
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.MetadataJson).HasColumnName("metadata_json");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.ProcessedSync).HasColumnName("processed_sync");
            
            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_video360_jobs_user_id");
            entity.HasIndex(e => e.VehicleId).HasDatabaseName("ix_video360_jobs_vehicle_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_video360_jobs_status");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("ix_video360_jobs_created_at");
            entity.HasIndex(e => new { e.Status, e.Priority }).HasDatabaseName("ix_video360_jobs_status_priority");
            
            entity.HasMany(e => e.ExtractedFrames)
                  .WithOne(f => f.Video360Job)
                  .HasForeignKey(f => f.Video360JobId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ExtractedFrame
        modelBuilder.Entity<ExtractedFrame>(entity =>
        {
            entity.ToTable("extracted_frames");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Video360JobId).HasColumnName("video360_job_id");
            entity.Property(e => e.FrameIndex).HasColumnName("frame_index");
            entity.Property(e => e.AngleDegrees).HasColumnName("angle_degrees");
            entity.Property(e => e.TimestampSeconds).HasColumnName("timestamp_seconds");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").HasMaxLength(2000);
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url").HasMaxLength(2000);
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.ContentType).HasColumnName("content_type").HasMaxLength(100);
            entity.Property(e => e.Width).HasColumnName("width");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.FileHash).HasColumnName("file_hash").HasMaxLength(100);
            entity.Property(e => e.IsOptimized).HasColumnName("is_optimized");
            entity.Property(e => e.AngleLabel).HasColumnName("angle_label").HasMaxLength(100);
            
            entity.HasIndex(e => e.Video360JobId).HasDatabaseName("ix_extracted_frames_job_id");
            entity.HasIndex(e => new { e.Video360JobId, e.FrameIndex }).IsUnique().HasDatabaseName("ix_extracted_frames_job_frame");
        });
        
        // ProviderConfiguration
        modelBuilder.Entity<ProviderConfiguration>(entity =>
        {
            entity.ToTable("provider_configurations");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Provider).HasColumnName("provider");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.CostPerVideoUsd).HasColumnName("cost_per_video_usd").HasPrecision(10, 4);
            entity.Property(e => e.DailyLimit).HasColumnName("daily_limit");
            entity.Property(e => e.DailyUsageCount).HasColumnName("daily_usage_count");
            entity.Property(e => e.LastDailyReset).HasColumnName("last_daily_reset");
            entity.Property(e => e.MaxVideoSizeMb).HasColumnName("max_video_size_mb");
            entity.Property(e => e.MaxVideoDurationSeconds).HasColumnName("max_video_duration_seconds");
            entity.Property(e => e.TimeoutSeconds).HasColumnName("timeout_seconds");
            entity.Property(e => e.SupportedFormats).HasColumnName("supported_formats").HasMaxLength(500);
            entity.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasIndex(e => e.Provider).IsUnique().HasDatabaseName("ix_provider_configs_provider");
        });
        
        // UsageRecord
        modelBuilder.Entity<UsageRecord>(entity =>
        {
            entity.ToTable("usage_records");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Video360JobId).HasColumnName("video360_job_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(100);
            entity.Property(e => e.Provider).HasColumnName("provider");
            entity.Property(e => e.IsSuccess).HasColumnName("is_success");
            entity.Property(e => e.VideoSizeBytes).HasColumnName("video_size_bytes");
            entity.Property(e => e.VideoDurationSeconds).HasColumnName("video_duration_seconds");
            entity.Property(e => e.FramesExtracted).HasColumnName("frames_extracted");
            entity.Property(e => e.ProcessingTimeMs).HasColumnName("processing_time_ms");
            entity.Property(e => e.CostUsd).HasColumnName("cost_usd").HasPrecision(10, 4);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.BillingPeriod).HasColumnName("billing_period").HasMaxLength(10);
            entity.Property(e => e.IsBilled).HasColumnName("is_billed");
            entity.Property(e => e.BilledAt).HasColumnName("billed_at");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id").HasMaxLength(100);
            
            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_usage_records_user_id");
            entity.HasIndex(e => e.TenantId).HasDatabaseName("ix_usage_records_tenant_id");
            entity.HasIndex(e => e.BillingPeriod).HasDatabaseName("ix_usage_records_billing_period");
            entity.HasIndex(e => new { e.IsBilled, e.BillingPeriod }).HasDatabaseName("ix_usage_records_billing");
        });
    }
}

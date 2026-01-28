using Microsoft.EntityFrameworkCore;
using Video360Service.Domain.Entities;

namespace Video360Service.Infrastructure.Persistence;

public class Video360DbContext : DbContext
{
    public Video360DbContext(DbContextOptions<Video360DbContext> options) : base(options)
    {
    }

    public DbSet<Video360Job> Video360Jobs => Set<Video360Job>();
    public DbSet<ExtractedFrame> ExtractedFrames => Set<ExtractedFrame>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Video360Job configuration
        modelBuilder.Entity<Video360Job>(entity =>
        {
            entity.ToTable("video360_jobs");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id").IsRequired();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.VideoUrl).HasColumnName("video_url").HasMaxLength(2048).IsRequired();
            entity.Property(e => e.OriginalFileName).HasColumnName("original_file_name").HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(e => e.FramesToExtract).HasColumnName("frames_to_extract").HasDefaultValue(6);
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.Progress).HasColumnName("progress").HasDefaultValue(0);
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
            entity.Property(e => e.ProcessingStartedAt).HasColumnName("processing_started_at");
            entity.Property(e => e.ProcessingCompletedAt).HasColumnName("processing_completed_at");
            entity.Property(e => e.ProcessingDurationMs).HasColumnName("processing_duration_ms");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            // Store ProcessingOptions as JSON
            entity.OwnsOne(e => e.Options, options =>
            {
                options.ToJson("processing_options");
            });

            // Indexes
            entity.HasIndex(e => e.VehicleId).HasDatabaseName("ix_video360_jobs_vehicle_id");
            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_video360_jobs_user_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_video360_jobs_status");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("ix_video360_jobs_created_at");

            // Relationships
            entity.HasMany(e => e.ExtractedFrames)
                .WithOne(f => f.Video360Job)
                .HasForeignKey(f => f.Video360JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ExtractedFrame configuration
        modelBuilder.Entity<ExtractedFrame>(entity =>
        {
            entity.ToTable("extracted_frames");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Video360JobId).HasColumnName("video360_job_id").IsRequired();
            entity.Property(e => e.SequenceNumber).HasColumnName("sequence_number");
            entity.Property(e => e.AngleDegrees).HasColumnName("angle_degrees");
            entity.Property(e => e.ViewName).HasColumnName("view_name").HasMaxLength(100);
            entity.Property(e => e.SourceFrameNumber).HasColumnName("source_frame_number");
            entity.Property(e => e.TimestampSeconds).HasColumnName("timestamp_seconds");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").HasMaxLength(2048);
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url").HasMaxLength(2048);
            entity.Property(e => e.Width).HasColumnName("width");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.Format).HasColumnName("format").HasMaxLength(10).HasDefaultValue("jpg");
            entity.Property(e => e.QualityScore).HasColumnName("quality_score");
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary").HasDefaultValue(false);
            entity.Property(e => e.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes
            entity.HasIndex(e => e.Video360JobId).HasDatabaseName("ix_extracted_frames_job_id");
            entity.HasIndex(e => new { e.Video360JobId, e.SequenceNumber })
                .HasDatabaseName("ix_extracted_frames_job_sequence")
                .IsUnique();
        });
    }
}

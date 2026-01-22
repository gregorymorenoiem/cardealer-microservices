using Microsoft.EntityFrameworkCore;
using SpyneIntegrationService.Domain.Entities;

namespace SpyneIntegrationService.Infrastructure.Persistence;

public class SpyneDbContext : DbContext
{
    public SpyneDbContext(DbContextOptions<SpyneDbContext> options) : base(options)
    {
    }

    public DbSet<ImageTransformation> ImageTransformations => Set<ImageTransformation>();
    public DbSet<SpinGeneration> SpinGenerations => Set<SpinGeneration>();
    public DbSet<VideoGeneration> VideoGenerations => Set<VideoGeneration>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatLeadInfo> ChatLeadInfos => Set<ChatLeadInfo>();
    public DbSet<SpyneWebhookEvent> WebhookEvents => Set<SpyneWebhookEvent>();
    public DbSet<BackgroundPresetConfig> BackgroundPresetConfigs => Set<BackgroundPresetConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ImageTransformation
        modelBuilder.Entity<ImageTransformation>(entity =>
        {
            entity.ToTable("image_transformations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id").IsRequired();
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.SpyneJobId).HasColumnName("spyne_job_id").HasMaxLength(100);
            entity.Property(e => e.SourceImageUrl).HasColumnName("source_image_url").HasMaxLength(2000).IsRequired();
            entity.Property(e => e.TransformedImageUrl).HasColumnName("transformed_image_url").HasMaxLength(2000);
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url").HasMaxLength(2000);
            entity.Property(e => e.TransformationType).HasColumnName("transformation_type").HasConversion<string>();
            entity.Property(e => e.BackgroundPreset).HasColumnName("background_preset").HasConversion<string>();
            entity.Property(e => e.CustomBackgroundId).HasColumnName("custom_background_id").HasMaxLength(100);
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
            entity.Property(e => e.ProcessingTimeMs).HasColumnName("processing_time_ms");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasIndex(e => e.VehicleId).HasDatabaseName("ix_image_transformations_vehicle_id");
            entity.HasIndex(e => e.SpyneJobId).HasDatabaseName("ix_image_transformations_spyne_job_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_image_transformations_status");
        });

        // SpinGeneration
        modelBuilder.Entity<SpinGeneration>(entity =>
        {
            entity.ToTable("spin_generations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id").IsRequired();
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.SpyneJobId).HasColumnName("spyne_job_id").HasMaxLength(100);
            entity.Property(e => e.SourceImageUrls).HasColumnName("source_image_urls")
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.Property(e => e.SpinViewerUrl).HasColumnName("spin_viewer_url").HasMaxLength(2000);
            entity.Property(e => e.SpinEmbedCode).HasColumnName("spin_embed_code");
            entity.Property(e => e.FallbackImageUrl).HasColumnName("fallback_image_url").HasMaxLength(2000);
            entity.Property(e => e.BackgroundPreset).HasColumnName("background_preset").HasConversion<string>();
            entity.Property(e => e.CustomBackgroundId).HasColumnName("custom_background_id").HasMaxLength(100);
            entity.Property(e => e.RequiredImageCount).HasColumnName("required_image_count");
            entity.Property(e => e.ReceivedImageCount).HasColumnName("received_image_count");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
            entity.Property(e => e.ProcessingTimeMs).HasColumnName("processing_time_ms");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasIndex(e => e.VehicleId).HasDatabaseName("ix_spin_generations_vehicle_id");
            entity.HasIndex(e => e.SpyneJobId).HasDatabaseName("ix_spin_generations_spyne_job_id");
        });

        // VideoGeneration
        modelBuilder.Entity<VideoGeneration>(entity =>
        {
            entity.ToTable("video_generations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id").IsRequired();
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.SpyneJobId).HasColumnName("spyne_job_id").HasMaxLength(100);
            entity.Property(e => e.SourceImageUrls).HasColumnName("source_image_urls")
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.Property(e => e.VideoUrl).HasColumnName("video_url").HasMaxLength(2000);
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url").HasMaxLength(2000);
            entity.Property(e => e.Style).HasColumnName("style").HasConversion<string>();
            entity.Property(e => e.OutputFormat).HasColumnName("output_format").HasConversion<string>();
            entity.Property(e => e.BackgroundPreset).HasColumnName("background_preset").HasConversion<string>();
            entity.Property(e => e.CustomBackgroundId).HasColumnName("custom_background_id").HasMaxLength(100);
            entity.Property(e => e.IncludeMusic).HasColumnName("include_music");
            entity.Property(e => e.MusicTrackId).HasColumnName("music_track_id").HasMaxLength(100);
            entity.Property(e => e.RequestedDuration).HasColumnName("requested_duration");
            entity.Property(e => e.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
            entity.Property(e => e.ProcessingTimeMs).HasColumnName("processing_time_ms");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasIndex(e => e.VehicleId).HasDatabaseName("ix_video_generations_vehicle_id");
            entity.HasIndex(e => e.SpyneJobId).HasDatabaseName("ix_video_generations_spyne_job_id");
        });

        // ChatSession
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.ToTable("chat_sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id").IsRequired();
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SessionIdentifier).HasColumnName("session_identifier").HasMaxLength(200);
            entity.Property(e => e.SpyneChatId).HasColumnName("spyne_chat_id").HasMaxLength(100);
            entity.Property(e => e.Language).HasColumnName("language").HasMaxLength(10);
            entity.Property(e => e.VehicleContextJson).HasColumnName("vehicle_context_json");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.IsQualifiedLead).HasColumnName("is_qualified_lead");
            entity.Property(e => e.ClosureReason).HasColumnName("closure_reason").HasMaxLength(500);
            entity.Property(e => e.UserRating).HasColumnName("user_rating");
            entity.Property(e => e.UserFeedback).HasColumnName("user_feedback");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ClosedAt).HasColumnName("closed_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasMany(e => e.Messages)
                .WithOne()
                .HasForeignKey(e => e.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LeadInfo)
                .WithOne()
                .HasForeignKey<ChatLeadInfo>(e => e.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.VehicleId).HasDatabaseName("ix_chat_sessions_vehicle_id");
            entity.HasIndex(e => e.DealerId).HasDatabaseName("ix_chat_sessions_dealer_id");
            entity.HasIndex(e => e.SessionIdentifier).HasDatabaseName("ix_chat_sessions_session_identifier");
        });

        // ChatMessage
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("chat_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChatSessionId).HasColumnName("chat_session_id").IsRequired();
            entity.Property(e => e.SpyneMessageId).HasColumnName("spyne_message_id").HasMaxLength(100);
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>();
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            // Configure Metadata as JSON column (PostgreSQL)
            entity.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => string.IsNullOrEmpty(v) ? null : System.Text.Json.JsonSerializer.Deserialize<ChatMessageMetadata>(v, (System.Text.Json.JsonSerializerOptions?)null));
            
            entity.HasIndex(e => e.ChatSessionId).HasDatabaseName("ix_chat_messages_session_id");
        });

        // ChatLeadInfo
        modelBuilder.Entity<ChatLeadInfo>(entity =>
        {
            entity.ToTable("chat_lead_infos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChatSessionId).HasColumnName("chat_session_id").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(e => e.PreferredContactMethod).HasColumnName("preferred_contact_method").HasMaxLength(50);
            entity.Property(e => e.Budget).HasColumnName("budget");
            entity.Property(e => e.FinancingInterest).HasColumnName("financing_interest").HasMaxLength(100);
            entity.Property(e => e.TradeInInfo).HasColumnName("trade_in_info");
            entity.Property(e => e.InterestType).HasColumnName("interest_type").HasConversion<string>();
            entity.Property(e => e.LeadScore).HasColumnName("lead_score");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // SpyneWebhookEvent
        modelBuilder.Entity<SpyneWebhookEvent>(entity =>
        {
            entity.ToTable("spyne_webhook_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
            entity.Property(e => e.SpyneJobId).HasColumnName("spyne_job_id").HasMaxLength(100);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.RawPayload).HasColumnName("raw_payload").IsRequired();
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.ProcessedSuccessfully).HasColumnName("processed_successfully");
            entity.Property(e => e.ProcessingError).HasColumnName("processing_error").HasMaxLength(1000);
            entity.Property(e => e.ReceivedAt).HasColumnName("received_at");
            
            entity.HasIndex(e => e.SpyneJobId).HasDatabaseName("ix_webhook_events_spyne_job_id");
            entity.HasIndex(e => e.EventType).HasDatabaseName("ix_webhook_events_event_type");
        });

        // BackgroundPresetConfig
        modelBuilder.Entity<BackgroundPresetConfig>(entity =>
        {
            entity.ToTable("background_preset_configs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Preset).HasColumnName("preset").HasConversion<string>();
            entity.Property(e => e.SpyneBackgroundId).HasColumnName("spyne_background_id").HasMaxLength(100).IsRequired();
            entity.Property(e => e.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url").HasMaxLength(500);
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(50);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            entity.HasIndex(e => e.Preset).HasDatabaseName("ix_background_presets_preset");
        });
    }
}

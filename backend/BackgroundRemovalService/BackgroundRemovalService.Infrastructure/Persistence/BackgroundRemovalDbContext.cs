using BackgroundRemovalService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackgroundRemovalService.Infrastructure.Persistence;

public class BackgroundRemovalDbContext : DbContext
{
    public BackgroundRemovalDbContext(DbContextOptions<BackgroundRemovalDbContext> options)
        : base(options)
    {
    }

    public DbSet<BackgroundRemovalJob> BackgroundRemovalJobs => Set<BackgroundRemovalJob>();
    public DbSet<ProviderConfiguration> ProviderConfigurations => Set<ProviderConfiguration>();
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("background_removal");
        
        // BackgroundRemovalJob
        modelBuilder.Entity<BackgroundRemovalJob>(entity =>
        {
            entity.ToTable("background_removal_jobs");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(100);
            entity.Property(e => e.SourceImageUrl).HasColumnName("source_image_url").HasMaxLength(2048).IsRequired();
            entity.Property(e => e.OriginalFileName).HasColumnName("original_file_name").HasMaxLength(500);
            entity.Property(e => e.OriginalFileSizeBytes).HasColumnName("original_file_size_bytes");
            entity.Property(e => e.OriginalContentType).HasColumnName("original_content_type").HasMaxLength(100);
            entity.Property(e => e.ResultImageUrl).HasColumnName("result_image_url").HasMaxLength(2048);
            entity.Property(e => e.ResultFileSizeBytes).HasColumnName("result_file_size_bytes");
            entity.Property(e => e.ResultContentType).HasColumnName("result_content_type").HasMaxLength(100);
            entity.Property(e => e.Provider).HasColumnName("provider").HasConversion<int>();
            entity.Property(e => e.FallbackProvider).HasColumnName("fallback_provider").HasConversion<int?>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.OutputFormat).HasColumnName("output_format").HasConversion<int>();
            entity.Property(e => e.OutputSize).HasColumnName("output_size").HasConversion<int>();
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
            entity.Property(e => e.ErrorCode).HasColumnName("error_code").HasMaxLength(100);
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");
            entity.Property(e => e.MaxRetries).HasColumnName("max_retries");
            entity.Property(e => e.ProcessingTimeMs).HasColumnName("processing_time_ms");
            entity.Property(e => e.CreditsConsumed).HasColumnName("credits_consumed").HasPrecision(18, 6);
            entity.Property(e => e.EstimatedCostUsd).HasColumnName("estimated_cost_usd").HasPrecision(18, 6);
            entity.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.CorrelationId).HasColumnName("correlation_id").HasMaxLength(200);
            entity.Property(e => e.CallbackUrl).HasColumnName("callback_url").HasMaxLength(2048);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.ProcessingStartedAt).HasColumnName("processing_started_at");
            entity.Property(e => e.ProcessingCompletedAt).HasColumnName("processing_completed_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            
            // Índices
            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_jobs_user_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_jobs_status");
            entity.HasIndex(e => e.CorrelationId).HasDatabaseName("ix_jobs_correlation_id").IsUnique();
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("ix_jobs_created_at");
            entity.HasIndex(e => new { e.Status, e.Priority, e.CreatedAt }).HasDatabaseName("ix_jobs_pending_queue");
        });
        
        // ProviderConfiguration
        modelBuilder.Entity<ProviderConfiguration>(entity =>
        {
            entity.ToTable("provider_configurations");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Provider).HasColumnName("provider").HasConversion<int>();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.ApiKey).HasColumnName("api_key").HasMaxLength(500);
            entity.Property(e => e.BaseUrl).HasColumnName("base_url").HasMaxLength(500);
            entity.Property(e => e.RateLimitPerMinute).HasColumnName("rate_limit_per_minute");
            entity.Property(e => e.RateLimitPerDay).HasColumnName("rate_limit_per_day");
            entity.Property(e => e.RequestsUsedToday).HasColumnName("requests_used_today");
            entity.Property(e => e.LastDailyReset).HasColumnName("last_daily_reset");
            entity.Property(e => e.TimeoutSeconds).HasColumnName("timeout_seconds");
            entity.Property(e => e.CostPerImageUsd).HasColumnName("cost_per_image_usd").HasPrecision(18, 6);
            entity.Property(e => e.AvailableCredits).HasColumnName("available_credits").HasPrecision(18, 6);
            entity.Property(e => e.MaxImageSizeMegapixels).HasColumnName("max_image_size_megapixels");
            entity.Property(e => e.MaxFileSizeMb).HasColumnName("max_file_size_mb");
            entity.Property(e => e.SupportedInputFormats).HasColumnName("supported_input_formats").HasMaxLength(500);
            entity.Property(e => e.SupportedOutputFormats).HasColumnName("supported_output_formats").HasMaxLength(500);
            entity.Property(e => e.AdditionalOptions).HasColumnName("additional_options").HasColumnType("jsonb");
            entity.Property(e => e.TotalRequestsProcessed).HasColumnName("total_requests_processed");
            entity.Property(e => e.AverageResponseTimeMs).HasColumnName("average_response_time_ms");
            entity.Property(e => e.SuccessRate).HasColumnName("success_rate");
            entity.Property(e => e.ConsecutiveFailures).HasColumnName("consecutive_failures");
            entity.Property(e => e.IsCircuitBreakerOpen).HasColumnName("is_circuit_breaker_open");
            entity.Property(e => e.CircuitBreakerResetAt).HasColumnName("circuit_breaker_reset_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasIndex(e => e.Provider).HasDatabaseName("ix_providers_provider").IsUnique();
            entity.HasIndex(e => new { e.IsEnabled, e.Priority }).HasDatabaseName("ix_providers_enabled_priority");
        });
        
        // UsageRecord
        modelBuilder.Entity<UsageRecord>(entity =>
        {
            entity.ToTable("usage_records");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(100);
            entity.Property(e => e.Provider).HasColumnName("provider").HasConversion<int>();
            entity.Property(e => e.IsSuccess).HasColumnName("is_success");
            entity.Property(e => e.InputSizeBytes).HasColumnName("input_size_bytes");
            entity.Property(e => e.OutputSizeBytes).HasColumnName("output_size_bytes");
            entity.Property(e => e.ProcessingTimeMs).HasColumnName("processing_time_ms");
            entity.Property(e => e.CreditsConsumed).HasColumnName("credits_consumed").HasPrecision(18, 6);
            entity.Property(e => e.CostUsd).HasColumnName("cost_usd").HasPrecision(18, 6);
            entity.Property(e => e.ClientIpAddress).HasColumnName("client_ip_address").HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.BillingPeriod).HasColumnName("billing_period");
            
            entity.HasIndex(e => e.JobId).HasDatabaseName("ix_usage_job_id");
            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_usage_user_id");
            entity.HasIndex(e => e.BillingPeriod).HasDatabaseName("ix_usage_billing_period");
            entity.HasIndex(e => new { e.UserId, e.BillingPeriod }).HasDatabaseName("ix_usage_user_billing");
        });
    }
}

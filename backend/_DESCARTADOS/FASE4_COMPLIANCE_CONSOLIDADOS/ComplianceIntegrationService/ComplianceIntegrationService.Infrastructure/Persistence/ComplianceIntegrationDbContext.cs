// =====================================================
// C12: ComplianceIntegrationService - DbContext
// Entity Framework Core para PostgreSQL
// =====================================================

using ComplianceIntegrationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceIntegrationService.Infrastructure.Persistence;

/// <summary>
/// DbContext para el servicio de integración con entes reguladores
/// </summary>
public class ComplianceIntegrationDbContext : DbContext
{
    public ComplianceIntegrationDbContext(DbContextOptions<ComplianceIntegrationDbContext> options)
        : base(options)
    {
    }

    public DbSet<IntegrationConfig> IntegrationConfigs { get; set; } = null!;
    public DbSet<IntegrationCredential> IntegrationCredentials { get; set; } = null!;
    public DbSet<DataTransmission> DataTransmissions { get; set; } = null!;
    public DbSet<FieldMapping> FieldMappings { get; set; } = null!;
    public DbSet<IntegrationLog> IntegrationLogs { get; set; } = null!;
    public DbSet<WebhookConfig> WebhookConfigs { get; set; } = null!;
    public DbSet<IntegrationStatusHistory> IntegrationStatusHistories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // IntegrationConfig
        modelBuilder.Entity<IntegrationConfig>(entity =>
        {
            entity.ToTable("integration_configs");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.RegulatoryBody).HasColumnName("regulatory_body").HasConversion<int>();
            entity.Property(e => e.IntegrationType).HasColumnName("integration_type").HasConversion<int>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.EndpointUrl).HasColumnName("endpoint_url").HasMaxLength(500);
            entity.Property(e => e.SandboxUrl).HasColumnName("sandbox_url").HasMaxLength(500);
            entity.Property(e => e.Port).HasColumnName("port");
            entity.Property(e => e.IsSandboxMode).HasColumnName("is_sandbox_mode");
            entity.Property(e => e.SyncFrequency).HasColumnName("sync_frequency").HasConversion<int>();
            entity.Property(e => e.ScheduledTime).HasColumnName("scheduled_time").HasMaxLength(10);
            entity.Property(e => e.ScheduledDays).HasColumnName("scheduled_days").HasMaxLength(20);
            entity.Property(e => e.TimeoutSeconds).HasColumnName("timeout_seconds");
            entity.Property(e => e.MaxRetries).HasColumnName("max_retries");
            entity.Property(e => e.RetryIntervalSeconds).HasColumnName("retry_interval_seconds");
            entity.Property(e => e.RequiresSsl).HasColumnName("requires_ssl");
            entity.Property(e => e.ProtocolVersion).HasColumnName("protocol_version").HasMaxLength(50);
            entity.Property(e => e.AdditionalConfig).HasColumnName("additional_config");
            entity.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(2000);
            entity.Property(e => e.LastSuccessfulSync).HasColumnName("last_successful_sync");
            entity.Property(e => e.LastFailedSync).HasColumnName("last_failed_sync");
            entity.Property(e => e.ConsecutiveErrors).HasColumnName("consecutive_errors");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasIndex(e => e.RegulatoryBody).HasDatabaseName("ix_integration_configs_regulatory_body");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_integration_configs_status");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("ix_integration_configs_is_active");
        });

        // IntegrationCredential
        modelBuilder.Entity<IntegrationCredential>(entity =>
        {
            entity.ToTable("integration_credentials");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IntegrationConfigId).HasColumnName("integration_config_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.CredentialType).HasColumnName("credential_type").HasConversion<int>();
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(200);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(500);
            entity.Property(e => e.ApiKeyHash).HasColumnName("api_key_hash").HasMaxLength(1000);
            entity.Property(e => e.CertificateData).HasColumnName("certificate_data");
            entity.Property(e => e.CertificateThumbprint).HasColumnName("certificate_thumbprint").HasMaxLength(100);
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.Environment).HasColumnName("environment").HasMaxLength(50);
            entity.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasOne(e => e.IntegrationConfig)
                  .WithMany(c => c.Credentials)
                  .HasForeignKey(e => e.IntegrationConfigId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.IntegrationConfigId).HasDatabaseName("ix_credentials_integration_id");
            entity.HasIndex(e => e.ExpiresAt).HasDatabaseName("ix_credentials_expires_at");
        });

        // DataTransmission
        modelBuilder.Entity<DataTransmission>(entity =>
        {
            entity.ToTable("data_transmissions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IntegrationConfigId).HasColumnName("integration_config_id");
            entity.Property(e => e.TransmissionCode).HasColumnName("transmission_code").IsRequired().HasMaxLength(50);
            entity.Property(e => e.ReportType).HasColumnName("report_type").HasConversion<int>();
            entity.Property(e => e.Direction).HasColumnName("direction").HasConversion<int>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(255);
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.FileHash).HasColumnName("file_hash").HasMaxLength(128);
            entity.Property(e => e.RecordCount).HasColumnName("record_count");
            entity.Property(e => e.TransmissionStartedAt).HasColumnName("transmission_started_at");
            entity.Property(e => e.TransmissionCompletedAt).HasColumnName("transmission_completed_at");
            entity.Property(e => e.ConfirmationNumber).HasColumnName("confirmation_number").HasMaxLength(100);
            entity.Property(e => e.ResponseData).HasColumnName("response_data");
            entity.Property(e => e.ResponseCode).HasColumnName("response_code");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
            entity.Property(e => e.ErrorDetails).HasColumnName("error_details");
            entity.Property(e => e.AttemptCount).HasColumnName("attempt_count");
            entity.Property(e => e.NextRetryAt).HasColumnName("next_retry_at");
            entity.Property(e => e.InitiatedByUserId).HasColumnName("initiated_by_user_id");
            entity.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasOne(e => e.IntegrationConfig)
                  .WithMany(c => c.Transmissions)
                  .HasForeignKey(e => e.IntegrationConfigId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.IntegrationConfigId).HasDatabaseName("ix_transmissions_integration_id");
            entity.HasIndex(e => e.TransmissionCode).IsUnique().HasDatabaseName("ix_transmissions_code");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_transmissions_status");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("ix_transmissions_created_at");
            entity.HasIndex(e => e.ReportType).HasDatabaseName("ix_transmissions_report_type");
        });

        // FieldMapping
        modelBuilder.Entity<FieldMapping>(entity =>
        {
            entity.ToTable("field_mappings");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IntegrationConfigId).HasColumnName("integration_config_id");
            entity.Property(e => e.ReportType).HasColumnName("report_type").HasConversion<int>();
            entity.Property(e => e.SourceField).HasColumnName("source_field").IsRequired().HasMaxLength(100);
            entity.Property(e => e.TargetField).HasColumnName("target_field").IsRequired().HasMaxLength(100);
            entity.Property(e => e.SourceDataType).HasColumnName("source_data_type").HasMaxLength(50);
            entity.Property(e => e.TargetDataType).HasColumnName("target_data_type").HasMaxLength(50);
            entity.Property(e => e.Transformation).HasColumnName("transformation").HasMaxLength(200);
            entity.Property(e => e.DefaultValue).HasColumnName("default_value").HasMaxLength(200);
            entity.Property(e => e.IsRequired).HasColumnName("is_required");
            entity.Property(e => e.MaxLength).HasColumnName("max_length");
            entity.Property(e => e.ValidationPattern).HasColumnName("validation_pattern").HasMaxLength(500);
            entity.Property(e => e.FieldOrder).HasColumnName("field_order");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasOne(e => e.IntegrationConfig)
                  .WithMany()
                  .HasForeignKey(e => e.IntegrationConfigId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.IntegrationConfigId, e.ReportType })
                  .HasDatabaseName("ix_field_mappings_integration_report");
        });

        // IntegrationLog
        modelBuilder.Entity<IntegrationLog>(entity =>
        {
            entity.ToTable("integration_logs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IntegrationConfigId).HasColumnName("integration_config_id");
            entity.Property(e => e.DataTransmissionId).HasColumnName("data_transmission_id");
            entity.Property(e => e.Severity).HasColumnName("severity").HasConversion<int>();
            entity.Property(e => e.Category).HasColumnName("category").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Message).HasColumnName("message").IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Details).HasColumnName("details");
            entity.Property(e => e.RequestData).HasColumnName("request_data");
            entity.Property(e => e.ResponseData).HasColumnName("response_data");
            entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            entity.Property(e => e.CorrelationId).HasColumnName("correlation_id").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasOne(e => e.IntegrationConfig)
                  .WithMany(c => c.Logs)
                  .HasForeignKey(e => e.IntegrationConfigId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.IntegrationConfigId).HasDatabaseName("ix_logs_integration_id");
            entity.HasIndex(e => e.DataTransmissionId).HasDatabaseName("ix_logs_transmission_id");
            entity.HasIndex(e => e.Severity).HasDatabaseName("ix_logs_severity");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("ix_logs_created_at");
            entity.HasIndex(e => e.CorrelationId).HasDatabaseName("ix_logs_correlation_id");
        });

        // WebhookConfig
        modelBuilder.Entity<WebhookConfig>(entity =>
        {
            entity.ToTable("webhook_configs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IntegrationConfigId).HasColumnName("integration_config_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.WebhookUrl).HasColumnName("webhook_url").IsRequired().HasMaxLength(500);
            entity.Property(e => e.SecretHash).HasColumnName("secret_hash").HasMaxLength(200);
            entity.Property(e => e.SubscribedEvents).HasColumnName("subscribed_events").HasMaxLength(1000);
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.LastEventReceivedAt).HasColumnName("last_event_received_at");
            entity.Property(e => e.EventCount).HasColumnName("event_count");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasOne(e => e.IntegrationConfig)
                  .WithMany()
                  .HasForeignKey(e => e.IntegrationConfigId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.IntegrationConfigId).HasDatabaseName("ix_webhooks_integration_id");
            entity.HasIndex(e => e.IsEnabled).HasDatabaseName("ix_webhooks_is_enabled");
        });

        // IntegrationStatusHistory
        modelBuilder.Entity<IntegrationStatusHistory>(entity =>
        {
            entity.ToTable("integration_status_histories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IntegrationConfigId).HasColumnName("integration_config_id");
            entity.Property(e => e.PreviousStatus).HasColumnName("previous_status").HasConversion<int>();
            entity.Property(e => e.NewStatus).HasColumnName("new_status").HasConversion<int>();
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500);
            entity.Property(e => e.ChangedByUserId).HasColumnName("changed_by_user_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasOne(e => e.IntegrationConfig)
                  .WithMany()
                  .HasForeignKey(e => e.IntegrationConfigId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.IntegrationConfigId).HasDatabaseName("ix_status_history_integration_id");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("ix_status_history_created_at");
        });
    }
}

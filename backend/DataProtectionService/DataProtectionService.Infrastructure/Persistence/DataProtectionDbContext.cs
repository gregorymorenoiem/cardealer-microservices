using Microsoft.EntityFrameworkCore;
using DataProtectionService.Domain.Entities;

namespace DataProtectionService.Infrastructure.Persistence;

public class DataProtectionDbContext : DbContext
{
    public DataProtectionDbContext(DbContextOptions<DataProtectionDbContext> options) : base(options)
    {
    }

    public DbSet<UserConsent> UserConsents => Set<UserConsent>();
    public DbSet<ARCORequest> ARCORequests => Set<ARCORequest>();
    public DbSet<ARCOAttachment> ARCOAttachments => Set<ARCOAttachment>();
    public DbSet<ARCOStatusHistory> ARCOStatusHistories => Set<ARCOStatusHistory>();
    public DbSet<DataChangeLog> DataChangeLogs => Set<DataChangeLog>();
    public DbSet<PrivacyPolicy> PrivacyPolicies => Set<PrivacyPolicy>();
    public DbSet<AnonymizationRecord> AnonymizationRecords => Set<AnonymizationRecord>();
    public DbSet<DataExport> DataExports => Set<DataExport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UserConsent Configuration
        modelBuilder.Entity<UserConsent>(entity =>
        {
            entity.ToTable("user_consents");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Type });
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.Type).HasColumnName("type")
                .HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(e => e.Version).HasColumnName("version").HasMaxLength(20).IsRequired();
            entity.Property(e => e.DocumentHash).HasColumnName("document_hash").HasMaxLength(128).IsRequired();
            entity.Property(e => e.Granted).HasColumnName("granted").IsRequired();
            entity.Property(e => e.GrantedAt).HasColumnName("granted_at").IsRequired();
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.RevokeReason).HasColumnName("revoke_reason").HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(e => e.CollectionMethod).HasColumnName("collection_method").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        });

        // ARCORequest Configuration
        modelBuilder.Entity<ARCORequest>(entity =>
        {
            entity.ToTable("arco_requests");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RequestNumber).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Deadline);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.RequestNumber).HasColumnName("request_number").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Type).HasColumnName("type")
                .HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status")
                .HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(2000).IsRequired();
            entity.Property(e => e.SpecificDataRequested).HasColumnName("specific_data_requested").HasMaxLength(1000);
            entity.Property(e => e.ProposedChanges).HasColumnName("proposed_changes").HasMaxLength(2000);
            entity.Property(e => e.OppositionReason).HasColumnName("opposition_reason").HasMaxLength(2000);
            entity.Property(e => e.RequestedAt).HasColumnName("requested_at").IsRequired();
            entity.Property(e => e.Deadline).HasColumnName("deadline").IsRequired();
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.ProcessedBy).HasColumnName("processed_by");
            entity.Property(e => e.ProcessedByName).HasColumnName("processed_by_name").HasMaxLength(200);
            entity.Property(e => e.Resolution).HasColumnName("resolution").HasMaxLength(2000);
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(1000);
            entity.Property(e => e.InternalNotes).HasColumnName("internal_notes").HasMaxLength(2000);
            entity.Property(e => e.ExportFileUrl).HasColumnName("export_file_url").HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

            entity.HasMany(e => e.Attachments)
                .WithOne(a => a.ARCORequest)
                .HasForeignKey(a => a.ARCORequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.StatusHistory)
                .WithOne(h => h.ARCORequest)
                .HasForeignKey(h => h.ARCORequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ARCOAttachment Configuration
        modelBuilder.Entity<ARCOAttachment>(entity =>
        {
            entity.ToTable("arco_attachments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ARCORequestId).HasColumnName("arco_request_id").IsRequired();
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.FileUrl).HasColumnName("file_url").HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileType).HasColumnName("file_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.FileSize).HasColumnName("file_size").IsRequired();
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by").IsRequired();
            entity.Property(e => e.UploadedAt).HasColumnName("uploaded_at").IsRequired();
        });

        // ARCOStatusHistory Configuration
        modelBuilder.Entity<ARCOStatusHistory>(entity =>
        {
            entity.ToTable("arco_status_history");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ARCORequestId);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ARCORequestId).HasColumnName("arco_request_id").IsRequired();
            entity.Property(e => e.OldStatus).HasColumnName("old_status")
                .HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.NewStatus).HasColumnName("new_status")
                .HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Comment).HasColumnName("comment").HasMaxLength(500);
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by").IsRequired();
            entity.Property(e => e.ChangedByName).HasColumnName("changed_by_name").HasMaxLength(200);
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at").IsRequired();
        });

        // DataChangeLog Configuration
        modelBuilder.Entity<DataChangeLog>(entity =>
        {
            entity.ToTable("data_change_logs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ChangedAt);
            entity.HasIndex(e => new { e.UserId, e.DataCategory });

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.DataField).HasColumnName("data_field").HasMaxLength(100).IsRequired();
            entity.Property(e => e.DataCategory).HasColumnName("data_category").HasMaxLength(50).IsRequired();
            entity.Property(e => e.OldValueHash).HasColumnName("old_value_hash").HasMaxLength(128);
            entity.Property(e => e.NewValueHash).HasColumnName("new_value_hash").HasMaxLength(128);
            entity.Property(e => e.OldValueMasked).HasColumnName("old_value_masked").HasMaxLength(500);
            entity.Property(e => e.NewValueMasked).HasColumnName("new_value_masked").HasMaxLength(500);
            entity.Property(e => e.ChangedByType).HasColumnName("changed_by_type").HasMaxLength(20).IsRequired();
            entity.Property(e => e.ChangedById).HasColumnName("changed_by_id");
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500);
            entity.Property(e => e.SourceService).HasColumnName("source_service").HasMaxLength(100).IsRequired();
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at").IsRequired();
        });

        // PrivacyPolicy Configuration
        modelBuilder.Entity<PrivacyPolicy>(entity =>
        {
            entity.ToTable("privacy_policies");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DocumentType, e.Language, e.Version }).IsUnique();
            entity.HasIndex(e => e.IsActive);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Version).HasColumnName("version").HasMaxLength(20).IsRequired();
            entity.Property(e => e.DocumentType).HasColumnName("document_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.ChangesSummary).HasColumnName("changes_summary").HasMaxLength(2000);
            entity.Property(e => e.Language).HasColumnName("language").HasMaxLength(5).IsRequired();
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(e => e.RequiresReAcceptance).HasColumnName("requires_re_acceptance").IsRequired();
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").IsRequired();
            entity.Property(e => e.CreatedByName).HasColumnName("created_by_name").HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // AnonymizationRecord Configuration
        modelBuilder.Entity<AnonymizationRecord>(entity =>
        {
            entity.ToTable("anonymization_records");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.ARCORequestId).HasColumnName("arco_request_id");
            entity.Property(e => e.RequestedBy).HasColumnName("requested_by");
            entity.Property(e => e.AnonymizedAt).HasColumnName("anonymized_at").IsRequired();
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500).IsRequired();
            entity.Property(e => e.OriginalEmail).HasColumnName("original_email").HasMaxLength(256);
            entity.Property(e => e.OriginalPhone).HasColumnName("original_phone").HasMaxLength(20);
            entity.Property(e => e.AnonymizedEmail).HasColumnName("anonymized_email").HasMaxLength(256);
            entity.Property(e => e.AnonymizedPhone).HasColumnName("anonymized_phone").HasMaxLength(20);
            entity.Property(e => e.AffectedRecordsCount).HasColumnName("affected_records_count");
            entity.Property(e => e.IsComplete).HasColumnName("is_complete").IsRequired();
            entity.Property(e => e.RetentionEndDate).HasColumnName("retention_end_date").IsRequired();
            
            // Store AffectedTables as JSON
            entity.Property(e => e.AffectedTables)
                .HasColumnName("affected_tables")
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        });

        // DataExport Configuration
        modelBuilder.Entity<DataExport>(entity =>
        {
            entity.ToTable("data_exports");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequestedAt);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.ARCORequestId).HasColumnName("arco_request_id");
            entity.Property(e => e.Status).HasColumnName("status")
                .HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.Format).HasColumnName("format").HasMaxLength(10).IsRequired();
            entity.Property(e => e.IncludeTransactions).HasColumnName("include_transactions").IsRequired();
            entity.Property(e => e.IncludeMessages).HasColumnName("include_messages").IsRequired();
            entity.Property(e => e.IncludeVehicleHistory).HasColumnName("include_vehicle_history").IsRequired();
            entity.Property(e => e.IncludeUserActivity).HasColumnName("include_user_activity").IsRequired();
            entity.Property(e => e.RequestedAt).HasColumnName("requested_at").IsRequired();
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.DownloadUrl).HasColumnName("download_url").HasMaxLength(500);
            entity.Property(e => e.DownloadExpiresAt).HasColumnName("download_expires_at");
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
        });
    }
}

using Microsoft.EntityFrameworkCore;
using RegulatoryAlertService.Domain.Entities;

namespace RegulatoryAlertService.Infrastructure.Persistence;

public class RegulatoryAlertDbContext : DbContext
{
    public RegulatoryAlertDbContext(DbContextOptions<RegulatoryAlertDbContext> options)
        : base(options) { }

    public DbSet<RegulatoryAlert> RegulatoryAlerts => Set<RegulatoryAlert>();
    public DbSet<AlertNotification> AlertNotifications => Set<AlertNotification>();
    public DbSet<AlertSubscription> AlertSubscriptions => Set<AlertSubscription>();
    public DbSet<RegulatoryCalendarEntry> RegulatoryCalendarEntries => Set<RegulatoryCalendarEntry>();
    public DbSet<ComplianceDeadline> ComplianceDeadlines => Set<ComplianceDeadline>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RegulatoryAlert
        modelBuilder.Entity<RegulatoryAlert>(entity =>
        {
            entity.ToTable("regulatory_alerts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(2000).IsRequired();
            entity.Property(e => e.DetailedContent).HasColumnName("detailed_content");
            entity.Property(e => e.AlertType).HasColumnName("alert_type").HasConversion<string>();
            entity.Property(e => e.Priority).HasColumnName("priority").HasConversion<string>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.RegulatoryBody).HasColumnName("regulatory_body").HasConversion<string>();
            entity.Property(e => e.Category).HasColumnName("category").HasConversion<string>();
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date");
            entity.Property(e => e.DeadlineDate).HasColumnName("deadline_date");
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.LegalReference).HasColumnName("legal_reference").HasMaxLength(500);
            entity.Property(e => e.OfficialDocumentUrl).HasColumnName("official_document_url").HasMaxLength(1000);
            entity.Property(e => e.SourceUrl).HasColumnName("source_url").HasMaxLength(1000);
            entity.Property(e => e.IsPublic).HasColumnName("is_public");
            entity.Property(e => e.RequiresAction).HasColumnName("requires_action");
            entity.Property(e => e.ActionRequired).HasColumnName("action_required").HasMaxLength(2000);
            entity.Property(e => e.Tags).HasColumnName("tags").HasMaxLength(500);
            entity.Property(e => e.MetadataJson).HasColumnName("metadata_json");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);

            entity.HasIndex(e => e.RegulatoryBody).HasDatabaseName("ix_alerts_regulatory_body");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_alerts_status");
            entity.HasIndex(e => e.Priority).HasDatabaseName("ix_alerts_priority");
            entity.HasIndex(e => e.DeadlineDate).HasDatabaseName("ix_alerts_deadline");

            entity.HasMany(e => e.Notifications)
                  .WithOne(n => n.RegulatoryAlert)
                  .HasForeignKey(n => n.RegulatoryAlertId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // AlertNotification
        modelBuilder.Entity<AlertNotification>(entity =>
        {
            entity.ToTable("alert_notifications");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RegulatoryAlertId).HasColumnName("regulatory_alert_id");
            entity.Property(e => e.UserId).HasColumnName("user_id").HasMaxLength(100);
            entity.Property(e => e.Channel).HasColumnName("channel").HasConversion<string>();
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.IsDelivered).HasColumnName("is_delivered");
            entity.Property(e => e.DeliveryError).HasColumnName("delivery_error").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_notifications_user");
            entity.HasIndex(e => e.IsDelivered).HasDatabaseName("ix_notifications_delivered");
        });

        // AlertSubscription
        modelBuilder.Entity<AlertSubscription>(entity =>
        {
            entity.ToTable("alert_subscriptions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").HasMaxLength(100).IsRequired();
            entity.Property(e => e.DealerId).HasColumnName("dealer_id").HasMaxLength(100);
            entity.Property(e => e.RegulatoryBody).HasColumnName("regulatory_body").HasConversion<string>();
            entity.Property(e => e.Category).HasColumnName("category").HasConversion<string>();
            entity.Property(e => e.AlertType).HasColumnName("alert_type").HasConversion<string>();
            entity.Property(e => e.MinimumPriority).HasColumnName("minimum_priority").HasConversion<string>();
            entity.Property(e => e.Frequency).HasColumnName("frequency").HasConversion<string>();
            entity.Property(e => e.PreferredChannel).HasColumnName("preferred_channel").HasConversion<string>();
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
            entity.Property(e => e.WebhookUrl).HasColumnName("webhook_url").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_subscriptions_user");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("ix_subscriptions_active");
        });

        // RegulatoryCalendarEntry
        modelBuilder.Entity<RegulatoryCalendarEntry>(entity =>
        {
            entity.ToTable("regulatory_calendar_entries");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(2000);
            entity.Property(e => e.RegulatoryBody).HasColumnName("regulatory_body").HasConversion<string>();
            entity.Property(e => e.Category).HasColumnName("category").HasConversion<string>();
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.IsRecurring).HasColumnName("is_recurring");
            entity.Property(e => e.RecurrencePattern).HasColumnName("recurrence_pattern").HasMaxLength(100);
            entity.Property(e => e.LegalBasis).HasColumnName("legal_basis").HasMaxLength(500);
            entity.Property(e => e.ReminderDaysBefore).HasColumnName("reminder_days_before");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.DueDate).HasDatabaseName("ix_calendar_due_date");
            entity.HasIndex(e => e.RegulatoryBody).HasDatabaseName("ix_calendar_body");
        });

        // ComplianceDeadline
        modelBuilder.Entity<ComplianceDeadline>(entity =>
        {
            entity.ToTable("compliance_deadlines");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(2000);
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.IsCompleted).HasColumnName("is_completed");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CompletedBy).HasColumnName("completed_by").HasMaxLength(100);
            entity.Property(e => e.CompletionNotes).HasColumnName("completion_notes").HasMaxLength(2000);
            entity.Property(e => e.Priority).HasColumnName("priority").HasConversion<string>();
            entity.Property(e => e.CalendarEntryId).HasColumnName("calendar_entry_id");
            entity.Property(e => e.AlertId).HasColumnName("alert_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_deadlines_user");
            entity.HasIndex(e => e.DueDate).HasDatabaseName("ix_deadlines_due_date");
            entity.HasIndex(e => e.IsCompleted).HasDatabaseName("ix_deadlines_completed");
        });
    }
}

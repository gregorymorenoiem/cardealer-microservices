using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public class ScheduledNotificationConfiguration : IEntityTypeConfiguration<ScheduledNotification>
{
    public void Configure(EntityTypeBuilder<ScheduledNotification> builder)
    {
        builder.ToTable("scheduled_notifications");

        builder.HasKey(sn => sn.Id);

        builder.Property(sn => sn.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(sn => sn.NotificationId)
            .HasColumnName("notification_id")
            .IsRequired();

        builder.Property(sn => sn.ScheduledFor)
            .HasColumnName("scheduled_for")
            .IsRequired();

        builder.Property(sn => sn.TimeZone)
            .HasColumnName("time_zone")
            .HasMaxLength(50)
            .HasDefaultValue("UTC");

        builder.Property(sn => sn.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(sn => sn.IsRecurring)
            .HasColumnName("is_recurring")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sn => sn.RecurrenceType)
            .HasColumnName("recurrence_type")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(sn => sn.CronExpression)
            .HasColumnName("cron_expression")
            .HasMaxLength(100);

        builder.Property(sn => sn.NextExecution)
            .HasColumnName("next_execution");

        builder.Property(sn => sn.LastExecution)
            .HasColumnName("last_execution");

        builder.Property(sn => sn.ExecutionCount)
            .HasColumnName("execution_count")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sn => sn.MaxExecutions)
            .HasColumnName("max_executions");

        builder.Property(sn => sn.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(sn => sn.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(sn => sn.CancelledAt)
            .HasColumnName("cancelled_at");

        builder.Property(sn => sn.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("System");

        builder.Property(sn => sn.CancelledBy)
            .HasColumnName("cancelled_by")
            .HasMaxLength(100);

        builder.Property(sn => sn.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(500);

        builder.Property(sn => sn.FailureCount)
            .HasColumnName("failure_count")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sn => sn.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(sn => sn.Notification)
            .WithMany()
            .HasForeignKey(sn => sn.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(sn => sn.NotificationId);
        builder.HasIndex(sn => sn.Status);
        builder.HasIndex(sn => sn.ScheduledFor);
        builder.HasIndex(sn => sn.NextExecution);
        builder.HasIndex(sn => sn.IsRecurring);
        builder.HasIndex(sn => new { sn.Status, sn.NextExecution }); // Composite for due queries
        builder.HasIndex(sn => new { sn.Status, sn.ScheduledFor }); // Composite for scheduled queries
    }
}

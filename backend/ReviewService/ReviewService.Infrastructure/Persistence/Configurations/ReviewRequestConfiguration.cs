using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewService.Domain.Entities;

namespace ReviewService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración EF para ReviewRequest
/// </summary>
public class ReviewRequestConfiguration : IEntityTypeConfiguration<ReviewRequest>
{
    public void Configure(EntityTypeBuilder<ReviewRequest> builder)
    {
        builder.ToTable("review_requests");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.SellerId)
            .IsRequired()
            .HasColumnName("seller_id");

        builder.Property(x => x.BuyerId)
            .IsRequired()
            .HasColumnName("buyer_id");

        builder.Property(x => x.VehicleId)
            .HasColumnName("vehicle_id");

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id");

        builder.Property(x => x.BuyerEmail)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("buyer_email");

        builder.Property(x => x.BuyerName)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("buyer_name");

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("request_token");

        builder.Property(x => x.RequestSentAt)
            .IsRequired()
            .HasColumnName("requested_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.ExpiresAt)
            .IsRequired()
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(ReviewRequestStatus.Sent)
            .HasColumnName("status");

        builder.Property(x => x.ReviewCreatedAt)
            .HasColumnName("completed_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.RemindersSent)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("reminders_sent");

        builder.Property(x => x.LastReminderAt)
            .HasColumnName("last_reminder_sent")
            .HasColumnType("timestamptz");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        // Índices
        builder.HasIndex(x => x.Token)
            .IsUnique()
            .HasDatabaseName("ix_review_requests_token");

        builder.HasIndex(x => new { x.SellerId, x.BuyerId, x.VehicleId })
            .HasDatabaseName("ix_review_requests_seller_buyer_vehicle");

        builder.HasIndex(x => x.BuyerEmail)
            .HasDatabaseName("ix_review_requests_buyer_email");

        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("ix_review_requests_expires_at");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_review_requests_status");

        builder.HasIndex(x => x.RequestSentAt)
            .HasDatabaseName("ix_review_requests_requested_at");

        builder.HasIndex(x => new { x.Status, x.ExpiresAt })
            .HasDatabaseName("ix_review_requests_status_expires");
    }
}
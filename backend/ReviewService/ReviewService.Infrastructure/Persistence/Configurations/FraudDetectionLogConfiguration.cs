using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewService.Domain.Entities;

namespace ReviewService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración EF para FraudDetectionLog
/// </summary>
public class FraudDetectionLogConfiguration : IEntityTypeConfiguration<FraudDetectionLog>
{
    public void Configure(EntityTypeBuilder<FraudDetectionLog> builder)
    {
        builder.ToTable("fraud_detection_logs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ReviewId)
            .IsRequired()
            .HasColumnName("review_id");

        builder.Property(x => x.CheckType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("check_type");

        builder.Property(x => x.Result)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("result");

        builder.Property(x => x.ConfidenceScore)
            .IsRequired()
            .HasColumnName("confidence_score");

        builder.Property(x => x.Details)
            .IsRequired()
            .HasMaxLength(1000)
            .HasColumnName("details");

        builder.Property(x => x.CheckedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("checked_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb")
            .HasColumnName("metadata");

        // Índices
        builder.HasIndex(x => x.ReviewId)
            .HasDatabaseName("ix_fraud_detection_logs_review_id");

        builder.HasIndex(x => x.CheckType)
            .HasDatabaseName("ix_fraud_detection_logs_check_type");

        builder.HasIndex(x => x.Result)
            .HasDatabaseName("ix_fraud_detection_logs_result");

        builder.HasIndex(x => x.CheckedAt)
            .HasDatabaseName("ix_fraud_detection_logs_checked_at");

        builder.HasIndex(x => new { x.CheckType, x.Result })
            .HasDatabaseName("ix_fraud_detection_logs_type_result");

        builder.HasIndex(x => x.ConfidenceScore)
            .HasDatabaseName("ix_fraud_detection_logs_confidence_score");

        // Relación con Review
        builder.HasOne(x => x.Review)
            .WithMany()
            .HasForeignKey(x => x.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
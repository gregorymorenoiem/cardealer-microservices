using AuditService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditService.Infrastructure.Persistence.EntityConfigurations;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events", "audit");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Source)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Payload)
            .IsRequired()
            .HasColumnType("jsonb"); // PostgreSQL JSONB for efficient querying

        builder.Property(e => e.EventTimestamp)
            .IsRequired();

        builder.Property(e => e.ConsumedAt)
            .IsRequired();

        builder.Property(e => e.CorrelationId)
            .HasMaxLength(100);

        builder.Property(e => e.UserId)
            .IsRequired(false);

        builder.Property(e => e.Metadata)
            .HasColumnType("jsonb");

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired(false);

        // Indexes for common queries
        builder.HasIndex(e => e.EventId)
            .HasDatabaseName("IX_AuditEvents_EventId");

        builder.HasIndex(e => e.EventType)
            .HasDatabaseName("IX_AuditEvents_EventType");

        builder.HasIndex(e => e.Source)
            .HasDatabaseName("IX_AuditEvents_Source");

        builder.HasIndex(e => e.EventTimestamp)
            .HasDatabaseName("IX_AuditEvents_EventTimestamp");

        builder.HasIndex(e => e.ConsumedAt)
            .HasDatabaseName("IX_AuditEvents_ConsumedAt");

        builder.HasIndex(e => e.CorrelationId)
            .HasDatabaseName("IX_AuditEvents_CorrelationId");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_AuditEvents_UserId");
    }
}

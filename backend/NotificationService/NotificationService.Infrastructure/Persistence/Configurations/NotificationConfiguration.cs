using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using System.Text.Json;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.Recipient)
            .HasColumnName("recipient")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(n => n.Subject)
            .HasColumnName("subject")
            .HasMaxLength(500);

        builder.Property(n => n.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.Provider)
            .HasColumnName("provider")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.Priority)
            .HasColumnName("priority")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(n => n.SentAt)
            .HasColumnName("sent_at");

        builder.Property(n => n.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        builder.Property(n => n.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(1000);

        builder.Property(n => n.TemplateName)
            .HasColumnName("template_name")
            .HasMaxLength(100);

        builder.Property(n => n.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions())
            );

        // Índices
        builder.HasIndex(n => n.Type);
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.Recipient);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.Provider);
        builder.HasIndex(n => new { n.Type, n.Status });
        builder.HasIndex(n => new { n.Recipient, n.CreatedAt });
    }
}
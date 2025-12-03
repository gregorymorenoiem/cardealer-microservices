using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using System.Text.Json;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("notification_templates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Subject)
            .HasColumnName("subject")
            .HasMaxLength(500);

        builder.Property(t => t.Body)
            .HasColumnName("body")
            .IsRequired();

        builder.Property(t => t.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(t => t.Category)
            .HasColumnName("category")
            .HasMaxLength(100);

        builder.Property(t => t.Variables)
            .HasColumnName("variables")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, new JsonSerializerOptions())
            );

        // ✅ New fields
        builder.Property(t => t.Version)
            .HasColumnName("version")
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(t => t.PreviousVersionId)
            .HasColumnName("previous_version_id");

        builder.Property(t => t.Tags)
            .HasColumnName("tags")
            .HasMaxLength(500);

        builder.Property(t => t.ValidationRules)
            .HasColumnName("validation_rules")
            .HasColumnType("jsonb");

        builder.Property(t => t.PreviewData)
            .HasColumnName("preview_data")
            .HasColumnType("jsonb");

        builder.Property(t => t.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("System");

        builder.Property(t => t.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.HasIndex(t => t.Type);
        builder.HasIndex(t => t.IsActive);
        builder.HasIndex(t => t.Category);
        builder.HasIndex(t => t.Version);
        builder.HasIndex(t => t.PreviousVersionId);
    }
}
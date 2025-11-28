using AuditService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditService.Infrastructure.Persistence.EntityConfigurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Resource)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.UserIp)
            .IsRequired()
            .HasMaxLength(45); // Soporta IPv6

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(100);

        builder.Property(x => x.ServiceName)
            .IsRequired()
            .HasMaxLength(100);

        // Configuración para AdditionalDataJson (JSON)
        builder.Property(x => x.AdditionalDataJson)
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Índices para mejor performance
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.Resource);
        builder.HasIndex(x => x.ServiceName);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
        builder.HasIndex(x => new { x.Action, x.CreatedAt });
        builder.HasIndex(x => new { x.ServiceName, x.CreatedAt });
        builder.HasIndex(x => x.Success);
        builder.HasIndex(x => x.Severity);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities.Privacy;

namespace UserService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de EF Core para PrivacyRequest
/// Usa nombres PascalCase para coincidir con la convención existente en la base de datos
/// </summary>
public class PrivacyRequestConfiguration : IEntityTypeConfiguration<PrivacyRequest>
{
    public void Configure(EntityTypeBuilder<PrivacyRequest> builder)
    {
        // Usar nombre de tabla en PascalCase para coincidir con convención existente
        builder.ToTable("PrivacyRequests");

        builder.HasKey(pr => pr.Id);

        // No renombrar columnas - usar nombres por defecto (PascalCase)
        builder.Property(pr => pr.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(pr => pr.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(pr => pr.ExportFormat)
            .HasConversion<string?>();

        builder.Property(pr => pr.DownloadToken)
            .HasMaxLength(500);

        builder.Property(pr => pr.FilePath)
            .HasMaxLength(1000);

        builder.Property(pr => pr.DeletionReason)
            .HasMaxLength(100);

        builder.Property(pr => pr.DeletionReasonOther)
            .HasMaxLength(500);

        builder.Property(pr => pr.ConfirmationCode)
            .HasMaxLength(10);

        builder.Property(pr => pr.IsConfirmed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pr => pr.IpAddress)
            .HasMaxLength(50);

        builder.Property(pr => pr.UserAgent)
            .HasMaxLength(500);

        builder.Property(pr => pr.Description)
            .HasMaxLength(2000);

        builder.Property(pr => pr.AdminNotes)
            .HasMaxLength(2000);

        // Relación con User
        builder.HasOne(pr => pr.User)
            .WithMany()
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(pr => pr.UserId);

        builder.HasIndex(pr => new { pr.UserId, pr.Type, pr.Status });

        builder.HasIndex(pr => pr.ConfirmationCode);

        builder.HasIndex(pr => pr.GracePeriodEndsAt);
    }
}

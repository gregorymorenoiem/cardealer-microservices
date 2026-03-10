using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities.Privacy;

namespace UserService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de EF Core para ConsentRecord — auditoría de cambios de consentimiento
/// Cumplimiento Ley 172-13 Art. 27: trazabilidad de opt-in/opt-out
/// </summary>
public class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> builder)
    {
        builder.ToTable("ConsentRecords");

        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.ConsentType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cr => cr.Source)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(cr => cr.IpAddress)
            .HasMaxLength(50);

        builder.Property(cr => cr.UserAgent)
            .HasMaxLength(500);

        builder.HasIndex(cr => cr.UserId);
        builder.HasIndex(cr => cr.Timestamp);
        builder.HasIndex(cr => new { cr.UserId, cr.ConsentType });

        builder.HasOne(cr => cr.User)
            .WithMany()
            .HasForeignKey(cr => cr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

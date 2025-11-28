using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace AuthService.Infrastructure.Persistence.EntityConfigurations;

public class VerificationTokenConfiguration : IEntityTypeConfiguration<VerificationToken>
{
    public void Configure(EntityTypeBuilder<VerificationToken> builder)
    {
        builder.ToTable("VerificationTokens");

        builder.HasKey(vt => vt.Id);

        builder.Property(vt => vt.Id)
            .ValueGeneratedOnAdd();

        builder.Property(vt => vt.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(vt => vt.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(vt => vt.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(vt => vt.ExpiresAt)
            .IsRequired();

        builder.Property(vt => vt.CreatedAt)
            .IsRequired();

        builder.Property(vt => vt.UsedAt);

        builder.Property(vt => vt.IsUsed)
            .HasDefaultValue(false);

        builder.Property(vt => vt.UserId)
            .IsRequired()
            .HasMaxLength(450);

        // Relación con User
        builder.HasOne<ApplicationUser>()
            .WithMany(u => u.VerificationTokens)
            .HasForeignKey(vt => vt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(vt => vt.Token)
            .IsUnique();

        builder.HasIndex(vt => vt.Email);

        builder.HasIndex(vt => vt.Type);

        builder.HasIndex(vt => vt.ExpiresAt);

        builder.HasIndex(vt => vt.CreatedAt);

        builder.HasIndex(vt => new { vt.Email, vt.Type, vt.IsUsed });

        // Ignorar propiedades computadas
        builder.Ignore(vt => vt.IsValid());
    }
}

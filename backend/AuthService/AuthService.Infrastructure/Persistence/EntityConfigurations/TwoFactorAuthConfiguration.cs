using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace AuthService.Infrastructure.Persistence.EntityConfigurations;

public class TwoFactorAuthConfiguration : IEntityTypeConfiguration<TwoFactorAuth>
{
    public void Configure(EntityTypeBuilder<TwoFactorAuth> builder)
    {
        builder.ToTable("TwoFactorAuths");
        builder.HasKey(t => t.Id);

        // Relación 1:1 con ApplicationUser
        builder.HasOne(t => t.User)
            .WithOne(u => u.TwoFactorAuth)
            .HasForeignKey<TwoFactorAuth>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configurar propiedades
        builder.Property(t => t.Secret).IsRequired().HasMaxLength(255);
        builder.Property(t => t.PhoneNumber).HasMaxLength(20);
        builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.PrimaryMethod).IsRequired().HasConversion<string>().HasMaxLength(50);

        // Configurar listas como JSON
        builder.Property(t => t.RecoveryCodes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            ).HasColumnType("text");

        builder.Property(t => t.EnabledMethods)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<TwoFactorAuthType>>(v, (JsonSerializerOptions?)null) ?? new List<TwoFactorAuthType>()
            ).HasColumnType("text");

        builder.Property(t => t.EnabledAt).IsRequired(false);
        builder.Property(t => t.LastUsedAt).IsRequired(false);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired(false);
        builder.Property(t => t.FailedAttempts).HasDefaultValue(0);

        // Índices
        builder.HasIndex(t => t.UserId).IsUnique();
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.PrimaryMethod);
    }
}
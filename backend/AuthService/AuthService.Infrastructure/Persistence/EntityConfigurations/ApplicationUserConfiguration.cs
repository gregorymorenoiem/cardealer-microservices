using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.EntityConfigurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        // Configurar propiedades
        builder.Property(u => u.Id).IsRequired().HasMaxLength(450);
        builder.Property(u => u.UserName).IsRequired().HasMaxLength(256);
        builder.Property(u => u.NormalizedUserName).HasMaxLength(256);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.NormalizedEmail).HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(1024);
        builder.Property(u => u.SecurityStamp).HasMaxLength(1024);
        builder.Property(u => u.ConcurrencyStamp).HasMaxLength(1024);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt);
        builder.Property(u => u.AccessFailedCount).HasDefaultValue(0);
        builder.Property(u => u.LockoutEnabled).HasDefaultValue(true);

        // Configurar relaciones
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.VerificationTokens)
            .WithOne()
            .HasForeignKey(vt => vt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación 1:1 con TwoFactorAuth
        builder.HasOne(u => u.TwoFactorAuth)
            .WithOne(t => t.User)
            .HasForeignKey<TwoFactorAuth>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
        builder.HasIndex(u => u.NormalizedUserName).IsUnique().HasDatabaseName("UserNameIndex");
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.CreatedAt);
    }
}

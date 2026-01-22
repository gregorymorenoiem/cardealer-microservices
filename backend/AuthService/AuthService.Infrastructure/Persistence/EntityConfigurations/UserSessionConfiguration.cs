using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.EntityConfigurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(s => s.RefreshTokenId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(s => s.DeviceInfo)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.Browser)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(s => s.OperatingSystem)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(s => s.IpAddress)
            .IsRequired()
            .HasMaxLength(45); // IPv6 max length

        builder.Property(s => s.Location)
            .HasMaxLength(256);

        builder.Property(s => s.Country)
            .HasMaxLength(100);

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.RevokedReason)
            .HasMaxLength(256);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.RefreshTokenId);
        builder.HasIndex(s => new { s.UserId, s.IsRevoked });

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.EntityConfigurations;

public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.ToTable("LoginHistories");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(l => l.DeviceInfo)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.Browser)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(l => l.OperatingSystem)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(l => l.IpAddress)
            .IsRequired()
            .HasMaxLength(45);

        builder.Property(l => l.Location)
            .HasMaxLength(256);

        builder.Property(l => l.Country)
            .HasMaxLength(100);

        builder.Property(l => l.City)
            .HasMaxLength(100);

        builder.Property(l => l.FailureReason)
            .HasMaxLength(512);

        builder.Property(l => l.Method)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.TwoFactorMethod)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => l.IpAddress);
        builder.HasIndex(l => new { l.UserId, l.LoginTime });
        builder.HasIndex(l => new { l.UserId, l.Success, l.LoginTime });

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

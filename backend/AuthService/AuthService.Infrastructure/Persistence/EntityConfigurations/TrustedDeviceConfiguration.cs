using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// US-18.4: Entity configuration for TrustedDevice.
/// </summary>
public class TrustedDeviceConfiguration : IEntityTypeConfiguration<TrustedDevice>
{
    public void Configure(EntityTypeBuilder<TrustedDevice> builder)
    {
        builder.ToTable("trusted_devices");

        builder.HasKey(td => td.Id);

        builder.Property(td => td.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(td => td.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(td => td.FingerprintHash)
            .HasColumnName("fingerprint_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(td => td.DeviceName)
            .HasColumnName("device_name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(td => td.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(1024);

        builder.Property(td => td.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(td => td.Location)
            .HasColumnName("location")
            .HasMaxLength(256);

        builder.Property(td => td.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(td => td.LastUsedAt)
            .HasColumnName("last_used_at")
            .IsRequired();

        builder.Property(td => td.LoginCount)
            .HasColumnName("login_count")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(td => td.IsTrusted)
            .HasColumnName("is_trusted")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(td => td.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(td => td.RevokeReason)
            .HasColumnName("revoke_reason")
            .HasMaxLength(256);

        // Indexes for performance
        builder.HasIndex(td => td.UserId)
            .HasDatabaseName("ix_trusted_devices_user_id");

        builder.HasIndex(td => new { td.UserId, td.FingerprintHash })
            .HasDatabaseName("ix_trusted_devices_user_fingerprint")
            .IsUnique();

        builder.HasIndex(td => new { td.UserId, td.IsTrusted })
            .HasDatabaseName("ix_trusted_devices_user_trusted");
    }
}

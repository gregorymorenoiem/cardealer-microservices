using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence.Configurations;

public class SellerConversionConfiguration : IEntityTypeConfiguration<SellerConversion>
{
    public void Configure(EntityTypeBuilder<SellerConversion> builder)
    {
        builder.ToTable("seller_conversions");

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.UserId)
            .IsRequired();

        builder.Property(sc => sc.SellerProfileId)
            .IsRequired();

        builder.Property(sc => sc.Source)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("conversion");

        builder.Property(sc => sc.PreviousAccountType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(sc => sc.NewAccountType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(sc => sc.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(sc => sc.KycProfileId);

        builder.Property(sc => sc.IdempotencyKey)
            .HasMaxLength(128);

        builder.Property(sc => sc.CorrelationId)
            .HasMaxLength(128);

        builder.Property(sc => sc.IpAddress)
            .HasMaxLength(45);

        builder.Property(sc => sc.UserAgent)
            .HasMaxLength(512);

        builder.Property(sc => sc.Notes)
            .HasMaxLength(1000);

        builder.Property(sc => sc.RequestedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(sc => sc.UserId)
            .IsUnique()
            .HasDatabaseName("IX_seller_conversions_user_id");

        builder.HasIndex(sc => sc.IdempotencyKey)
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL")
            .HasDatabaseName("IX_seller_conversions_idempotency_key");

        builder.HasIndex(sc => sc.Status)
            .HasDatabaseName("IX_seller_conversions_status");

        builder.HasIndex(sc => sc.SellerProfileId)
            .HasDatabaseName("IX_seller_conversions_seller_profile_id");
    }
}

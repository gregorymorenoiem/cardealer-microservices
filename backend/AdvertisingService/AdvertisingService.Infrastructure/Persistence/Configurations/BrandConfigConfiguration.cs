using AdvertisingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdvertisingService.Infrastructure.Persistence.Configurations;

public class BrandConfigConfiguration : IEntityTypeConfiguration<BrandConfig>
{
    public void Configure(EntityTypeBuilder<BrandConfig> builder)
    {
        builder.ToTable("brand_configs");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BrandKey).HasMaxLength(100).IsRequired();
        builder.Property(b => b.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(b => b.LogoUrl).HasMaxLength(500);
        builder.Property(b => b.LogoInitials).HasMaxLength(3).IsRequired();
        builder.Property(b => b.Route).HasMaxLength(200).IsRequired();

        builder.HasIndex(b => b.BrandKey).IsUnique();
    }
}

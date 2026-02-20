using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdvertisingService.Infrastructure.Persistence.Configurations;

public class AdCampaignConfiguration : IEntityTypeConfiguration<AdCampaign>
{
    public void Configure(EntityTypeBuilder<AdCampaign> builder)
    {
        builder.ToTable("ad_campaigns");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.OwnerType).HasMaxLength(20).IsRequired();
        builder.Property(c => c.PlacementType).HasConversion<int>().IsRequired();
        builder.Property(c => c.PricingModel).HasConversion<int>().IsRequired();
        builder.Property(c => c.Status).HasConversion<int>().IsRequired();
        builder.Property(c => c.PricePerView).HasColumnType("decimal(10,4)");
        builder.Property(c => c.FixedPrice).HasColumnType("decimal(18,2)");
        builder.Property(c => c.TotalBudget).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(c => c.SpentBudget).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        builder.Property(c => c.QualityScore).HasColumnType("decimal(3,2)").HasDefaultValue(0.50m);

        builder.HasIndex(c => new { c.PlacementType, c.Status }).HasDatabaseName("idx_ad_campaigns_placement_status");
        builder.HasIndex(c => new { c.OwnerId, c.OwnerType }).HasDatabaseName("idx_ad_campaigns_owner");
        builder.HasIndex(c => c.VehicleId).HasDatabaseName("idx_ad_campaigns_vehicle");

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

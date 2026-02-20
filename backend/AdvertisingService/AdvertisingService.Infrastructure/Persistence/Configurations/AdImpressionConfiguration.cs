using AdvertisingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdvertisingService.Infrastructure.Persistence.Configurations;

public class AdImpressionConfiguration : IEntityTypeConfiguration<AdImpression>
{
    public void Configure(EntityTypeBuilder<AdImpression> builder)
    {
        builder.ToTable("ad_impressions");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.SessionId).HasMaxLength(100);
        builder.Property(i => i.IpHash).HasMaxLength(64);

        builder.HasOne(i => i.Campaign)
            .WithMany()
            .HasForeignKey(i => i.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => new { i.CampaignId, i.RecordedAt }).HasDatabaseName("idx_ad_impressions_campaign");
    }
}

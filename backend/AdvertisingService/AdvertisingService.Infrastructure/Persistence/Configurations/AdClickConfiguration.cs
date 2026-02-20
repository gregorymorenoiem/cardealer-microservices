using AdvertisingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdvertisingService.Infrastructure.Persistence.Configurations;

public class AdClickConfiguration : IEntityTypeConfiguration<AdClick>
{
    public void Configure(EntityTypeBuilder<AdClick> builder)
    {
        builder.ToTable("ad_clicks");
        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.Campaign)
            .WithMany()
            .HasForeignKey(c => c.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.CampaignId, c.RecordedAt }).HasDatabaseName("idx_ad_clicks_campaign");
    }
}

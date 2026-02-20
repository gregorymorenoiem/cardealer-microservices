using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdvertisingService.Infrastructure.Persistence.Configurations;

public class RotationConfigConfiguration : IEntityTypeConfiguration<RotationConfig>
{
    public void Configure(EntityTypeBuilder<RotationConfig> builder)
    {
        builder.ToTable("rotation_configs");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Section).HasConversion<int>().IsRequired();
        builder.Property(r => r.AlgorithmType).HasConversion<int>().HasDefaultValue(RotationAlgorithmType.WeightedRandom);
        builder.Property(r => r.WeightRemainingBudget).HasColumnType("decimal(3,2)").HasDefaultValue(0.30m);
        builder.Property(r => r.WeightCtr).HasColumnType("decimal(3,2)").HasDefaultValue(0.25m);
        builder.Property(r => r.WeightQualityScore).HasColumnType("decimal(3,2)").HasDefaultValue(0.25m);
        builder.Property(r => r.WeightRecency).HasColumnType("decimal(3,2)").HasDefaultValue(0.20m);

        builder.HasIndex(r => r.Section).IsUnique();
    }
}

using AdvertisingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdvertisingService.Infrastructure.Persistence.Configurations;

public class CategoryImageConfigConfiguration : IEntityTypeConfiguration<CategoryImageConfig>
{
    public void Configure(EntityTypeBuilder<CategoryImageConfig> builder)
    {
        builder.ToTable("category_image_configs");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CategoryKey).HasMaxLength(50).IsRequired();
        builder.Property(c => c.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(300);
        builder.Property(c => c.ImageUrl).HasMaxLength(500).IsRequired();
        builder.Property(c => c.IconUrl).HasMaxLength(500);
        builder.Property(c => c.Gradient).HasMaxLength(100).HasDefaultValue("from-blue-600 to-blue-800");
        builder.Property(c => c.Route).HasMaxLength(200).IsRequired();

        builder.HasIndex(c => c.CategoryKey).IsUnique();
    }
}

using MediaService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaService.Infrastructure.Persistence.Configurations;

public class MediaVariantConfiguration : IEntityTypeConfiguration<MediaVariant>
{
    public void Configure(EntityTypeBuilder<MediaVariant> builder)
    {
        builder.ToTable("media_variants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.MediaAssetId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.StorageKey)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(x => x.CdnUrl)
            .HasMaxLength(1024);

        builder.Property(x => x.Format)
            .HasMaxLength(20);

        builder.Property(x => x.VideoProfile)
            .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Relación con MediaAsset
        builder.HasOne(x => x.MediaAsset)
            .WithMany(x => x.Variants)
            .HasForeignKey(x => x.MediaAssetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices compuestos para mejor performance
        builder.HasIndex(x => x.MediaAssetId);
        builder.HasIndex(x => new { x.MediaAssetId, x.Name })
            .IsUnique();
        builder.HasIndex(x => x.StorageKey);
    }
}
using MediaService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaService.Infrastructure.Persistence.Configurations;

public class ImageMediaConfiguration : IEntityTypeConfiguration<ImageMedia>
{
    public void Configure(EntityTypeBuilder<ImageMedia> builder)
    {
        builder.ToTable("image_media");

        builder.Property(x => x.Width)
            .IsRequired();

        builder.Property(x => x.Height)
            .IsRequired();

        builder.Property(x => x.HashSha256)
            .HasMaxLength(64);

        builder.Property(x => x.AltText)
            .HasMaxLength(500);

        // Índices específicos para imágenes
        builder.HasIndex(x => x.Width);
        builder.HasIndex(x => x.Height);
        builder.HasIndex(x => x.IsPrimary);
    }
}
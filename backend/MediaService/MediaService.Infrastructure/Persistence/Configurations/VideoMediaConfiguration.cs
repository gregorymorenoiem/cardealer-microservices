using MediaService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaService.Infrastructure.Persistence.Configurations;

public class VideoMediaConfiguration : IEntityTypeConfiguration<VideoMedia>
{
    public void Configure(EntityTypeBuilder<VideoMedia> builder)
    {
        builder.ToTable("video_media");

        builder.Property(x => x.Duration);

        builder.Property(x => x.Width);

        builder.Property(x => x.Height);

        builder.Property(x => x.VideoCodec)
            .HasMaxLength(50);

        builder.Property(x => x.AudioCodec)
            .HasMaxLength(50);

        builder.Property(x => x.FrameRate);

        builder.Property(x => x.Bitrate);

        builder.Property(x => x.StorageKeyInput)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(x => x.StorageKeyHls)
            .HasMaxLength(1024);

        // Índices específicos para videos
        builder.HasIndex(x => x.Duration);
        builder.HasIndex(x => x.HasHlsStream);
    }
}
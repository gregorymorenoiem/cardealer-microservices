namespace MediaService.Application.DTOs;

public class MediaVariantDto
{
    public string Id { get; set; } = string.Empty;
    public string MediaAssetId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string? CdnUrl { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long SizeBytes { get; set; }
    public string? Format { get; set; }
    public int? Quality { get; set; }
    public string? VideoProfile { get; set; }
    public int? Bitrate { get; set; }
    public TimeSpan? Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static MediaVariantDto FromEntity(Domain.Entities.MediaVariant entity)
    {
        return new MediaVariantDto
        {
            Id = entity.Id,
            MediaAssetId = entity.MediaAssetId,
            Name = entity.Name,
            StorageKey = entity.StorageKey,
            CdnUrl = entity.CdnUrl,
            Width = entity.Width,
            Height = entity.Height,
            SizeBytes = entity.SizeBytes,
            Format = entity.Format,
            Quality = entity.Quality,
            VideoProfile = entity.VideoProfile,
            Bitrate = entity.Bitrate,
            Duration = entity.Duration,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
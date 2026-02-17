namespace MediaService.Application.DTOs;

public class MediaDto
{
    public string Id { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string? Context { get; set; }
    public string Type { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CdnUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessingError { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public List<MediaVariantDto> Variants { get; set; } = new();

    public static MediaDto FromEntity(Domain.Entities.MediaAsset entity)
    {
        return new MediaDto
        {
            Id = entity.Id,
            OwnerId = entity.OwnerId,
            Context = entity.Context,
            Type = entity.Type.ToString(),
            OriginalFileName = entity.OriginalFileName,
            ContentType = entity.ContentType,
            SizeBytes = entity.SizeBytes,
            Status = entity.Status.ToString(),
            StorageKey = entity.StorageKey,
            CdnUrl = entity.CdnUrl,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ProcessedAt = entity.ProcessedAt,
            ProcessingError = entity.ProcessingError,
            Variants = entity.Variants.Select(MediaVariantDto.FromEntity).ToList()
        };
    }
}
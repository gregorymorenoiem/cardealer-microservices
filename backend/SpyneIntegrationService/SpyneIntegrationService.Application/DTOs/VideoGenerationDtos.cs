using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Application.DTOs;

/// <summary>
/// DTO for video generation
/// </summary>
public record VideoGenerationDto
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public string? SpyneJobId { get; init; }
    public List<string> SourceImageUrls { get; init; } = new();
    public string? VideoUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
    public int DurationSeconds { get; init; }
    public VideoStyle Style { get; init; }
    public VideoFormat Format { get; init; }
    public VideoFormat OutputFormat { get; init; }
    public BackgroundPreset BackgroundPreset { get; init; }
    public bool IncludeMusic { get; init; }
    public string? BrandingLogoUrl { get; init; }
    public TransformationStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public int? ProcessingTimeMs { get; init; }
    public long? FileSizeBytes { get; init; }
    public decimal CreditsCost { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

/// <summary>
/// Request to generate a video
/// </summary>
public record GenerateVideoRequest
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public int DurationSeconds { get; init; } = 30;
    public VideoStyle Style { get; init; } = VideoStyle.Cinematic;
    public VideoFormat Format { get; init; } = VideoFormat.Horizontal;
    public bool IncludeMusic { get; init; } = true;
    public string? MusicTrackId { get; init; }
    public string? BrandingLogoUrl { get; init; }
}

/// <summary>
/// Response for video generation
/// </summary>
public record GenerateVideoResponse
{
    public Guid VideoId { get; init; }
    public TransformationStatus Status { get; init; }
    public int EstimatedCompletionMinutes { get; init; }
    public int DurationSeconds { get; init; }
    public VideoFormat Format { get; init; }
}

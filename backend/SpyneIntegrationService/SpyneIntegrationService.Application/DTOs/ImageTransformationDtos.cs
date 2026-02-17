using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Application.DTOs;

/// <summary>
/// DTO for image transformation
/// </summary>
public record ImageTransformationDto
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public string OriginalImageUrl { get; init; } = string.Empty;
    public string? TransformedImageUrl { get; init; }
    public string? TransformedImageUrlHd { get; init; }
    public string? SpyneJobId { get; init; }
    public BackgroundPreset BackgroundPreset { get; init; }
    public TransformationStatus Status { get; init; }
    public bool LicensePlateMasked { get; init; }
    public bool QualityEnhanced { get; init; }
    public string? ErrorMessage { get; init; }
    public int? ProcessingTimeMs { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

/// <summary>
/// Request to transform an image
/// </summary>
public record TransformImageRequest
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public string OriginalImageUrl { get; init; } = string.Empty;
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Showroom;
    public string? CustomBackgroundId { get; init; }
    public bool MaskLicensePlate { get; init; } = true;
    public bool EnhanceQuality { get; init; } = true;
}

/// <summary>
/// Request to transform multiple images
/// </summary>
public record TransformBatchRequest
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Showroom;
    public bool MaskLicensePlate { get; init; } = true;
    public bool EnhanceQuality { get; init; } = true;
}

/// <summary>
/// Response for batch transformation
/// </summary>
public record TransformBatchResponse
{
    public List<ImageTransformationDto> Transformations { get; init; } = new();
    public int TotalImages { get; init; }
    public int EstimatedCompletionSeconds { get; init; }
}

/// <summary>
/// Result DTO for batch transformation (alias for TransformBatchResponse)
/// </summary>
public record BatchTransformationResultDto
{
    public Guid BatchId { get; init; }
    public List<ImageTransformationDto> Transformations { get; init; } = new();
    public int TotalImages { get; init; }
    public int ProcessedImages { get; init; }
    public int FailedImages { get; init; }
    public int EstimatedCompletionSeconds { get; init; }
    public string Status { get; init; } = "Processing";
}

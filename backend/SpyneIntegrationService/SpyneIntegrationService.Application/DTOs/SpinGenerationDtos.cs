using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Application.DTOs;

/// <summary>
/// DTO for 360° spin generation
/// </summary>
public record SpinGenerationDto
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public string? SpyneJobId { get; init; }
    public List<string> SourceImageUrls { get; init; } = new();
    public string? SpinViewerUrl { get; init; }
    public string? SpinEmbedCode { get; init; }
    public string? FallbackImageUrl { get; init; }
    public BackgroundPreset BackgroundPreset { get; init; }
    public int RequiredImageCount { get; init; }
    public int ReceivedImageCount { get; init; }
    public TransformationStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public int? ProcessingTimeMs { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

/// <summary>
/// Request to generate a 360° spin
/// </summary>
public record GenerateSpinRequest
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Studio;
    public string? CustomBackgroundId { get; init; }
}

/// <summary>
/// Response for spin generation
/// </summary>
public record GenerateSpinResponse
{
    public Guid SpinId { get; init; }
    public TransformationStatus Status { get; init; }
    public int EstimatedCompletionMinutes { get; init; }
    public int RequiredImageCount { get; init; }
    public int ReceivedImageCount { get; init; }
    public bool HasSufficientImages { get; init; }
}

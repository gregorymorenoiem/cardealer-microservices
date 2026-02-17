using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Application.DTOs;

/// <summary>
/// DTO for background preset
/// </summary>
public record BackgroundPresetDto
{
    public Guid Id { get; init; }
    public BackgroundPreset Preset { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SpyneBackgroundId { get; init; } = string.Empty;
    public string PreviewUrl { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal CreditsCost { get; init; }
    public int SortOrder { get; init; }
}

/// <summary>
/// DTO for webhook payload from Spyne
/// </summary>
public record SpyneWebhookPayloadDto
{
    public string EventType { get; init; } = string.Empty;
    public string JobId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? ResultUrl { get; init; }
    public string? ResultUrlHd { get; init; }
    public string? EmbedCode { get; init; }
    public string? ThumbnailUrl { get; init; }
    public long? FileSizeBytes { get; init; }
    public int? ProcessingTimeMs { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public DateTime Timestamp { get; init; }
    public string? Signature { get; init; }
}

/// <summary>
/// Response after processing webhook
/// </summary>
public record WebhookProcessedResponse
{
    public bool Success { get; init; }
    public Guid? TransformationId { get; init; }
    public TransformationType? TransformationType { get; init; }
    public TransformationStatus NewStatus { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Result of webhook processing (used by handler)
/// </summary>
public record WebhookProcessingResult
{
    public bool Success { get; init; }
    public bool Acknowledged { get; init; }
    public string? Message { get; init; }
    public Guid? EntityId { get; init; }
    
    /// <summary>Alias for EntityId</summary>
    public Guid? TransformationId 
    { 
        get => EntityId; 
        init => EntityId = value; 
    }
    
    public string? EntityType { get; init; }
    public TransformationStatus? NewStatus { get; init; }
}

using MediatR;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Application.Features.Webhooks.Commands;

/// <summary>
/// Command to process Spyne webhook callback
/// </summary>
public record ProcessSpyneWebhookCommand : IRequest<WebhookProcessingResult>
{
    public string EventType { get; init; } = string.Empty;
    public string JobId { get; init; } = string.Empty;
    
    /// <summary>Alias for JobId</summary>
    public string SpyneJobId => JobId;
    
    public string Status { get; init; } = string.Empty;
    public string? ResultUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? EmbedCode { get; init; }
    public string? ErrorMessage { get; init; }
    public long? ProcessingTimeMs { get; init; }
    public long? FileSizeBytes { get; init; }
    public int? DurationSeconds { get; init; }
    
    /// <summary>Type of transformation</summary>
    public TransformationType TransformationType { get; init; }
    
    public Dictionary<string, object>? AdditionalData { get; init; }
    public string RawPayload { get; init; } = string.Empty;
}

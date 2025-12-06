namespace IntegrationService.Application.DTOs;

public record WebhookEventDto(
    Guid Id,
    Guid IntegrationId,
    string EventType,
    string EventName,
    string Status,
    string Payload,
    string? Response,
    int RetryCount,
    int MaxRetries,
    DateTime? NextRetryAt,
    DateTime ReceivedAt,
    DateTime? ProcessedAt,
    string? ErrorMessage
);

public record CreateWebhookEventRequest(
    Guid IntegrationId,
    string EventType,
    string EventName,
    string Payload,
    string? Headers = null,
    int MaxRetries = 3
);

public record ProcessWebhookEventRequest(
    bool Success,
    string? Response = null,
    string? ErrorMessage = null
);

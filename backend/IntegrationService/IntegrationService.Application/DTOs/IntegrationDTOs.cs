namespace IntegrationService.Application.DTOs;

public record IntegrationDto(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string Status,
    string? WebhookUrl,
    string? Configuration,
    DateTime? LastSyncAt,
    string? LastSyncStatus,
    string? LastError,
    DateTime CreatedAt
);

public record CreateIntegrationRequest(
    string Name,
    string Type,
    string? Description = null,
    string? ApiKey = null,
    string? ApiSecret = null,
    string? WebhookUrl = null,
    string? Configuration = null
);

public record UpdateIntegrationRequest(
    string Name,
    string? Description = null,
    string? Configuration = null
);

public record SetCredentialsRequest(
    string? ApiKey,
    string? ApiSecret
);

public record SetTokensRequest(
    string AccessToken,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null
);

public record SetWebhookRequest(
    string WebhookUrl,
    string? WebhookSecret = null
);

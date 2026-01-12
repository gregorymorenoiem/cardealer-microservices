namespace NotificationService.Application.DTOs;

/// <summary>
/// Notification preference for a specific notification type
/// </summary>
public record NotificationPreferenceDto(
    string Id,
    string Type,
    string Title,
    string Description,
    bool Enabled,
    List<string> Channels
);

/// <summary>
/// Request to update a notification preference
/// </summary>
public record UpdateNotificationPreferenceRequest(
    string Type,
    bool Enabled,
    List<string>? Channels = null
);

/// <summary>
/// Notification preferences response with all preferences
/// </summary>
public record NotificationPreferencesResponse(
    string UserId,
    string? DealerId,
    List<NotificationPreferenceDto> Preferences
);

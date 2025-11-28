using System.Collections.Generic;

namespace NotificationService.Application.DTOs;

public record SendPushNotificationRequest(
    string DeviceToken,
    string Title,
    string Body,
    object? Data = null,
    Dictionary<string, object>? Metadata = null
);
using System.Collections.Generic;

namespace NotificationService.Application.DTOs;

public record SendSmsNotificationRequest(
    string To,
    string Message,
    Dictionary<string, object>? Metadata = null
);
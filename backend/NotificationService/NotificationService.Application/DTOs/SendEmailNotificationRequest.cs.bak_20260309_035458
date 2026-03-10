using System.Collections.Generic;

namespace NotificationService.Application.DTOs;

public record SendEmailNotificationRequest(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true,
    Dictionary<string, object>? Metadata = null
);
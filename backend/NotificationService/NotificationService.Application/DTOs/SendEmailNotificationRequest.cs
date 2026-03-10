using System.Collections.Generic;

namespace NotificationService.Application.DTOs;

public record SendEmailNotificationRequest(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true,
    bool IsMarketing = false,
    Guid? RecipientUserId = null,
    Dictionary<string, object>? Metadata = null
);
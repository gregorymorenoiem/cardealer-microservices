namespace NotificationService.Application.DTOs;

public record SendEmailNotificationResponse(
    Guid NotificationId,
    string Status,
    string Message
);
namespace NotificationService.Application.DTOs;

public record SendSmsNotificationResponse(
    Guid NotificationId,
    string Status,
    string Message
);
namespace NotificationService.Application.DTOs;

public record SendPushNotificationResponse(
    Guid NotificationId,
    string Status,
    string Message
);
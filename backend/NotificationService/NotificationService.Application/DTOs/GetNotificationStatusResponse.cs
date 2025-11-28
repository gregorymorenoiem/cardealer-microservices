namespace NotificationService.Application.DTOs;

public record GetNotificationStatusResponse(
    Guid NotificationId,
    string Status,
    DateTime? SentAt,
    string? ErrorMessage
);
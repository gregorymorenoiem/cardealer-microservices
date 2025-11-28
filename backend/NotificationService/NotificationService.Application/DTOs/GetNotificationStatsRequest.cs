namespace NotificationService.Application.DTOs;

public record GetNotificationStatsRequest(
    DateTime? From = null,
    DateTime? To = null
);
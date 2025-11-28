namespace NotificationService.Application.DTOs
{
    public record GetNotificationStatsResponse(
        int TotalNotifications,
        int NotificationsLast24Hours,
        int NotificationsLast7Days,
        Dictionary<string, int> NotificationsByType
    );
}
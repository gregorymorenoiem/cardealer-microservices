using System.Collections.Generic;

namespace NotificationService.Application.DTOs;

public record GetNotificationStatsResponse(
    int TotalNotifications,
    int SentLast24Hours,
    int SentLast7Days,
    Dictionary<string, int> NotificationsByType,
    Dictionary<string, int> NotificationsByStatus
);
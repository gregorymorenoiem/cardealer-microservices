namespace NotificationService.Domain.Enums;

public enum ScheduledNotificationStatus
{
    Pending = 1,
    Processing = 2,
    Executed = 3,
    Failed = 4,
    Cancelled = 5,
    Completed = 6 // For recurring notifications that reached max executions
}

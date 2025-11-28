namespace NotificationService.Domain.Enums;

public enum QueueStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Retry = 5,
    Cancelled = 6
}
namespace NotificationService.Domain.Enums;

public enum RecurrencePattern
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4,
    Cron = 5 // Custom cron expression
}

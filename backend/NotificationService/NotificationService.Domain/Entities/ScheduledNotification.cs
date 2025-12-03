using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

public class ScheduledNotification
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Notification? Notification { get; set; }

    // Scheduling fields
    public DateTime ScheduledFor { get; set; } // UTC
    public string? TimeZone { get; set; } // IANA timezone (e.g., "America/New_York")
    public ScheduledNotificationStatus Status { get; set; }

    // Recurrence
    public bool IsRecurring { get; set; }
    public RecurrencePattern? RecurrenceType { get; set; }
    public string? CronExpression { get; set; } // For complex schedules
    public DateTime? NextExecution { get; set; } // Next scheduled execution
    public DateTime? LastExecution { get; set; } // Last execution time
    public int ExecutionCount { get; set; } // Number of times executed
    public int? MaxExecutions { get; set; } // Max executions (null = infinite)

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string CreatedBy { get; set; } = "System";
    public string? CancelledBy { get; set; }
    public string? CancellationReason { get; set; }

    // Error handling
    public int FailureCount { get; set; }
    public string? LastError { get; set; }

    // Constructor
    public ScheduledNotification()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Status = ScheduledNotificationStatus.Pending;
        ExecutionCount = 0;
        FailureCount = 0;
    }

    // Factory method - One-time notification
    public static ScheduledNotification CreateOneTime(Notification notification, DateTime scheduledFor,
        string? timeZone = null, string? createdBy = null)
    {
        return new ScheduledNotification
        {
            NotificationId = notification.Id,
            Notification = notification,
            ScheduledFor = scheduledFor,
            TimeZone = timeZone ?? "UTC",
            IsRecurring = false,
            CreatedBy = createdBy ?? "System"
        };
    }

    // Factory method - Recurring notification
    public static ScheduledNotification CreateRecurring(Notification notification, DateTime firstExecution,
        RecurrencePattern recurrenceType, string? timeZone = null, int? maxExecutions = null, string? createdBy = null)
    {
        var scheduled = new ScheduledNotification
        {
            NotificationId = notification.Id,
            Notification = notification,
            ScheduledFor = firstExecution,
            TimeZone = timeZone ?? "UTC",
            IsRecurring = true,
            RecurrenceType = recurrenceType,
            MaxExecutions = maxExecutions,
            NextExecution = firstExecution,
            CreatedBy = createdBy ?? "System"
        };

        return scheduled;
    }

    // Factory method - Cron-based notification
    public static ScheduledNotification CreateWithCron(Notification notification, string cronExpression,
        string? timeZone = null, int? maxExecutions = null, string? createdBy = null)
    {
        return new ScheduledNotification
        {
            NotificationId = notification.Id,
            Notification = notification,
            ScheduledFor = DateTime.UtcNow, // Will be calculated from cron
            TimeZone = timeZone ?? "UTC",
            IsRecurring = true,
            RecurrenceType = RecurrencePattern.Cron,
            CronExpression = cronExpression,
            MaxExecutions = maxExecutions,
            CreatedBy = createdBy ?? "System"
        };
    }

    // Business methods
    public void MarkAsProcessing()
    {
        Status = ScheduledNotificationStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsExecuted()
    {
        Status = ScheduledNotificationStatus.Executed;
        LastExecution = DateTime.UtcNow;
        ExecutionCount++;
        UpdatedAt = DateTime.UtcNow;

        // Check if should schedule next execution
        if (IsRecurring && (!MaxExecutions.HasValue || ExecutionCount < MaxExecutions.Value))
        {
            Status = ScheduledNotificationStatus.Pending;
        }
        else if (IsRecurring && MaxExecutions.HasValue && ExecutionCount >= MaxExecutions.Value)
        {
            Status = ScheduledNotificationStatus.Completed;
        }
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = ScheduledNotificationStatus.Failed;
        LastError = errorMessage;
        FailureCount++;
        UpdatedAt = DateTime.UtcNow;

        // If failed too many times, cancel
        if (FailureCount >= 5)
        {
            Cancel("Exceeded maximum failure count", "System");
        }
    }

    public void Cancel(string reason, string? cancelledBy = null)
    {
        Status = ScheduledNotificationStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;
        CancelledBy = cancelledBy ?? "System";
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSchedule(DateTime newScheduledFor)
    {
        if (Status != ScheduledNotificationStatus.Pending && Status != ScheduledNotificationStatus.Failed)
            throw new InvalidOperationException("Can only update schedule for pending or failed notifications");

        ScheduledFor = newScheduledFor;
        NextExecution = newScheduledFor;
        Status = ScheduledNotificationStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNextExecution(DateTime nextExecution)
    {
        NextExecution = nextExecution;
        ScheduledFor = nextExecution;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsDue()
    {
        return Status == ScheduledNotificationStatus.Pending &&
               (NextExecution ?? ScheduledFor) <= DateTime.UtcNow;
    }

    public bool CanExecute()
    {
        return Status == ScheduledNotificationStatus.Pending && IsDue();
    }
}

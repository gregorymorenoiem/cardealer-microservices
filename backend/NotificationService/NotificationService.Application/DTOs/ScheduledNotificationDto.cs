using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs;

public record ScheduleNotificationRequest(
    Guid NotificationId,
    DateTime ScheduledFor,
    string? TimeZone = null,
    bool IsRecurring = false,
    RecurrencePattern? RecurrenceType = null,
    string? CronExpression = null,
    int? MaxExecutions = null
);

public record ScheduledNotificationResponse(
    Guid Id,
    Guid NotificationId,
    DateTime ScheduledFor,
    string? TimeZone,
    ScheduledNotificationStatus Status,
    bool IsRecurring,
    RecurrencePattern? RecurrenceType,
    string? CronExpression,
    DateTime? NextExecution,
    DateTime? LastExecution,
    int ExecutionCount,
    int? MaxExecutions,
    DateTime CreatedAt,
    string CreatedBy
);

public record GetScheduledNotificationsRequest(
    ScheduledNotificationStatus? Status = null,
    DateTime? ScheduledFrom = null,
    DateTime? ScheduledTo = null,
    bool? IsRecurring = null,
    int PageNumber = 1,
    int PageSize = 20
);

public record GetScheduledNotificationsResponse(
    List<ScheduledNotificationResponse> ScheduledNotifications,
    int TotalCount,
    int PageNumber,
    int PageSize
);

using Cronos;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Interfaces.Repositories;
using TimeZoneConverter;

namespace NotificationService.Infrastructure.Services;

public interface ISchedulingService
{
    Task<ScheduledNotification> ScheduleOneTimeAsync(Notification notification, DateTime scheduledFor, string? timeZone = null, string? createdBy = null);
    Task<ScheduledNotification> ScheduleRecurringAsync(Notification notification, DateTime firstExecution, RecurrencePattern pattern, string? timeZone = null, int? maxExecutions = null, string? createdBy = null);
    Task<ScheduledNotification> ScheduleWithCronAsync(Notification notification, string cronExpression, string? timeZone = null, int? maxExecutions = null, string? createdBy = null);
    Task<bool> CancelAsync(Guid scheduledNotificationId, string reason, string? cancelledBy = null);
    Task<bool> RescheduleAsync(Guid scheduledNotificationId, DateTime newScheduledFor);
    DateTime? CalculateNextExecution(ScheduledNotification scheduledNotification);
    DateTime ConvertToUtc(DateTime localTime, string timeZone);
    DateTime ConvertFromUtc(DateTime utcTime, string timeZone);
}

public class SchedulingService : ISchedulingService
{
    private readonly IScheduledNotificationRepository _repository;

    public SchedulingService(IScheduledNotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ScheduledNotification> ScheduleOneTimeAsync(Notification notification, DateTime scheduledFor,
        string? timeZone = null, string? createdBy = null)
    {
        var utcScheduledFor = ConvertToUtc(scheduledFor, timeZone ?? "UTC");
        var scheduled = ScheduledNotification.CreateOneTime(notification, utcScheduledFor, timeZone, createdBy);
        await _repository.AddAsync(scheduled);
        return scheduled;
    }

    public async Task<ScheduledNotification> ScheduleRecurringAsync(Notification notification, DateTime firstExecution,
        RecurrencePattern pattern, string? timeZone = null, int? maxExecutions = null, string? createdBy = null)
    {
        var utcFirstExecution = ConvertToUtc(firstExecution, timeZone ?? "UTC");
        var scheduled = ScheduledNotification.CreateRecurring(notification, utcFirstExecution, pattern, timeZone, maxExecutions, createdBy);
        scheduled.SetNextExecution(utcFirstExecution);
        await _repository.AddAsync(scheduled);
        return scheduled;
    }

    public async Task<ScheduledNotification> ScheduleWithCronAsync(Notification notification, string cronExpression,
        string? timeZone = null, int? maxExecutions = null, string? createdBy = null)
    {
        var scheduled = ScheduledNotification.CreateWithCron(notification, cronExpression, timeZone, maxExecutions, createdBy);
        var nextExecution = CalculateNextExecutionFromCron(cronExpression, timeZone ?? "UTC");
        if (nextExecution.HasValue)
        {
            scheduled.SetNextExecution(nextExecution.Value);
        }
        await _repository.AddAsync(scheduled);
        return scheduled;
    }

    public async Task<bool> CancelAsync(Guid scheduledNotificationId, string reason, string? cancelledBy = null)
    {
        var scheduled = await _repository.GetByIdAsync(scheduledNotificationId);
        if (scheduled == null) return false;

        scheduled.Cancel(reason, cancelledBy);
        await _repository.UpdateAsync(scheduled);
        return true;
    }

    public async Task<bool> RescheduleAsync(Guid scheduledNotificationId, DateTime newScheduledFor)
    {
        var scheduled = await _repository.GetByIdAsync(scheduledNotificationId);
        if (scheduled == null) return false;

        var utcScheduledFor = ConvertToUtc(newScheduledFor, scheduled.TimeZone ?? "UTC");
        scheduled.UpdateSchedule(utcScheduledFor);
        await _repository.UpdateAsync(scheduled);
        return true;
    }

    public DateTime? CalculateNextExecution(ScheduledNotification scheduledNotification)
    {
        if (!scheduledNotification.IsRecurring)
            return null;

        if (scheduledNotification.RecurrenceType == RecurrencePattern.Cron && !string.IsNullOrWhiteSpace(scheduledNotification.CronExpression))
        {
            return CalculateNextExecutionFromCron(scheduledNotification.CronExpression, scheduledNotification.TimeZone ?? "UTC");
        }

        var lastExecution = scheduledNotification.LastExecution ?? scheduledNotification.ScheduledFor;
        var localTime = ConvertFromUtc(lastExecution, scheduledNotification.TimeZone ?? "UTC");

        var nextLocal = scheduledNotification.RecurrenceType switch
        {
            RecurrencePattern.Daily => localTime.AddDays(1),
            RecurrencePattern.Weekly => localTime.AddDays(7),
            RecurrencePattern.Monthly => localTime.AddMonths(1),
            RecurrencePattern.Yearly => localTime.AddYears(1),
            _ => localTime
        };

        return ConvertToUtc(nextLocal, scheduledNotification.TimeZone ?? "UTC");
    }

    private DateTime? CalculateNextExecutionFromCron(string cronExpression, string timeZone)
    {
        try
        {
            var expression = CronExpression.Parse(cronExpression);
            var tzInfo = TZConvert.GetTimeZoneInfo(timeZone);
            var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tzInfo);
            var next = expression.GetNextOccurrence(now, tzInfo);
            return next.HasValue ? TimeZoneInfo.ConvertTimeToUtc(next.Value, tzInfo) : null;
        }
        catch
        {
            return null;
        }
    }

    public DateTime ConvertToUtc(DateTime localTime, string timeZone)
    {
        if (timeZone == "UTC" || string.IsNullOrWhiteSpace(timeZone))
            return localTime;

        try
        {
            var tzInfo = TZConvert.GetTimeZoneInfo(timeZone);
            return TimeZoneInfo.ConvertTimeToUtc(localTime, tzInfo);
        }
        catch
        {
            return localTime;
        }
    }

    public DateTime ConvertFromUtc(DateTime utcTime, string timeZone)
    {
        if (timeZone == "UTC" || string.IsNullOrWhiteSpace(timeZone))
            return utcTime;

        try
        {
            var tzInfo = TZConvert.GetTimeZoneInfo(timeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzInfo);
        }
        catch
        {
            return utcTime;
        }
    }
}

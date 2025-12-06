using CarDealer.Shared.MultiTenancy;

namespace ReportsService.Domain.Entities;

public enum ScheduleFrequency
{
    Once,
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

public class ReportSchedule : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public Guid ReportId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public ScheduleFrequency Frequency { get; private set; }

    public string? CronExpression { get; private set; }
    public TimeOnly? ExecutionTime { get; private set; }
    public DayOfWeek? DayOfWeek { get; private set; }
    public int? DayOfMonth { get; private set; }

    public bool IsActive { get; private set; }

    public string? Recipients { get; private set; } // JSON array of emails
    public bool SendEmail { get; private set; }
    public bool SaveToStorage { get; private set; }

    public DateTime? LastRunAt { get; private set; }
    public DateTime? NextRunAt { get; private set; }
    public string? LastRunStatus { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // Navigation
    public Report? Report { get; private set; }

    private ReportSchedule() { }

    public ReportSchedule(
        Guid dealerId,
        Guid reportId,
        string name,
        ScheduleFrequency frequency,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        ReportId = reportId;
        Name = name;
        Frequency = frequency;
        CreatedBy = createdBy;
        IsActive = true;
        SendEmail = true;
        SaveToStorage = true;
        CreatedAt = DateTime.UtcNow;
        CalculateNextRun();
    }

    public void SetExecutionTime(TimeOnly time)
    {
        ExecutionTime = time;
        CalculateNextRun();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetWeeklySchedule(DayOfWeek dayOfWeek)
    {
        DayOfWeek = dayOfWeek;
        CalculateNextRun();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMonthlySchedule(int dayOfMonth)
    {
        if (dayOfMonth < 1 || dayOfMonth > 31)
            throw new ArgumentException("Day of month must be between 1 and 31");

        DayOfMonth = dayOfMonth;
        CalculateNextRun();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRecipients(string recipients)
    {
        Recipients = recipients;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDeliveryOptions(bool sendEmail, bool saveToStorage)
    {
        SendEmail = sendEmail;
        SaveToStorage = saveToStorage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        CalculateNextRun();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        NextRunAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordExecution(bool success, string? status = null)
    {
        LastRunAt = DateTime.UtcNow;
        LastRunStatus = success ? "Success" : "Failed";
        if (!string.IsNullOrEmpty(status))
            LastRunStatus = status;
        CalculateNextRun();
        UpdatedAt = DateTime.UtcNow;
    }

    private void CalculateNextRun()
    {
        if (!IsActive)
        {
            NextRunAt = null;
            return;
        }

        var now = DateTime.UtcNow;
        var time = ExecutionTime ?? new TimeOnly(0, 0);

        NextRunAt = Frequency switch
        {
            ScheduleFrequency.Daily => now.Date.AddDays(1).Add(time.ToTimeSpan()),
            ScheduleFrequency.Weekly => GetNextWeekday(now, DayOfWeek ?? System.DayOfWeek.Monday).Add(time.ToTimeSpan()),
            ScheduleFrequency.Monthly => GetNextMonthDay(now, DayOfMonth ?? 1).Add(time.ToTimeSpan()),
            _ => now.AddDays(1)
        };
    }

    private static DateTime GetNextWeekday(DateTime from, DayOfWeek day)
    {
        var daysUntil = ((int)day - (int)from.DayOfWeek + 7) % 7;
        return daysUntil == 0 ? from.Date.AddDays(7) : from.Date.AddDays(daysUntil);
    }

    private static DateTime GetNextMonthDay(DateTime from, int day)
    {
        var nextMonth = new DateTime(from.Year, from.Month, 1).AddMonths(1);
        var actualDay = Math.Min(day, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
        return new DateTime(nextMonth.Year, nextMonth.Month, actualDay);
    }
}

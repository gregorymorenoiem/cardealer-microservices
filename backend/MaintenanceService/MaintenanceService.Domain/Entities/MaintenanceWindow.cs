namespace MaintenanceService.Domain.Entities;

/// <summary>
/// Ventana de mantenimiento programada
/// </summary>
public class MaintenanceWindow
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public MaintenanceType Type { get; private set; }
    public MaintenanceStatus Status { get; private set; }
    public DateTime ScheduledStart { get; private set; }
    public DateTime ScheduledEnd { get; private set; }
    public DateTime? ActualStart { get; private set; }
    public DateTime? ActualEnd { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? Notes { get; private set; }
    public bool NotifyUsers { get; private set; }
    public int NotifyMinutesBefore { get; private set; }
    public List<string> AffectedServices { get; private set; } = new();

    private MaintenanceWindow() { } // EF Core

    public MaintenanceWindow(
        string title,
        string description,
        MaintenanceType type,
        DateTime scheduledStart,
        DateTime scheduledEnd,
        string createdBy,
        bool notifyUsers = true,
        int notifyMinutesBefore = 30,
        List<string>? affectedServices = null)
    {
        if (scheduledEnd <= scheduledStart)
            throw new ArgumentException("End time must be after start time");

        Id = Guid.NewGuid();
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Type = type;
        Status = MaintenanceStatus.Scheduled;
        ScheduledStart = scheduledStart;
        ScheduledEnd = scheduledEnd;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedAt = DateTime.UtcNow;
        NotifyUsers = notifyUsers;
        NotifyMinutesBefore = notifyMinutesBefore;
        AffectedServices = affectedServices ?? new List<string>();
    }

    public void Start()
    {
        if (Status != MaintenanceStatus.Scheduled)
            throw new InvalidOperationException($"Cannot start maintenance in status: {Status}");

        Status = MaintenanceStatus.InProgress;
        ActualStart = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != MaintenanceStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete maintenance in status: {Status}");

        Status = MaintenanceStatus.Completed;
        ActualEnd = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == MaintenanceStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed maintenance");

        Status = MaintenanceStatus.Cancelled;
        Notes = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSchedule(DateTime newStart, DateTime newEnd)
    {
        if (Status != MaintenanceStatus.Scheduled)
            throw new InvalidOperationException("Can only reschedule scheduled maintenance");

        if (newEnd <= newStart)
            throw new ArgumentException("End time must be after start time");

        ScheduledStart = newStart;
        ScheduledEnd = newEnd;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive()
    {
        return Status == MaintenanceStatus.InProgress;
    }

    public bool IsUpcoming()
    {
        return Status == MaintenanceStatus.Scheduled && 
               ScheduledStart > DateTime.UtcNow;
    }
}

public enum MaintenanceType
{
    Scheduled = 1,
    Emergency = 2,
    Database = 3,
    Deployment = 4,
    Infrastructure = 5,
    Other = 99
}

public enum MaintenanceStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

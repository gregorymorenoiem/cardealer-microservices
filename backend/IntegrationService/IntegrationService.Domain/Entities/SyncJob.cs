using CarDealer.Shared.MultiTenancy;

namespace IntegrationService.Domain.Entities;

public enum SyncDirection
{
    Inbound,
    Outbound,
    Bidirectional
}

public enum SyncStatus
{
    Idle,
    Running,
    Completed,
    Failed,
    Cancelled
}

public class SyncJob : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public Guid IntegrationId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public SyncDirection Direction { get; private set; }
    public SyncStatus Status { get; private set; }

    public string EntityType { get; private set; } = string.Empty; // Products, Customers, etc.
    public string? FilterCriteria { get; private set; } // JSON

    public DateTime? ScheduledAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public int TotalRecords { get; private set; }
    public int ProcessedRecords { get; private set; }
    public int SuccessCount { get; private set; }
    public int ErrorCount { get; private set; }

    public string? ErrorLog { get; private set; } // JSON array

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // Navigation
    public Integration? Integration { get; private set; }

    private SyncJob() { }

    public SyncJob(
        Guid dealerId,
        Guid integrationId,
        string name,
        string entityType,
        SyncDirection direction,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        IntegrationId = integrationId;
        Name = name;
        EntityType = entityType;
        Direction = direction;
        CreatedBy = createdBy;
        Status = SyncStatus.Idle;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetFilter(string filterCriteria)
    {
        FilterCriteria = filterCriteria;
    }

    public void Schedule(DateTime scheduledAt)
    {
        if (scheduledAt <= DateTime.UtcNow)
            throw new InvalidOperationException("Scheduled date must be in the future");

        ScheduledAt = scheduledAt;
    }

    public void Start(int totalRecords)
    {
        Status = SyncStatus.Running;
        StartedAt = DateTime.UtcNow;
        TotalRecords = totalRecords;
        ProcessedRecords = 0;
        SuccessCount = 0;
        ErrorCount = 0;
    }

    public void UpdateProgress(int processed, int success, int errors)
    {
        ProcessedRecords = processed;
        SuccessCount = success;
        ErrorCount = errors;
    }

    public void Complete()
    {
        Status = SyncStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string errorLog)
    {
        Status = SyncStatus.Failed;
        ErrorLog = errorLog;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = SyncStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }
}

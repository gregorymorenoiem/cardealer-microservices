using CarDealer.Shared.MultiTenancy;

namespace ReportsService.Domain.Entities;

public enum ReportType
{
    Sales,
    Inventory,
    Financial,
    CRM,
    Marketing,
    Custom
}

public enum ReportFormat
{
    Pdf,
    Excel,
    Csv,
    Html,
    Json
}

public enum ReportStatus
{
    Draft,
    Generating,
    Ready,
    Failed,
    Expired
}

public class Report : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public ReportType Type { get; private set; }
    public ReportFormat Format { get; private set; }
    public ReportStatus Status { get; private set; }

    public string? QueryDefinition { get; private set; } // JSON
    public string? Parameters { get; private set; } // JSON
    public string? FilterCriteria { get; private set; } // JSON

    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public string? FilePath { get; private set; }
    public long? FileSize { get; private set; }
    public string? ErrorMessage { get; private set; }

    public DateTime? GeneratedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    private Report() { }

    public Report(
        Guid dealerId,
        string name,
        ReportType type,
        ReportFormat format,
        Guid createdBy,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        Name = name;
        Type = type;
        Format = format;
        CreatedBy = createdBy;
        Description = description;
        Status = ReportStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetDateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be after start date");

        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetQueryDefinition(string queryDefinition)
    {
        QueryDefinition = queryDefinition;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetParameters(string parameters)
    {
        Parameters = parameters;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFilter(string filterCriteria)
    {
        FilterCriteria = filterCriteria;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartGeneration()
    {
        Status = ReportStatus.Generating;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(string filePath, long fileSize, DateTime? expiresAt = null)
    {
        Status = ReportStatus.Ready;
        FilePath = filePath;
        FileSize = fileSize;
        GeneratedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = ReportStatus.Failed;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsExpired()
    {
        Status = ReportStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }
}

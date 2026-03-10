namespace ReportsService.Domain.Entities;

public enum ContentReportType
{
    Vehicle,
    User,
    Message,
    Dealer
}

public enum ContentReportStatus
{
    Pending,
    Investigating,
    Resolved,
    Dismissed
}

public enum ContentReportPriority
{
    Low,
    Medium,
    High
}

/// <summary>
/// Content moderation report — submitted by users to flag
/// vehicles, users, messages, or dealers for review.
/// </summary>
public class ContentReport
{
    public Guid Id { get; private set; }
    public ContentReportType Type { get; private set; }
    public string TargetId { get; private set; } = string.Empty;
    public string TargetTitle { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid ReportedById { get; private set; }
    public string ReportedByEmail { get; private set; } = string.Empty;
    public ContentReportStatus Status { get; private set; } = ContentReportStatus.Pending;
    public ContentReportPriority Priority { get; private set; } = ContentReportPriority.Medium;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; private set; }
    public string? ResolvedById { get; private set; }
    public string? Resolution { get; private set; }
    public int ReportCount { get; private set; } = 1;

    private ContentReport() { }

    public ContentReport(
        ContentReportType type,
        string targetId,
        string targetTitle,
        string reason,
        string description,
        Guid reportedById,
        string reportedByEmail,
        ContentReportPriority priority = ContentReportPriority.Medium)
    {
        if (string.IsNullOrWhiteSpace(targetId))
            throw new ArgumentException("Target ID is required", nameof(targetId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required", nameof(reason));

        Id = Guid.NewGuid();
        Type = type;
        TargetId = targetId;
        TargetTitle = targetTitle;
        Reason = reason;
        Description = description;
        ReportedById = reportedById;
        ReportedByEmail = reportedByEmail;
        Priority = priority;
        Status = ContentReportStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetStatus(ContentReportStatus status, string? resolution = null, string? resolvedById = null)
    {
        Status = status;
        if (status == ContentReportStatus.Resolved || status == ContentReportStatus.Dismissed)
        {
            ResolvedAt = DateTime.UtcNow;
            ResolvedById = resolvedById;
            Resolution = resolution;
        }
    }

    public void IncrementReportCount()
    {
        ReportCount++;
        if (ReportCount >= 5 && Priority != ContentReportPriority.High)
            Priority = ContentReportPriority.High;
    }
}

using AdminService.Domain.Enums;

namespace AdminService.Domain.Entities;

public class Report
{
    public Guid Id { get; set; }
    public ReportType Type { get; set; }
    public string TargetId { get; set; } = string.Empty;
    public string TargetTitle { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ReportedById { get; set; }
    public string ReportedByEmail { get; set; } = string.Empty;
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public ReportPriority Priority { get; set; } = ReportPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedById { get; set; }
    public string? Resolution { get; set; }
    public int ReportCount { get; set; } = 1;
}

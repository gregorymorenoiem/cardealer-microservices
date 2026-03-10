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
/// Razón específica del reporte — categorías requeridas por auditoría.
/// Cada listing report debe tener una razón concreta para facilitar
/// la moderación y el auto-suspend tras 3 reportes.
/// </summary>
public enum ContentReportReason
{
    /// <summary>El precio publicado no corresponde al real</summary>
    PrecioIncorrecto = 0,

    /// <summary>El vehículo ya fue vendido o no está disponible</summary>
    VehiculoNoDisponible = 1,

    /// <summary>Las fotos del listing no corresponden al vehículo real</summary>
    FotosNoCorresponden = 2,

    /// <summary>El anuncio parece ser una estafa o fraude</summary>
    PosibleFraude = 3,

    /// <summary>Otra razón — requiere descripción</summary>
    Otra = 99
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
    public ContentReportReason? ReportCategory { get; private set; }
    public Guid ReportedById { get; private set; }
    public string ReportedByEmail { get; private set; } = string.Empty;
    public ContentReportStatus Status { get; private set; } = ContentReportStatus.Pending;
    public ContentReportPriority Priority { get; private set; } = ContentReportPriority.Medium;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; private set; }
    public string? ResolvedById { get; private set; }
    public string? Resolution { get; private set; }
    public int ReportCount { get; private set; } = 1;
    /// <summary>
    /// IP address of the anonymous reporter for abuse prevention.
    /// </summary>
    public string? ReporterIpAddress { get; private set; }

    private ContentReport() { }

    public ContentReport(
        ContentReportType type,
        string targetId,
        string targetTitle,
        string reason,
        string description,
        Guid reportedById,
        string reportedByEmail,
        ContentReportPriority priority = ContentReportPriority.Medium,
        ContentReportReason? reportCategory = null,
        string? reporterIpAddress = null)
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
        ReportCategory = reportCategory;
        ReportedById = reportedById;
        ReportedByEmail = reportedByEmail;
        Priority = priority;
        ReporterIpAddress = reporterIpAddress;
        Status = ContentReportStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        // Fraude reports start at High priority
        if (reportCategory == ContentReportReason.PosibleFraude)
            Priority = ContentReportPriority.High;
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
        // Auto-escalate to High priority at ≥3 reports (audit requirement)
        if (ReportCount >= 3 && Priority != ContentReportPriority.High)
            Priority = ContentReportPriority.High;
    }

    /// <summary>
    /// Whether this report has reached the auto-suspend threshold (≥3 reports).
    /// </summary>
    public bool HasReachedAutoSuspendThreshold => ReportCount >= 3;
}

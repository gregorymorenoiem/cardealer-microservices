// =====================================================
// ComplianceReportingService - Entities
// Reportes Consolidados de Cumplimiento RD
// =====================================================

namespace ComplianceReportingService.Domain.Entities;

/// <summary>
/// Reporte de cumplimiento regulatorio
/// </summary>
public class ComplianceReport
{
    public Guid Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public Enums.ReportType ReportType { get; set; }
    public Enums.RegulatoryBody RegulatoryBody { get; set; }
    public string Period { get; set; } = string.Empty; // YYYYMM
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Enums.ReportStatus Status { get; set; }
    public string? ReportContent { get; set; } // JSON con datos
    public string? FilePath { get; set; }
    public string? SubmissionReference { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public ICollection<ReportItem> Items { get; set; } = new List<ReportItem>();
    public ICollection<ReportSubmission> Submissions { get; set; } = new List<ReportSubmission>();
}

/// <summary>
/// Ítem dentro de un reporte
/// </summary>
public class ReportItem
{
    public Guid Id { get; set; }
    public Guid ComplianceReportId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string ItemData { get; set; } = string.Empty; // JSON
    public decimal? Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime ItemDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ComplianceReport? ComplianceReport { get; set; }
}

/// <summary>
/// Historial de envío de reportes
/// </summary>
public class ReportSubmission
{
    public Guid Id { get; set; }
    public Guid ComplianceReportId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string SubmissionMethod { get; set; } = string.Empty; // API, Web, Email
    public string? SubmissionReference { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ResponseMessage { get; set; }
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public ComplianceReport? ComplianceReport { get; set; }
}

/// <summary>
/// Programación de generación de reportes
/// </summary>
public class ReportSchedule
{
    public Guid Id { get; set; }
    public Enums.ReportType ReportType { get; set; }
    public Enums.RegulatoryBody RegulatoryBody { get; set; }
    public string CronExpression { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Plantilla de reporte
/// </summary>
public class ReportTemplate
{
    public Guid Id { get; set; }
    public Enums.ReportType ReportType { get; set; }
    public Enums.RegulatoryBody RegulatoryBody { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string TemplateVersion { get; set; } = string.Empty;
    public string TemplateContent { get; set; } = string.Empty; // JSON schema
    public bool IsActive { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime CreatedAt { get; set; }
}

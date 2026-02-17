// ComplianceReportingService - Domain Entities
// Reportería regulatoria para DGII, Pro-Consumidor, Ley 155-17 PLD

namespace ComplianceReportingService.Domain.Entities;

#region Enums

public enum ReportType
{
    // DGII Reports
    DGII_606,           // Compras e importaciones de bienes y servicios
    DGII_607,           // Ventas de bienes y servicios
    DGII_608,           // Comprobantes anulados
    DGII_609,           // Pagos al exterior
    DGII_IT1,           // Impuesto sobre Transferencias
    
    // PLD/AML Reports (Ley 155-17)
    UAF_ROS,            // Reporte de Operación Sospechosa
    UAF_CTR,            // Currency Transaction Report
    UAF_Threshold,      // Umbral de transacciones
    
    // Pro-Consumidor
    ProConsumidor_Complaints,   // Quejas de consumidores
    ProConsumidor_Resolutions,  // Resoluciones de disputas
    
    // Internal
    Transactions_Daily,
    Transactions_Monthly,
    KYC_Verification,
    Compliance_Summary,
    Disputes_Summary,
    Vehicle_Sales,
    User_Activity,
    Custom
}

public enum ReportStatus
{
    Draft,
    Pending,
    Processing,
    Generating,
    Generated,
    Submitting,
    Submitted,
    Accepted,
    Rejected,
    SubmissionFailed,
    RequiresCorrection,
    Expired,
    Cancelled
}

public enum ReportFormat
{
    TXT,
    PDF,
    Excel,
    CSV,
    XML,
    JSON
}

public enum ReportFrequency
{
    OnDemand,
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

public enum DestinationType
{
    DGII,
    UAF,
    ProConsumidor,
    BancoCentral,
    Internal,
    External
}

#endregion

#region Entities

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ReportNumber { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Draft;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Period
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    
    // Generation
    public ReportFormat Format { get; set; } = ReportFormat.PDF;
    public DateTime? GeneratedAt { get; set; }
    public string? FilePath { get; set; }
    public long? FileSize { get; set; }
    public string? FileHash { get; set; }
    public string? GeneratedBy { get; set; }
    
    // Submission
    public DestinationType Destination { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? SubmittedBy { get; set; }
    public string? SubmissionReference { get; set; }
    public string? SubmissionResponse { get; set; }
    
    // Content summary
    public int RecordCount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; } = "DOP";
    public string? MetadataJson { get; set; }
    
    // Parameters and notes (added for handlers)
    public string? ParametersJson { get; set; }
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
    
    // Dates
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
}

public class ReportSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ReportType ReportType { get; set; }
    public ReportFrequency Frequency { get; set; }
    public ReportFormat Format { get; set; } = ReportFormat.PDF;
    public DestinationType Destination { get; set; }
    
    // Schedule
    public string CronExpression { get; set; } = string.Empty;
    public DateTime? NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public Guid? LastReportId { get; set; }
    
    // Options
    public bool AutoSubmit { get; set; }
    public string? NotificationEmail { get; set; }
    public string? ParametersJson { get; set; }
    
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ReportTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ReportType ForReportType { get; set; }
    public string? Description { get; set; }
    public string TemplateContent { get; set; } = string.Empty;
    public string? QueryDefinition { get; set; }
    public string? ParametersSchema { get; set; }
    public string Version { get; set; } = "1.0";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ReportExecution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReportId { get; set; }
    public Guid? ScheduleId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int DurationMs { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public string ExecutedBy { get; set; } = string.Empty;
}

public class ReportSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public ReportType ReportType { get; set; }
    public ReportFrequency Frequency { get; set; }
    public string DeliveryMethod { get; set; } = "Email";
    public string? DeliveryAddress { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DGIISubmission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReportId { get; set; }
    public string ReportCode { get; set; } = string.Empty;
    public string RNC { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    public string? ConfirmationNumber { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public string? RawRequest { get; set; }
    public string? RawResponse { get; set; }
    public int Attempts { get; set; } = 1;
}

public class UAFSubmission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReportId { get; set; }
    public string ReportCode { get; set; } = string.Empty;
    public string EntityRNC { get; set; } = string.Empty;
    public string ReportingPeriod { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    public string? UAFCaseNumber { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public bool IsUrgent { get; set; }
}

public class ComplianceMetric
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MetricCode { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? Threshold { get; set; }
    public string? Unit { get; set; }
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public bool IsAlert { get; set; }
    public string? AlertMessage { get; set; }
    public string? RecordedBy { get; set; }
}

#endregion

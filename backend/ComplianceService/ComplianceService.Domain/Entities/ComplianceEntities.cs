// ComplianceService - Domain Entities
// Compliance: Ley 155-17 (Prevención de Lavado de Activos)
// Provides consolidated compliance management and regulatory reporting

namespace ComplianceService.Domain.Entities;

#region Enums

/// <summary>
/// Estado de la evaluación de compliance
/// </summary>
public enum ComplianceStatus
{
    NotEvaluated = 0,
    Pending = 1,
    InProgress = 2,
    Compliant = 3,
    NonCompliant = 4,
    PartiallyCompliant = 5,
    UnderRemediation = 6,
    Exempted = 7
}

/// <summary>
/// Tipo de regulación aplicable
/// </summary>
public enum RegulationType
{
    PLD_AML = 1,              // Ley 155-17 - Prevención de Lavado
    DataProtection = 2,       // Ley 172-13 - Protección de Datos
    ConsumerProtection = 3,   // Ley 358-05 - Protección al Consumidor
    ElectronicCommerce = 4,   // Ley 126-02 - Comercio Electrónico
    FinancialRegulation = 5,  // Regulaciones SIB/SIPEN
    TaxCompliance = 6,        // DGII - Cumplimiento Fiscal
    VehicleRegistration = 7,  // DGTT - Registro Vehicular
    Environmental = 8,        // MARENA - Medio Ambiente
    Other = 99
}

/// <summary>
/// Nivel de criticidad del requerimiento
/// </summary>
public enum CriticalityLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Estado de la tarea de cumplimiento
/// </summary>
public enum TaskStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Overdue = 4,
    Cancelled = 5,
    OnHold = 6
}

/// <summary>
/// Tipo de hallazgo de auditoría
/// </summary>
public enum FindingType
{
    Observation = 1,
    MinorNonConformity = 2,
    MajorNonConformity = 3,
    CriticalNonConformity = 4,
    Recommendation = 5,
    BestPractice = 6
}

/// <summary>
/// Estado del hallazgo
/// </summary>
public enum FindingStatus
{
    Open = 1,
    InProgress = 2,
    Resolved = 3,
    Verified = 4,
    Closed = 5,
    Overdue = 6,
    Escalated = 7
}

/// <summary>
/// Tipo de reporte regulatorio
/// </summary>
public enum RegulatoryReportType
{
    AnnualCompliance = 1,     // Reporte anual de cumplimiento
    QuarterlyPLD = 2,         // Reporte trimestral PLD
    IncidentReport = 3,       // Reporte de incidentes
    AuditReport = 4,          // Reporte de auditoría
    RiskAssessment = 5,       // Evaluación de riesgos
    TrainingReport = 6,       // Reporte de capacitaciones
    TransactionReport = 7,    // Reporte de transacciones
    UAFReport = 8,            // Reporte a UAF
    SIBReport = 9,            // Reporte a SIB
    DGIIReport = 10           // Reporte a DGII
}

/// <summary>
/// Estado del reporte regulatorio
/// </summary>
public enum ReportStatus
{
    Draft = 1,
    PendingReview = 2,
    Approved = 3,
    Submitted = 4,
    Acknowledged = 5,
    Rejected = 6,
    RequiresCorrection = 7,
    Accepted = 8              // Aceptado por el regulador
}

/// <summary>
/// Tipo de control de compliance
/// </summary>
public enum ControlType
{
    Preventive = 1,     // Control preventivo
    Detective = 2,      // Control detectivo
    Corrective = 3,     // Control correctivo
    Directive = 4       // Control directivo
}

/// <summary>
/// Frecuencia de evaluación
/// </summary>
public enum EvaluationFrequency
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Quarterly = 4,
    SemiAnnual = 5,
    Annual = 6,
    OnDemand = 7,
    Continuous = 8
}

#endregion

#region Entities

/// <summary>
/// Marco regulatorio - Definición de regulaciones aplicables
/// </summary>
public class RegulatoryFramework
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RegulationType Type { get; set; }
    public string? LegalReference { get; set; }           // Ley o regulación de referencia
    public string? RegulatoryBody { get; set; }           // Entidad reguladora (UAF, SIB, DGII, etc.)
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Version { get; set; }
    public string? Notes { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navegación
    public ICollection<ComplianceRequirement> Requirements { get; set; } = new List<ComplianceRequirement>();
    public ICollection<ComplianceControl> Controls { get; set; } = new List<ComplianceControl>();
}

/// <summary>
/// Requerimiento de cumplimiento específico
/// </summary>
public class ComplianceRequirement
{
    public Guid Id { get; set; }
    public Guid FrameworkId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CriticalityLevel Criticality { get; set; }
    public string? ArticleReference { get; set; }          // Artículo de la ley
    public int DeadlineDays { get; set; }                  // Días para cumplir
    public EvaluationFrequency EvaluationFrequency { get; set; }
    public bool RequiresEvidence { get; set; } = true;
    public bool RequiresApproval { get; set; } = true;
    public string? EvidenceRequirements { get; set; }      // JSON con requisitos de evidencia
    public bool IsActive { get; set; } = true;
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    
    // Navegación
    public RegulatoryFramework? Framework { get; set; }
    public ICollection<ComplianceAssessment> Assessments { get; set; } = new List<ComplianceAssessment>();
    public ICollection<ComplianceControl> Controls { get; set; } = new List<ComplianceControl>();
}

/// <summary>
/// Control de cumplimiento implementado
/// </summary>
public class ComplianceControl
{
    public Guid Id { get; set; }
    public Guid FrameworkId { get; set; }
    public Guid? RequirementId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ControlType Type { get; set; }
    public string? ImplementationDetails { get; set; }
    public string? ResponsibleRole { get; set; }
    public EvaluationFrequency TestingFrequency { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public string? LastTestedBy { get; set; }              // Último evaluador
    public DateTime? NextTestDate { get; set; }
    public ComplianceStatus Status { get; set; } = ComplianceStatus.NotEvaluated;
    public int EffectivenessScore { get; set; }            // 0-100
    public bool IsActive { get; set; } = true;
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navegación
    public RegulatoryFramework? Framework { get; set; }
    public ComplianceRequirement? Requirement { get; set; }
    public ICollection<ControlTest> Tests { get; set; } = new List<ControlTest>();
}

/// <summary>
/// Prueba de control
/// </summary>
public class ControlTest
{
    public Guid Id { get; set; }
    public Guid ControlId { get; set; }
    public DateTime TestDate { get; set; }
    public DateTime TestedAt { get; set; }                 // Alias para compatibilidad
    public string TestedBy { get; set; } = string.Empty;
    public string TestProcedure { get; set; } = string.Empty;
    public string? TestResults { get; set; }
    public bool IsPassed { get; set; }
    public int? EffectivenessScore { get; set; }           // 0-100
    public string? Findings { get; set; }
    public string? Recommendations { get; set; }
    public List<string> EvidenceDocuments { get; set; } = new();
    
    // Navegación
    public ComplianceControl? Control { get; set; }
}

/// <summary>
/// Evaluación de cumplimiento de una entidad
/// </summary>
public class ComplianceAssessment
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;  // User, Dealer, Transaction, etc.
    public Guid EntityId { get; set; }
    public Guid RequirementId { get; set; }
    public ComplianceStatus Status { get; set; }
    public DateTime AssessmentDate { get; set; }
    public string AssessedBy { get; set; } = string.Empty;
    public int? Score { get; set; }                         // 0-100 si aplica
    public string? Observations { get; set; }
    public string? EvidenceProvided { get; set; }
    public DateTime? NextAssessmentDate { get; set; }
    public DateTime? DeadlineDate { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navegación
    public ComplianceRequirement? Requirement { get; set; }
    public ICollection<ComplianceFinding> Findings { get; set; } = new List<ComplianceFinding>();
}

/// <summary>
/// Hallazgo de cumplimiento
/// </summary>
public class ComplianceFinding
{
    public Guid Id { get; set; }
    public Guid AssessmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FindingType Type { get; set; }
    public FindingStatus Status { get; set; } = FindingStatus.Open;
    public CriticalityLevel Criticality { get; set; }
    public string? RootCause { get; set; }
    public string? Impact { get; set; }
    public string? Recommendation { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? Resolution { get; set; }
    public List<string> EvidenceDocuments { get; set; } = new();
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navegación
    public ComplianceAssessment? Assessment { get; set; }
    public ICollection<RemediationAction> RemediationActions { get; set; } = new List<RemediationAction>();
}

/// <summary>
/// Acción de remediación
/// </summary>
public class RemediationAction
{
    public Guid Id { get; set; }
    public Guid FindingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public string? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public int? Priority { get; set; }                      // 1 = highest
    public string? CompletionNotes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public bool RequiresVerification { get; set; } = true;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navegación
    public ComplianceFinding? Finding { get; set; }
}

/// <summary>
/// Reporte regulatorio
/// </summary>
public class RegulatoryReport
{
    public Guid Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public RegulatoryReportType Type { get; set; }
    public RegulationType RegulationType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Draft;
    public string? RegulatoryBody { get; set; }            // UAF, SIB, DGII, etc.
    public DateTime? SubmissionDeadline { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? SubmittedBy { get; set; }
    public string? SubmissionReference { get; set; }       // Número de recibo/confirmación
    public string? Content { get; set; }                   // JSON con contenido del reporte
    public List<string> Attachments { get; set; } = new();
    public string? ReviewComments { get; set; }
    public string? RegulatoryResponse { get; set; }
    public string? RejectionReason { get; set; }           // Razón de rechazo si aplica
    
    // Aprobaciones
    public string? PreparedBy { get; set; }
    public DateTime? PreparedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Calendario de cumplimiento - Fechas importantes
/// </summary>
public class ComplianceCalendar
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RegulationType RegulationType { get; set; }
    public DateTime DueDate { get; set; }
    public int ReminderDaysBefore { get; set; } = 7;
    public bool IsRecurring { get; set; }
    public EvaluationFrequency? RecurrencePattern { get; set; }
    public string? AssignedTo { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public string? CompletionNotes { get; set; }
    public string? Notes { get; set; }
    public bool NotificationSent { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Capacitación de compliance
/// </summary>
public class ComplianceTraining
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RegulationType RegulationType { get; set; }
    public string? TargetRoles { get; set; }               // JSON con roles objetivo
    public bool IsMandatory { get; set; } = true;
    public int DurationMinutes { get; set; }
    public string? ContentUrl { get; set; }
    public string? ExamUrl { get; set; }
    public int? PassingScore { get; set; }                 // Porcentaje mínimo
    public DateTime? ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navegación
    public ICollection<TrainingCompletion> Completions { get; set; } = new List<TrainingCompletion>();
}

/// <summary>
/// Registro de capacitación completada
/// </summary>
public class TrainingCompletion
{
    public Guid Id { get; set; }
    public Guid TrainingId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? Score { get; set; }
    public bool IsPassed { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? CertificateUrl { get; set; }
    
    // Navegación
    public ComplianceTraining? Training { get; set; }
}

/// <summary>
/// Métricas de cumplimiento
/// </summary>
public class ComplianceMetric
{
    public Guid Id { get; set; }
    public RegulationType RegulationType { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal Value { get; set; }
    public string? Unit { get; set; }                      // %, count, days, etc.
    public decimal? Target { get; set; }
    public decimal? Threshold { get; set; }                // Umbral de alerta
    public bool IsWithinTarget { get; set; }
    public string? CalculationMethod { get; set; }
    
    // Auditoría
    public DateTime CalculatedAt { get; set; }
    public string CalculatedBy { get; set; } = string.Empty;
}

#endregion

// ComplianceService - Commands

namespace ComplianceService.Application.Commands;

using MediatR;
using ComplianceService.Domain.Entities;
using ComplianceService.Application.DTOs;

#region Framework Commands

public record CreateFrameworkCommand(
    string Code,
    string Name,
    string? Description,
    RegulationType Type,
    string? LegalReference,
    string? RegulatoryBody,
    DateTime EffectiveDate,
    DateTime? ExpirationDate,
    string? Version,
    string? Notes,
    string CreatedBy
) : IRequest<Guid>;

public record UpdateFrameworkCommand(
    Guid Id,
    string Name,
    string? Description,
    string? LegalReference,
    string? RegulatoryBody,
    DateTime? ExpirationDate,
    bool IsActive,
    string? Version,
    string? Notes,
    string UpdatedBy
) : IRequest<bool>;

#endregion

#region Requirement Commands

public record CreateRequirementCommand(
    Guid FrameworkId,
    string Code,
    string Title,
    string Description,
    CriticalityLevel Criticality,
    string? ArticleReference,
    int DeadlineDays,
    EvaluationFrequency EvaluationFrequency,
    bool RequiresEvidence,
    bool RequiresApproval,
    string? EvidenceRequirements,
    string CreatedBy
) : IRequest<Guid>;

public record UpdateRequirementCommand(
    Guid Id,
    string Title,
    string Description,
    CriticalityLevel Criticality,
    int DeadlineDays,
    bool IsActive,
    string UpdatedBy
) : IRequest<bool>;

#endregion

#region Control Commands

public record CreateControlCommand(
    Guid FrameworkId,
    Guid? RequirementId,
    string Code,
    string Name,
    string Description,
    ControlType Type,
    string? ImplementationDetails,
    string? ResponsibleRole,
    EvaluationFrequency TestingFrequency,
    string CreatedBy
) : IRequest<Guid>;

public record UpdateControlCommand(
    Guid Id,
    string Name,
    string Description,
    string? ImplementationDetails,
    string? ResponsibleRole,
    EvaluationFrequency TestingFrequency,
    bool IsActive,
    string UpdatedBy
) : IRequest<bool>;

public record CreateControlTestCommand(
    Guid ControlId,
    string TestProcedure,
    string? TestResults,
    bool IsPassed,
    int? EffectivenessScore,
    string? Findings,
    string? Recommendations,
    List<string>? EvidenceDocuments,
    string TestedBy
) : IRequest<Guid>;

#endregion

#region Assessment Commands

public record CreateAssessmentCommand(
    string EntityType,
    Guid EntityId,
    Guid RequirementId,
    ComplianceStatus Status,
    int? Score,
    string? Observations,
    string? EvidenceProvided,
    DateTime? NextAssessmentDate,
    string AssessedBy
) : IRequest<Guid>;

public record UpdateAssessmentCommand(
    Guid Id,
    ComplianceStatus Status,
    int? Score,
    string? Observations,
    string? EvidenceProvided,
    DateTime? NextAssessmentDate,
    string UpdatedBy
) : IRequest<bool>;

#endregion

#region Finding Commands

public record CreateFindingCommand(
    Guid AssessmentId,
    string Title,
    string Description,
    FindingType Type,
    CriticalityLevel Criticality,
    string? RootCause,
    string? Impact,
    string? Recommendation,
    string? AssignedTo,
    DateTime? DueDate,
    string CreatedBy
) : IRequest<Guid>;

public record UpdateFindingCommand(
    Guid Id,
    FindingStatus Status,
    string? Resolution,
    string? AssignedTo,
    DateTime? DueDate,
    string UpdatedBy
) : IRequest<bool>;

public record ResolveFindingCommand(
    Guid Id,
    string Resolution,
    string ResolvedBy
) : IRequest<bool>;

#endregion

#region Remediation Commands

public record CreateRemediationCommand(
    Guid FindingId,
    string Title,
    string Description,
    string? AssignedTo,
    DateTime? DueDate,
    int? Priority,
    bool RequiresVerification,
    string CreatedBy
) : IRequest<Guid>;

public record UpdateRemediationCommand(
    Guid Id,
    TaskStatus Status,
    string? CompletionNotes,
    string UpdatedBy
) : IRequest<bool>;

public record CompleteRemediationCommand(
    Guid Id,
    string CompletionNotes,
    string CompletedBy
) : IRequest<bool>;

public record VerifyRemediationCommand(
    Guid Id,
    string VerifiedBy
) : IRequest<bool>;

#endregion

#region Report Commands

public record CreateReportCommand(
    RegulatoryReportType ReportType,
    RegulationType RegulationType,
    string Title,
    string? Description,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string? RegulatoryBody,
    DateTime? SubmissionDeadline,
    string? Content,
    string CreatedBy
) : IRequest<CreateReportResponse>;

public record UpdateReportCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? Content,
    List<string>? Attachments,
    string? ReviewComments,
    string UpdatedBy
) : IRequest<bool>;

public record ApproveReportCommand(
    Guid Id,
    string ApprovedBy
) : IRequest<bool>;

public record SubmitReportCommand(
    Guid Id,
    string? SubmissionReference,
    string SubmittedBy
) : IRequest<bool>;

#endregion

#region Calendar Commands

public record CreateCalendarItemCommand(
    string Title,
    string? Description,
    RegulationType RegulationType,
    DateTime DueDate,
    int ReminderDaysBefore,
    bool IsRecurring,
    EvaluationFrequency? RecurrencePattern,
    string? AssignedTo,
    string CreatedBy
) : IRequest<Guid>;

// Alias for controller compatibility
public record CreateCalendarEventCommand(
    string Title,
    string? Description,
    RegulationType RegulationType,
    DateTime DueDate,
    int ReminderDaysBefore,
    bool IsRecurring,
    EvaluationFrequency? RecurrencePattern,
    string? AssignedTo,
    string CreatedBy
) : IRequest<Guid>;

public record UpdateCalendarEventCommand(
    Guid Id,
    string Title,
    string? Description,
    DateTime DueDate,
    TaskStatus Status,
    string? AssignedTo,
    string UpdatedBy
) : IRequest<bool>;

public record CompleteCalendarEventCommand(
    Guid Id,
    string CompletedBy,
    string? Notes
) : IRequest<bool>;

public record CompleteCalendarItemCommand(
    Guid Id,
    string CompletedBy
) : IRequest<bool>;

public record DeleteCalendarEventCommand(
    Guid Id
) : IRequest<bool>;

public record DeleteCalendarItemCommand(
    Guid Id
) : IRequest<bool>;

#endregion

#region Training Commands

public record CreateTrainingCommand(
    string Title,
    string? Description,
    RegulationType RegulationType,
    string? TargetRoles,
    bool IsMandatory,
    int DurationMinutes,
    string? ContentUrl,
    string? ExamUrl,
    int? PassingScore,
    DateTime? ValidUntil,
    string CreatedBy
) : IRequest<Guid>;

public record UpdateTrainingCommand(
    Guid Id,
    string Title,
    string? Description,
    bool IsMandatory,
    int DurationMinutes,
    string? ContentUrl,
    string? ExamUrl,
    int? PassingScore,
    DateTime? ValidUntil,
    bool IsActive,
    string UpdatedBy
) : IRequest<bool>;

public record RecordTrainingCompletionCommand(
    Guid TrainingId,
    Guid UserId,
    decimal? Score,
    bool IsPassed,
    string? CertificateUrl
) : IRequest<Guid>;

#endregion

#region Metric Commands

public record CreateMetricCommand(
    RegulationType RegulationType,
    string MetricName,
    string? Description,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal Value,
    string? Unit,
    decimal? Target,
    decimal? Threshold,
    string? CalculationMethod,
    string CalculatedBy
) : IRequest<Guid>;

// Alias for controller compatibility
public record RecordMetricCommand(
    RegulationType RegulationType,
    string MetricName,
    string? Description,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal Value,
    string? Unit,
    decimal? Target,
    decimal? Threshold,
    string? CalculationMethod,
    string CalculatedBy
) : IRequest<Guid>;

public record CalculateMetricsCommand(
    RegulationType RegulationType,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string CalculatedBy
) : IRequest<List<ComplianceMetricDto>>;

#endregion

#region Control Test Commands

public record RecordControlTestCommand(
    Guid ControlId,
    string TestProcedure,
    string? TestResults,
    bool IsPassed,
    int? EffectivenessScore,
    string? Findings,
    string? Recommendations,
    List<string>? EvidenceDocuments,
    string TestedBy
) : IRequest<Guid>;

#endregion

#region Report Response DTOs

public record CreateReportResponse(Guid Id, string ReportNumber);

#endregion

// ComplianceService - DTOs

namespace ComplianceService.Application.DTOs;

using ComplianceService.Domain.Entities;

#region Framework DTOs

public record RegulatoryFrameworkDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    RegulationType Type,
    string? LegalReference,
    string? RegulatoryBody,
    DateTime EffectiveDate,
    DateTime? ExpirationDate,
    bool IsActive,
    string? Version,
    DateTime CreatedAt,
    int RequirementsCount,
    int ControlsCount
);

public record RegulatoryFrameworkDetailDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    RegulationType Type,
    string? LegalReference,
    string? RegulatoryBody,
    DateTime EffectiveDate,
    DateTime? ExpirationDate,
    bool IsActive,
    string? Version,
    string? Notes,
    DateTime CreatedAt,
    List<ComplianceRequirementDto> Requirements,
    List<ComplianceControlDto> Controls
);

public record CreateFrameworkDto(
    string Code,
    string Name,
    string? Description,
    RegulationType Type,
    string? LegalReference,
    string? RegulatoryBody,
    DateTime EffectiveDate,
    DateTime? ExpirationDate,
    string? Version,
    string? Notes
);

public record UpdateFrameworkDto(
    string Name,
    string? Description,
    string? LegalReference,
    string? RegulatoryBody,
    DateTime? ExpirationDate,
    bool IsActive,
    string? Version,
    string? Notes
);

#endregion

#region Requirement DTOs

public record ComplianceRequirementDto(
    Guid Id,
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
    bool IsActive
);

public record CreateRequirementDto(
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
    string? EvidenceRequirements
);

#endregion

#region Control DTOs

public record ComplianceControlDto(
    Guid Id,
    Guid FrameworkId,
    Guid? RequirementId,
    string Code,
    string Name,
    string Description,
    ControlType Type,
    string? ResponsibleRole,
    EvaluationFrequency TestingFrequency,
    DateTime? LastTestedAt,
    DateTime? NextTestDate,
    ComplianceStatus Status,
    int EffectivenessScore,
    bool IsActive
);

public record CreateControlDto(
    Guid FrameworkId,
    Guid? RequirementId,
    string Code,
    string Name,
    string Description,
    ControlType Type,
    string? ImplementationDetails,
    string? ResponsibleRole,
    EvaluationFrequency TestingFrequency
);

public record ControlTestDto(
    Guid Id,
    Guid ControlId,
    DateTime TestDate,
    string TestedBy,
    string TestProcedure,
    string? TestResults,
    bool IsPassed,
    int? EffectivenessScore,
    string? Findings,
    string? Recommendations,
    List<string> EvidenceDocuments
);

public record CreateControlTestDto(
    Guid ControlId,
    string TestProcedure,
    string? TestResults,
    bool IsPassed,
    int? EffectivenessScore,
    string? Findings,
    string? Recommendations,
    List<string>? EvidenceDocuments
);

public record ControlStatisticsDto(
    int TotalControls,
    int CompliantControls,
    int NonCompliantControls,
    int PendingTestControls,
    double OverallEffectiveness
);

#endregion

#region Assessment DTOs

public record ComplianceAssessmentDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    Guid RequirementId,
    string? RequirementTitle,
    ComplianceStatus Status,
    DateTime AssessmentDate,
    string AssessedBy,
    int? Score,
    string? Observations,
    DateTime? NextAssessmentDate,
    DateTime? DeadlineDate,
    int FindingsCount
);

public record CreateAssessmentDto(
    string EntityType,
    Guid EntityId,
    Guid RequirementId,
    ComplianceStatus Status,
    int? Score,
    string? Observations,
    string? EvidenceProvided,
    DateTime? NextAssessmentDate
);

public record AssessmentStatisticsDto(
    int TotalAssessments,
    int CompliantCount,
    int NonCompliantCount,
    int PendingCount,
    int OverdueCount,
    double ComplianceRate
);

#endregion

#region Finding DTOs

public record ComplianceFindingDto(
    Guid Id,
    Guid AssessmentId,
    string Title,
    string Description,
    FindingType Type,
    FindingStatus Status,
    CriticalityLevel Criticality,
    string? RootCause,
    string? Impact,
    string? Recommendation,
    string? AssignedTo,
    DateTime? DueDate,
    DateTime? ResolvedAt,
    string? Resolution,
    DateTime CreatedAt,
    int RemediationActionsCount
);

public record CreateFindingDto(
    Guid AssessmentId,
    string Title,
    string Description,
    FindingType Type,
    CriticalityLevel Criticality,
    string? RootCause,
    string? Impact,
    string? Recommendation,
    string? AssignedTo,
    DateTime? DueDate
);

public record UpdateFindingDto(
    FindingStatus Status,
    string? Resolution,
    string? AssignedTo,
    DateTime? DueDate
);

public record FindingStatisticsDto(
    int TotalFindings,
    int OpenCount,
    int InProgressCount,
    int ResolvedCount,
    int OverdueCount,
    int CriticalCount,
    double ResolutionRate,
    double AverageResolutionDays
);

#endregion

#region Remediation DTOs

public record RemediationActionDto(
    Guid Id,
    Guid FindingId,
    string Title,
    string Description,
    TaskStatus Status,
    string? AssignedTo,
    DateTime? DueDate,
    int? Priority,
    DateTime? CompletedAt,
    bool RequiresVerification,
    DateTime? VerifiedAt,
    DateTime CreatedAt
);

public record CreateRemediationDto(
    Guid FindingId,
    string Title,
    string Description,
    string? AssignedTo,
    DateTime? DueDate,
    int? Priority,
    bool RequiresVerification
);

public record UpdateRemediationDto(
    TaskStatus Status,
    string? CompletionNotes
);

#endregion

#region Report DTOs

public record RegulatoryReportDto(
    Guid Id,
    string ReportNumber,
    RegulatoryReportType Type,
    RegulationType RegulationType,
    string Title,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    ReportStatus Status,
    string? RegulatoryBody,
    DateTime? SubmissionDeadline,
    DateTime? SubmittedAt,
    string? SubmissionReference,
    DateTime CreatedAt
);

public record RegulatoryReportDetailDto(
    Guid Id,
    string ReportNumber,
    RegulatoryReportType Type,
    RegulationType RegulationType,
    string Title,
    string? Description,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    ReportStatus Status,
    string? RegulatoryBody,
    DateTime? SubmissionDeadline,
    DateTime? SubmittedAt,
    string? SubmittedBy,
    string? SubmissionReference,
    string? Content,
    List<string> Attachments,
    string? ReviewComments,
    string? RegulatoryResponse,
    string? PreparedBy,
    DateTime? PreparedAt,
    string? ReviewedBy,
    DateTime? ReviewedAt,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    DateTime CreatedAt
);

public record CreateReportDto(
    RegulatoryReportType Type,
    RegulationType RegulationType,
    string Title,
    string? Description,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string? RegulatoryBody,
    DateTime? SubmissionDeadline,
    string? Content
);

public record UpdateReportDto(
    string? Title,
    string? Description,
    string? Content,
    List<string>? Attachments,
    string? ReviewComments
);

public record SubmitReportDto(
    string? SubmissionReference
);

#endregion

#region Calendar DTOs

public record ComplianceCalendarDto(
    Guid Id,
    string Title,
    string? Description,
    RegulationType RegulationType,
    DateTime DueDate,
    int ReminderDaysBefore,
    bool IsRecurring,
    EvaluationFrequency? RecurrencePattern,
    string? AssignedTo,
    TaskStatus Status,
    DateTime? CompletedAt,
    bool NotificationSent
);

public record CreateCalendarItemDto(
    string Title,
    string? Description,
    RegulationType RegulationType,
    DateTime DueDate,
    int ReminderDaysBefore,
    bool IsRecurring,
    EvaluationFrequency? RecurrencePattern,
    string? AssignedTo
);

#endregion

#region Training DTOs

public record ComplianceTrainingDto(
    Guid Id,
    string Title,
    string? Description,
    RegulationType RegulationType,
    bool IsMandatory,
    int DurationMinutes,
    string? ContentUrl,
    int? PassingScore,
    DateTime? ValidUntil,
    bool IsActive,
    int CompletionsCount,
    int PassedCount
);

public record CreateTrainingDto(
    string Title,
    string? Description,
    RegulationType RegulationType,
    string? TargetRoles,
    bool IsMandatory,
    int DurationMinutes,
    string? ContentUrl,
    string? ExamUrl,
    int? PassingScore,
    DateTime? ValidUntil
);

public record TrainingCompletionDto(
    Guid Id,
    Guid TrainingId,
    string? TrainingTitle,
    Guid UserId,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int? Score,
    bool IsPassed,
    DateTime? ExpiresAt,
    string? CertificateUrl
);

public record RecordTrainingCompletionDto(
    Guid TrainingId,
    Guid UserId,
    int? Score,
    bool IsPassed
);

public record TrainingStatisticsDto(
    int TotalTrainings,
    int TotalCompletions,
    int PassedCount,
    int FailedCount,
    int ExpiringSoonCount,
    double PassRate
);

#endregion

#region Metric DTOs

public record ComplianceMetricDto(
    Guid Id,
    RegulationType RegulationType,
    string MetricName,
    string? Description,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal Value,
    string? Unit,
    decimal? Target,
    decimal? Threshold,
    bool IsWithinTarget,
    DateTime CalculatedAt
);

public record CreateMetricDto(
    RegulationType RegulationType,
    string MetricName,
    string? Description,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal Value,
    string? Unit,
    decimal? Target,
    decimal? Threshold,
    string? CalculationMethod
);

#endregion

#region Dashboard DTOs

public record ComplianceDashboardDto(
    AssessmentStatisticsDto Assessments,
    FindingStatisticsDto Findings,
    ControlStatisticsDto Controls,
    TrainingStatisticsDto Training,
    List<ComplianceCalendarDto> UpcomingDeadlines,
    List<ComplianceFindingDto> CriticalFindings,
    List<RegulatoryReportDto> PendingReports,
    List<ComplianceMetricDto> KeyMetrics
);

#endregion

#region Common

public record PaginatedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

#endregion

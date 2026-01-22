// ReportingService - DTOs
// Reportería regulatoria DGII, UAF, Pro-Consumidor

namespace ReportingService.Application.DTOs;

using ReportingService.Domain.Entities;

#region Report DTOs

public record ReportDto(
    Guid Id,
    string ReportNumber,
    ReportType Type,
    ReportStatus Status,
    string Name,
    string? Description,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    ReportFormat Format,
    DateTime? GeneratedAt,
    string? FilePath,
    long? FileSize,
    DestinationType Destination,
    DateTime? SubmittedAt,
    string? SubmissionReference,
    int RecordCount,
    decimal? TotalAmount,
    string? Currency,
    DateTime CreatedAt,
    DateTime? DueDate);

public record ReportSummaryDto(
    Guid Id,
    string ReportNumber,
    ReportType Type,
    ReportStatus Status,
    string Name,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    DateTime? GeneratedAt,
    DateTime? SubmittedAt,
    DateTime? DueDate);

public record ReportPagedResultDto(
    List<ReportSummaryDto> Items,
    int Total,
    int Page,
    int PageSize);

public record GenerateReportDto(
    ReportType Type,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    ReportFormat Format,
    DestinationType Destination,
    string? Description,
    string? ParametersJson);

#endregion

#region Schedule DTOs

public record ReportScheduleDto(
    Guid Id,
    string Name,
    ReportType ReportType,
    ReportFrequency Frequency,
    ReportFormat Format,
    DestinationType Destination,
    string CronExpression,
    DateTime? NextRunAt,
    DateTime? LastRunAt,
    bool AutoSubmit,
    string? NotificationEmail,
    bool IsActive);

public record CreateScheduleDto(
    string Name,
    ReportType ReportType,
    ReportFrequency Frequency,
    ReportFormat Format,
    DestinationType Destination,
    string CronExpression,
    bool AutoSubmit,
    string? NotificationEmail,
    string? ParametersJson);

#endregion

#region Template DTOs

public record ReportTemplateDto(
    Guid Id,
    string Code,
    string Name,
    ReportType ForReportType,
    string? Description,
    string Version,
    bool IsActive);

public record CreateTemplateDto(
    string Code,
    string Name,
    ReportType ForReportType,
    string? Description,
    string TemplateContent,
    string? QueryDefinition,
    string? ParametersSchema);

#endregion

#region Execution DTOs

public record ReportExecutionDto(
    Guid Id,
    Guid ReportId,
    Guid? ScheduleId,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int DurationMs,
    bool Success,
    string? ErrorMessage,
    string ExecutedBy);

#endregion

#region Subscription DTOs

public record ReportSubscriptionDto(
    Guid Id,
    Guid UserId,
    ReportType ReportType,
    ReportFrequency Frequency,
    string DeliveryMethod,
    string? DeliveryAddress,
    bool IsActive);

public record CreateSubscriptionDto(
    ReportType ReportType,
    ReportFrequency Frequency,
    string DeliveryMethod,
    string DeliveryAddress);

#endregion

#region DGII DTOs

public record DGIISubmissionDto(
    Guid Id,
    Guid ReportId,
    string ReportCode,
    string RNC,
    string Period,
    DateTime SubmissionDate,
    string Status,
    string? ConfirmationNumber,
    string? ResponseMessage,
    int Attempts);

public record DGII606RecordDto(
    string RNCProvider,
    string ProviderName,
    string NCFType,
    string NCFNumber,
    DateTime NCFDate,
    decimal NetAmount,
    decimal ITBIS,
    decimal Total);

public record DGII607RecordDto(
    string RNCBuyer,
    string BuyerName,
    string NCFType,
    string NCFNumber,
    DateTime NCFDate,
    decimal NetAmount,
    decimal ITBIS,
    decimal Total);

#endregion

#region UAF DTOs

public record UAFSubmissionDto(
    Guid Id,
    Guid ReportId,
    string ReportCode,
    string EntityRNC,
    string ReportingPeriod,
    DateTime SubmissionDate,
    string Status,
    string? UAFCaseNumber,
    bool IsUrgent);

public record UAFROSDto(
    string SubjectName,
    string SubjectIdType,
    string SubjectIdNumber,
    string TransactionType,
    decimal Amount,
    string Currency,
    DateTime TransactionDate,
    string SuspicionIndicators,
    string Narrative);

#endregion

#region Compliance DTOs

public record ComplianceMetricDto(
    Guid Id,
    string MetricCode,
    string MetricName,
    string Category,
    decimal Value,
    decimal? Threshold,
    string? Unit,
    DateTime MeasuredAt,
    bool IsAlert,
    string? AlertMessage);

public record ComplianceDashboardDto(
    int TotalReportsGenerated,
    int PendingSubmissions,
    int OverdueReports,
    int ComplianceScore,
    List<ComplianceMetricDto> RecentMetrics,
    Dictionary<ReportType, int> ReportsByType,
    Dictionary<string, int> AlertsByCategory);

public record ReportingStatisticsDto(
    int TotalReports,
    int GeneratedReports,
    int SubmittedReports,
    int AcceptedReports,
    int RejectedReports,
    decimal AverageGenerationTimeMs,
    Dictionary<ReportType, int> ByType,
    Dictionary<DestinationType, int> ByDestination);

#endregion

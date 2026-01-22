// =====================================================
// ComplianceReportingService - DTOs
// Reportes Consolidados de Cumplimiento RD
// =====================================================

using ComplianceReportingService.Domain.Enums;

namespace ComplianceReportingService.Application.DTOs;

// ==================== Reportes ====================
public record ComplianceReportDto(
    Guid Id,
    string ReportNumber,
    ReportType ReportType,
    RegulatoryBody RegulatoryBody,
    string Period,
    DateTime StartDate,
    DateTime EndDate,
    ReportStatus Status,
    DateTime GeneratedAt,
    string GeneratedBy,
    DateTime? SubmittedAt,
    int ItemCount
);

public record ComplianceReportDetailDto(
    Guid Id,
    string ReportNumber,
    ReportType ReportType,
    RegulatoryBody RegulatoryBody,
    string Period,
    DateTime StartDate,
    DateTime EndDate,
    ReportStatus Status,
    string? ReportContent,
    string? FilePath,
    string? SubmissionReference,
    DateTime GeneratedAt,
    string GeneratedBy,
    DateTime? SubmittedAt,
    string? RejectionReason,
    IEnumerable<ReportItemDto> Items,
    IEnumerable<ReportSubmissionDto> Submissions
);

public record GenerateReportDto(
    ReportType ReportType,
    RegulatoryBody RegulatoryBody,
    string Period,
    DateTime StartDate,
    DateTime EndDate
);

// ==================== Items ====================
public record ReportItemDto(
    Guid Id,
    string ItemType,
    string ItemData,
    decimal? Amount,
    string? ReferenceNumber,
    DateTime ItemDate
);

// ==================== Submissions ====================
public record ReportSubmissionDto(
    Guid Id,
    DateTime SubmittedAt,
    string SubmissionMethod,
    string? SubmissionReference,
    bool IsSuccessful,
    string? ResponseMessage,
    string SubmittedBy
);

public record SubmitReportDto(
    Guid ReportId,
    string SubmissionMethod
);

// ==================== Schedules ====================
public record ReportScheduleDto(
    Guid Id,
    ReportType ReportType,
    RegulatoryBody RegulatoryBody,
    string CronExpression,
    bool IsActive,
    string? Description,
    DateTime? LastRunAt,
    DateTime? NextRunAt
);

public record CreateScheduleDto(
    ReportType ReportType,
    RegulatoryBody RegulatoryBody,
    string CronExpression,
    string? Description
);

// ==================== Templates ====================
public record ReportTemplateDto(
    Guid Id,
    ReportType ReportType,
    RegulatoryBody RegulatoryBody,
    string TemplateName,
    string TemplateVersion,
    bool IsActive,
    DateTime ValidFrom,
    DateTime? ValidTo
);

public record CreateTemplateDto(
    ReportType ReportType,
    RegulatoryBody RegulatoryBody,
    string TemplateName,
    string TemplateVersion,
    string TemplateContent,
    DateTime ValidFrom,
    DateTime? ValidTo
);

// ==================== Statistics ====================
public record ReportingStatisticsDto(
    int TotalReports,
    int PendingReports,
    int SubmittedReports,
    int AcceptedReports,
    int RejectedReports,
    int ActiveSchedules,
    Dictionary<string, int> ReportsByBody
);

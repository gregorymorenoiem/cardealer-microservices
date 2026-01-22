// =====================================================
// ComplianceReportingService - Queries
// Reportes Consolidados de Cumplimiento RD
// =====================================================

using MediatR;
using ComplianceReportingService.Application.DTOs;
using ComplianceReportingService.Domain.Enums;

namespace ComplianceReportingService.Application.Queries;

// ==================== Reportes ====================
public record GetReportByIdQuery(Guid Id) : IRequest<ComplianceReportDetailDto?>;
public record GetReportByNumberQuery(string ReportNumber) : IRequest<ComplianceReportDetailDto?>;
public record GetReportsByBodyQuery(RegulatoryBody Body) : IRequest<IEnumerable<ComplianceReportDto>>;
public record GetReportsByPeriodQuery(string Period) : IRequest<IEnumerable<ComplianceReportDto>>;
public record GetReportsByStatusQuery(ReportStatus Status) : IRequest<IEnumerable<ComplianceReportDto>>;
public record GetPendingReportsQuery() : IRequest<IEnumerable<ComplianceReportDto>>;

// ==================== Schedules ====================
public record GetScheduleByIdQuery(Guid Id) : IRequest<ReportScheduleDto?>;
public record GetActiveSchedulesQuery() : IRequest<IEnumerable<ReportScheduleDto>>;
public record GetDueSchedulesQuery() : IRequest<IEnumerable<ReportScheduleDto>>;

// ==================== Templates ====================
public record GetTemplateByIdQuery(Guid Id) : IRequest<ReportTemplateDto?>;
public record GetActiveTemplateQuery(ReportType Type, RegulatoryBody Body) : IRequest<ReportTemplateDto?>;
public record GetTemplatesByBodyQuery(RegulatoryBody Body) : IRequest<IEnumerable<ReportTemplateDto>>;

// ==================== Statistics ====================
public record GetReportingStatisticsQuery() : IRequest<ReportingStatisticsDto>;

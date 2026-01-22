// =====================================================
// ComplianceReportingService - Commands
// Reportes Consolidados de Cumplimiento RD
// =====================================================

using MediatR;
using ComplianceReportingService.Application.DTOs;

namespace ComplianceReportingService.Application.Commands;

// ==================== Reportes ====================
public record GenerateReportCommand(GenerateReportDto Data) : IRequest<ComplianceReportDto>;
public record ValidateReportCommand(Guid ReportId) : IRequest<bool>;
public record SubmitReportCommand(SubmitReportDto Data) : IRequest<ReportSubmissionDto>;
public record RegenerateReportCommand(Guid ReportId) : IRequest<ComplianceReportDto>;
public record DeleteReportCommand(Guid ReportId) : IRequest<bool>;

// ==================== Schedules ====================
public record CreateScheduleCommand(CreateScheduleDto Data) : IRequest<ReportScheduleDto>;
public record UpdateScheduleCommand(Guid ScheduleId, CreateScheduleDto Data) : IRequest<bool>;
public record ActivateScheduleCommand(Guid ScheduleId) : IRequest<bool>;
public record DeactivateScheduleCommand(Guid ScheduleId) : IRequest<bool>;
public record RunScheduledReportsCommand() : IRequest<IEnumerable<ComplianceReportDto>>;

// ==================== Templates ====================
public record CreateTemplateCommand(CreateTemplateDto Data) : IRequest<ReportTemplateDto>;
public record ActivateTemplateCommand(Guid TemplateId) : IRequest<bool>;
public record DeactivateTemplateCommand(Guid TemplateId) : IRequest<bool>;

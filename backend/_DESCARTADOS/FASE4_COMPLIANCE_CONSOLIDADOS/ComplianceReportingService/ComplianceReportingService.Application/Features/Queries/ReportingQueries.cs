// ComplianceReportingService - Queries
// Consultas para reportería regulatoria

namespace ComplianceReportingService.Application.Features.Queries;

using MediatR;
using ComplianceReportingService.Application.DTOs;
using ComplianceReportingService.Domain.Entities;

#region Report Queries

/// <summary>
/// Obtiene un reporte por ID
/// </summary>
public record GetReportByIdQuery(Guid ReportId) : IRequest<ReportDto?>;

/// <summary>
/// Obtiene un reporte por número
/// </summary>
public record GetReportByNumberQuery(string ReportNumber) : IRequest<ReportDto?>;

/// <summary>
/// Lista reportes con paginación y filtros
/// </summary>
public record GetReportsPagedQuery(
    int Page,
    int PageSize,
    ReportType? Type,
    ReportStatus? Status,
    DestinationType? Destination,
    DateTime? FromDate,
    DateTime? ToDate,
    string? SearchTerm) : IRequest<ReportPagedResultDto>;

/// <summary>
/// Obtiene reportes pendientes de envío
/// </summary>
public record GetPendingReportsQuery(
    DestinationType? Destination) : IRequest<List<ReportSummaryDto>>;

/// <summary>
/// Obtiene reportes vencidos o por vencer
/// </summary>
public record GetOverdueReportsQuery(
    DateTime AsOfDate,
    int DaysAhead) : IRequest<List<ReportSummaryDto>>;

/// <summary>
/// Obtiene reportes por período
/// </summary>
public record GetReportsByPeriodQuery(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    ReportType? Type) : IRequest<List<ReportSummaryDto>>;

#endregion

#region Schedule Queries

/// <summary>
/// Obtiene una programación por ID
/// </summary>
public record GetScheduleByIdQuery(Guid ScheduleId) : IRequest<ReportScheduleDto?>;

/// <summary>
/// Lista todas las programaciones activas
/// </summary>
public record GetActiveSchedulesQuery() : IRequest<List<ReportScheduleDto>>;

/// <summary>
/// Obtiene programaciones por tipo de reporte
/// </summary>
public record GetSchedulesByTypeQuery(
    ReportType ReportType) : IRequest<List<ReportScheduleDto>>;

/// <summary>
/// Obtiene programaciones próximas a ejecutar
/// </summary>
public record GetUpcomingSchedulesQuery(
    DateTime Until) : IRequest<List<ReportScheduleDto>>;

#endregion

#region Template Queries

/// <summary>
/// Obtiene una plantilla por ID
/// </summary>
public record GetTemplateByIdQuery(Guid TemplateId) : IRequest<ReportTemplateDto?>;

/// <summary>
/// Obtiene una plantilla por código
/// </summary>
public record GetTemplateByCodeQuery(string Code) : IRequest<ReportTemplateDto?>;

/// <summary>
/// Lista todas las plantillas activas
/// </summary>
public record GetActiveTemplatesQuery() : IRequest<List<ReportTemplateDto>>;

/// <summary>
/// Obtiene plantillas por tipo de reporte
/// </summary>
public record GetTemplatesByTypeQuery(
    ReportType ForReportType) : IRequest<List<ReportTemplateDto>>;

#endregion

#region Execution Queries

/// <summary>
/// Obtiene el historial de ejecuciones de un reporte
/// </summary>
public record GetExecutionsByReportQuery(
    Guid ReportId) : IRequest<List<ReportExecutionDto>>;

/// <summary>
/// Obtiene el historial de ejecuciones de una programación
/// </summary>
public record GetExecutionsByScheduleQuery(
    Guid ScheduleId,
    int Limit) : IRequest<List<ReportExecutionDto>>;

/// <summary>
/// Obtiene ejecuciones fallidas recientes
/// </summary>
public record GetFailedExecutionsQuery(
    DateTime Since) : IRequest<List<ReportExecutionDto>>;

#endregion

#region Subscription Queries

/// <summary>
/// Obtiene una suscripción por ID
/// </summary>
public record GetSubscriptionByIdQuery(Guid SubscriptionId) : IRequest<ReportSubscriptionDto?>;

/// <summary>
/// Obtiene suscripciones de un usuario
/// </summary>
public record GetUserSubscriptionsQuery(
    Guid UserId) : IRequest<List<ReportSubscriptionDto>>;

/// <summary>
/// Obtiene suscriptores de un tipo de reporte
/// </summary>
public record GetSubscribersByTypeQuery(
    ReportType ReportType) : IRequest<List<ReportSubscriptionDto>>;

#endregion

#region DGII Queries

/// <summary>
/// Obtiene una presentación DGII por ID
/// </summary>
public record GetDGIISubmissionByIdQuery(Guid SubmissionId) : IRequest<DGIISubmissionDto?>;

/// <summary>
/// Obtiene presentaciones DGII por período
/// </summary>
public record GetDGIISubmissionsByPeriodQuery(
    string Period,
    string? ReportCode) : IRequest<List<DGIISubmissionDto>>;

/// <summary>
/// Obtiene el estado de cumplimiento DGII
/// </summary>
public record GetDGIIComplianceStatusQuery(
    string RNC,
    int Year) : IRequest<Dictionary<string, string>>;

/// <summary>
/// Obtiene presentaciones DGII pendientes
/// </summary>
public record GetPendingDGIISubmissionsQuery() : IRequest<List<DGIISubmissionDto>>;

#endregion

#region UAF Queries

/// <summary>
/// Obtiene una presentación UAF por ID
/// </summary>
public record GetUAFSubmissionByIdQuery(Guid SubmissionId) : IRequest<UAFSubmissionDto?>;

/// <summary>
/// Obtiene presentaciones UAF por período
/// </summary>
public record GetUAFSubmissionsByPeriodQuery(
    string ReportingPeriod) : IRequest<List<UAFSubmissionDto>>;

/// <summary>
/// Obtiene ROS urgentes pendientes
/// </summary>
public record GetUrgentROSPendingQuery() : IRequest<List<UAFSubmissionDto>>;

/// <summary>
/// Obtiene el estado de cumplimiento UAF/PLD
/// </summary>
public record GetUAFComplianceStatusQuery(
    int Year) : IRequest<Dictionary<string, object>>;

#endregion

#region Compliance Queries

/// <summary>
/// Obtiene métricas de cumplimiento actuales
/// </summary>
public record GetCurrentMetricsQuery(
    string? Category) : IRequest<List<ComplianceMetricDto>>;

/// <summary>
/// Obtiene el dashboard de cumplimiento
/// </summary>
public record GetComplianceDashboardQuery(
    DateTime AsOfDate) : IRequest<ComplianceDashboardDto>;

/// <summary>
/// Obtiene estadísticas de reportería
/// </summary>
public record GetReportingStatisticsQuery(
    DateTime FromDate,
    DateTime ToDate) : IRequest<ReportingStatisticsDto>;

/// <summary>
/// Obtiene alertas activas de cumplimiento
/// </summary>
public record GetActiveAlertsQuery() : IRequest<List<ComplianceMetricDto>>;

/// <summary>
/// Obtiene historial de métricas
/// </summary>
public record GetMetricHistoryQuery(
    string MetricCode,
    DateTime FromDate,
    DateTime ToDate) : IRequest<List<ComplianceMetricDto>>;

#endregion

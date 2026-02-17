// ComplianceReportingService - Commands
// Comandos para generación y envío de reportes regulatorios

namespace ComplianceReportingService.Application.Features.Commands;

using MediatR;
using ComplianceReportingService.Application.DTOs;
using ComplianceReportingService.Domain.Entities;

#region Report Commands

/// <summary>
/// Genera un nuevo reporte regulatorio
/// </summary>
public record GenerateReportCommand(
    ReportType Type,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    ReportFormat Format,
    DestinationType Destination,
    string? Description,
    string? ParametersJson,
    string UserId) : IRequest<ReportDto>;

/// <summary>
/// Envía un reporte generado a la entidad regulatoria
/// </summary>
public record SubmitReportCommand(
    Guid ReportId,
    string UserId) : IRequest<ReportDto>;

/// <summary>
/// Reintenta el envío de un reporte fallido
/// </summary>
public record RetrySubmissionCommand(
    Guid ReportId,
    string UserId) : IRequest<ReportDto>;

/// <summary>
/// Cancela un reporte pendiente
/// </summary>
public record CancelReportCommand(
    Guid ReportId,
    string Reason,
    string UserId) : IRequest<bool>;

/// <summary>
/// Regenera un reporte existente con los mismos parámetros
/// </summary>
public record RegenerateReportCommand(
    Guid ReportId,
    string UserId) : IRequest<ReportDto>;

/// <summary>
/// Marca un reporte como aceptado (confirmación de la entidad)
/// </summary>
public record ConfirmSubmissionCommand(
    Guid ReportId,
    string ConfirmationNumber,
    string? ResponseMessage) : IRequest<bool>;

#endregion

#region Schedule Commands

/// <summary>
/// Crea una programación automática de reportes
/// </summary>
public record CreateScheduleCommand(
    string Name,
    ReportType ReportType,
    ReportFrequency Frequency,
    ReportFormat Format,
    DestinationType Destination,
    string CronExpression,
    bool AutoSubmit,
    string? NotificationEmail,
    string? ParametersJson,
    string UserId) : IRequest<ReportScheduleDto>;

/// <summary>
/// Actualiza una programación existente
/// </summary>
public record UpdateScheduleCommand(
    Guid ScheduleId,
    string? Name,
    ReportFrequency? Frequency,
    ReportFormat? Format,
    string? CronExpression,
    bool? AutoSubmit,
    string? NotificationEmail,
    string UserId) : IRequest<ReportScheduleDto>;

/// <summary>
/// Activa/desactiva una programación
/// </summary>
public record ToggleScheduleCommand(
    Guid ScheduleId,
    bool IsActive,
    string UserId) : IRequest<bool>;

/// <summary>
/// Ejecuta manualmente una programación
/// </summary>
public record ExecuteScheduleCommand(
    Guid ScheduleId,
    string UserId) : IRequest<ReportDto>;

/// <summary>
/// Elimina una programación
/// </summary>
public record DeleteScheduleCommand(
    Guid ScheduleId,
    string UserId) : IRequest<bool>;

#endregion

#region Template Commands

/// <summary>
/// Crea una nueva plantilla de reporte
/// </summary>
public record CreateTemplateCommand(
    string Code,
    string Name,
    ReportType ForReportType,
    string? Description,
    string TemplateContent,
    string? QueryDefinition,
    string? ParametersSchema,
    string UserId) : IRequest<ReportTemplateDto>;

/// <summary>
/// Actualiza una plantilla existente
/// </summary>
public record UpdateTemplateCommand(
    Guid TemplateId,
    string? Name,
    string? Description,
    string? TemplateContent,
    string? QueryDefinition,
    string? ParametersSchema,
    string UserId) : IRequest<ReportTemplateDto>;

/// <summary>
/// Activa/desactiva una plantilla
/// </summary>
public record ToggleTemplateCommand(
    Guid TemplateId,
    bool IsActive,
    string UserId) : IRequest<bool>;

#endregion

#region Subscription Commands

/// <summary>
/// Crea una suscripción de reportes para un usuario
/// </summary>
public record CreateSubscriptionCommand(
    Guid UserId,
    ReportType ReportType,
    ReportFrequency Frequency,
    string DeliveryMethod,
    string DeliveryAddress,
    string CreatedBy) : IRequest<ReportSubscriptionDto>;

/// <summary>
/// Actualiza una suscripción
/// </summary>
public record UpdateSubscriptionCommand(
    Guid SubscriptionId,
    ReportFrequency? Frequency,
    string? DeliveryMethod,
    string? DeliveryAddress,
    string UserId) : IRequest<ReportSubscriptionDto>;

/// <summary>
/// Cancela una suscripción
/// </summary>
public record CancelSubscriptionCommand(
    Guid SubscriptionId,
    string UserId) : IRequest<bool>;

#endregion

#region DGII Commands

/// <summary>
/// Genera y envía formato 606 a DGII
/// </summary>
public record GenerateDGII606Command(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string RNC,
    bool AutoSubmit,
    string UserId) : IRequest<DGIISubmissionDto>;

/// <summary>
/// Genera y envía formato 607 a DGII
/// </summary>
public record GenerateDGII607Command(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string RNC,
    bool AutoSubmit,
    string UserId) : IRequest<DGIISubmissionDto>;

/// <summary>
/// Genera y envía formato 608 (anulaciones)
/// </summary>
public record GenerateDGII608Command(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string RNC,
    bool AutoSubmit,
    string UserId) : IRequest<DGIISubmissionDto>;

/// <summary>
/// Genera y envía IT1 (ITBIS)
/// </summary>
public record GenerateDGIIIT1Command(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string RNC,
    bool AutoSubmit,
    string UserId) : IRequest<DGIISubmissionDto>;

#endregion

#region UAF Commands

/// <summary>
/// Genera Reporte de Operación Sospechosa (ROS) - Ley 155-17
/// </summary>
public record GenerateROSCommand(
    string SubjectName,
    string SubjectIdType,
    string SubjectIdNumber,
    string TransactionType,
    decimal Amount,
    string Currency,
    DateTime TransactionDate,
    string SuspicionIndicators,
    string Narrative,
    bool IsUrgent,
    string UserId) : IRequest<UAFSubmissionDto>;

/// <summary>
/// Genera Reporte de Transacción en Efectivo (CTR) - Ley 155-17
/// </summary>
public record GenerateCTRCommand(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal ThresholdAmount,
    string UserId) : IRequest<UAFSubmissionDto>;

#endregion

#region Compliance Commands

/// <summary>
/// Registra una métrica de cumplimiento
/// </summary>
public record RecordMetricCommand(
    string MetricCode,
    string MetricName,
    string Category,
    decimal Value,
    decimal? Threshold,
    string? Unit,
    string UserId) : IRequest<ComplianceMetricDto>;

/// <summary>
/// Genera alertas basadas en métricas
/// </summary>
public record GenerateComplianceAlertsCommand(
    DateTime AsOfDate,
    string UserId) : IRequest<List<ComplianceMetricDto>>;

#endregion

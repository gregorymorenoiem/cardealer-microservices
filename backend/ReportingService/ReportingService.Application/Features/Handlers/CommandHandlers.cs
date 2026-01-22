// ReportingService - Command Handlers
// Handlers para generación y envío de reportes regulatorios

namespace ReportingService.Application.Features.Handlers;

using MediatR;
using Microsoft.Extensions.Logging;
using ReportingService.Application.DTOs;
using ReportingService.Application.Features.Commands;
using ReportingService.Domain.Entities;
using ReportingService.Domain.Interfaces;

#region Report Command Handlers

public class GenerateReportHandler : IRequestHandler<GenerateReportCommand, ReportDto>
{
    private readonly IReportRepository _reportRepo;
    private readonly IReportTemplateRepository _templateRepo;
    private readonly IReportExecutionRepository _executionRepo;
    private readonly ILogger<GenerateReportHandler> _logger;

    public GenerateReportHandler(
        IReportRepository reportRepo,
        IReportTemplateRepository templateRepo,
        IReportExecutionRepository executionRepo,
        ILogger<GenerateReportHandler> logger)
    {
        _reportRepo = reportRepo;
        _templateRepo = templateRepo;
        _executionRepo = executionRepo;
        _logger = logger;
    }

    public async Task<ReportDto> Handle(GenerateReportCommand request, CancellationToken ct)
    {
        var execution = new ReportExecution
        {
            Id = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            ExecutedBy = request.UserId
        };

        try
        {
            var report = new Report
            {
                Id = Guid.NewGuid(),
                ReportNumber = GenerateReportNumber(request.Type),
                Type = request.Type,
                Status = ReportStatus.Generating,
                Name = GetReportName(request.Type, request.PeriodStart, request.PeriodEnd),
                Description = request.Description,
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                Format = request.Format,
                Destination = request.Destination,
                ParametersJson = request.ParametersJson,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.UserId,
                DueDate = CalculateDueDate(request.Type, request.PeriodEnd)
            };

            await _reportRepo.AddAsync(report, ct);

            execution.ReportId = report.Id;

            // Simular generación (en producción: llamar a servicio de generación)
            await Task.Delay(100, ct);
            
            report.Status = ReportStatus.Generated;
            report.GeneratedAt = DateTime.UtcNow;
            report.FilePath = $"/reports/{report.Type}/{report.ReportNumber}.{request.Format.ToString().ToLower()}";
            report.FileSize = 1024; // Tamaño real en producción
            report.RecordCount = 0; // Conteo real en producción

            await _reportRepo.UpdateAsync(report, ct);

            execution.CompletedAt = DateTime.UtcNow;
            execution.Success = true;
            execution.DurationMs = (int)(execution.CompletedAt.Value - execution.StartedAt).TotalMilliseconds;
            await _executionRepo.AddAsync(execution, ct);

            _logger.LogInformation("Reporte {ReportNumber} generado exitosamente", report.ReportNumber);

            return MapToDto(report);
        }
        catch (Exception ex)
        {
            execution.CompletedAt = DateTime.UtcNow;
            execution.Success = false;
            execution.ErrorMessage = ex.Message;
            execution.DurationMs = (int)(execution.CompletedAt.Value - execution.StartedAt).TotalMilliseconds;
            await _executionRepo.AddAsync(execution, ct);

            _logger.LogError(ex, "Error generando reporte tipo {Type}", request.Type);
            throw;
        }
    }

    private string GenerateReportNumber(ReportType type)
    {
        var prefix = type switch
        {
            ReportType.DGII_606 => "606",
            ReportType.DGII_607 => "607",
            ReportType.DGII_608 => "608",
            ReportType.DGII_609 => "609",
            ReportType.DGII_IT1 => "IT1",
            ReportType.UAF_ROS => "ROS",
            ReportType.UAF_CTR => "CTR",
            _ => "RPT"
        };
        return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private string GetReportName(ReportType type, DateTime start, DateTime end)
    {
        return $"{type} - Período {start:yyyy-MM-dd} a {end:yyyy-MM-dd}";
    }

    private DateTime? CalculateDueDate(ReportType type, DateTime periodEnd)
    {
        // DGII: Día 15 del mes siguiente
        // UAF ROS: Inmediato (24-48 horas)
        return type switch
        {
            ReportType.DGII_606 or ReportType.DGII_607 or ReportType.DGII_608 
                => new DateTime(periodEnd.Year, periodEnd.Month, 1).AddMonths(1).AddDays(14),
            ReportType.DGII_IT1 
                => new DateTime(periodEnd.Year, periodEnd.Month, 1).AddMonths(1).AddDays(19),
            ReportType.UAF_ROS 
                => DateTime.UtcNow.AddHours(24),
            _ => null
        };
    }

    private ReportDto MapToDto(Report r) => new(
        r.Id, r.ReportNumber, r.Type, r.Status, r.Name, r.Description,
        r.PeriodStart, r.PeriodEnd, r.Format, r.GeneratedAt, r.FilePath,
        r.FileSize, r.Destination, r.SubmittedAt, r.SubmissionReference,
        r.RecordCount, r.TotalAmount, r.Currency, r.CreatedAt, r.DueDate);
}

public class SubmitReportHandler : IRequestHandler<SubmitReportCommand, ReportDto>
{
    private readonly IReportRepository _reportRepo;
    private readonly IDGIISubmissionRepository _dgiiRepo;
    private readonly IUAFSubmissionRepository _uafRepo;
    private readonly ILogger<SubmitReportHandler> _logger;

    public SubmitReportHandler(
        IReportRepository reportRepo,
        IDGIISubmissionRepository dgiiRepo,
        IUAFSubmissionRepository uafRepo,
        ILogger<SubmitReportHandler> logger)
    {
        _reportRepo = reportRepo;
        _dgiiRepo = dgiiRepo;
        _uafRepo = uafRepo;
        _logger = logger;
    }

    public async Task<ReportDto> Handle(SubmitReportCommand request, CancellationToken ct)
    {
        var report = await _reportRepo.GetByIdAsync(request.ReportId, ct)
            ?? throw new InvalidOperationException($"Reporte {request.ReportId} no encontrado");

        if (report.Status != ReportStatus.Generated)
            throw new InvalidOperationException("Solo se pueden enviar reportes en estado 'Generado'");

        report.Status = ReportStatus.Submitting;
        await _reportRepo.UpdateAsync(report, ct);

        try
        {
            // Crear registro de presentación según destino
            if (report.Destination == DestinationType.DGII)
            {
                var submission = new DGIISubmission
                {
                    Id = Guid.NewGuid(),
                    ReportId = report.Id,
                    ReportCode = report.Type.ToString().Replace("DGII_", ""),
                    RNC = "101010101", // Obtener de configuración
                    Period = $"{report.PeriodStart:yyyyMM}",
                    SubmissionDate = DateTime.UtcNow,
                    Status = "Enviado",
                    Attempts = 1
                };
                await _dgiiRepo.AddAsync(submission, ct);
                report.SubmissionReference = $"DGII-{submission.Id.ToString()[..8]}";
            }
            else if (report.Destination == DestinationType.UAF)
            {
                var submission = new UAFSubmission
                {
                    Id = Guid.NewGuid(),
                    ReportId = report.Id,
                    ReportCode = report.Type.ToString().Replace("UAF_", ""),
                    EntityRNC = "101010101",
                    ReportingPeriod = $"{report.PeriodStart:yyyyMM}",
                    SubmissionDate = DateTime.UtcNow,
                    Status = "Enviado",
                    IsUrgent = report.Type == ReportType.UAF_ROS
                };
                await _uafRepo.AddAsync(submission, ct);
                report.SubmissionReference = $"UAF-{submission.Id.ToString()[..8]}";
            }

            report.Status = ReportStatus.Submitted;
            report.SubmittedAt = DateTime.UtcNow;
            await _reportRepo.UpdateAsync(report, ct);

            _logger.LogInformation("Reporte {ReportNumber} enviado a {Destination}", 
                report.ReportNumber, report.Destination);

            return MapToDto(report);
        }
        catch (Exception ex)
        {
            report.Status = ReportStatus.SubmissionFailed;
            await _reportRepo.UpdateAsync(report, ct);
            
            _logger.LogError(ex, "Error enviando reporte {ReportNumber}", report.ReportNumber);
            throw;
        }
    }

    private ReportDto MapToDto(Report r) => new(
        r.Id, r.ReportNumber, r.Type, r.Status, r.Name, r.Description,
        r.PeriodStart, r.PeriodEnd, r.Format, r.GeneratedAt, r.FilePath,
        r.FileSize, r.Destination, r.SubmittedAt, r.SubmissionReference,
        r.RecordCount, r.TotalAmount, r.Currency, r.CreatedAt, r.DueDate);
}

public class CancelReportHandler : IRequestHandler<CancelReportCommand, bool>
{
    private readonly IReportRepository _reportRepo;

    public CancelReportHandler(IReportRepository reportRepo)
    {
        _reportRepo = reportRepo;
    }

    public async Task<bool> Handle(CancelReportCommand request, CancellationToken ct)
    {
        var report = await _reportRepo.GetByIdAsync(request.ReportId, ct);
        if (report == null) return false;

        if (report.Status == ReportStatus.Submitted || report.Status == ReportStatus.Accepted)
            throw new InvalidOperationException("No se pueden cancelar reportes ya enviados o aceptados");

        report.Status = ReportStatus.Cancelled;
        report.Notes = request.Reason;
        await _reportRepo.UpdateAsync(report, ct);

        return true;
    }
}

#endregion

#region Schedule Command Handlers

public class CreateScheduleHandler : IRequestHandler<CreateScheduleCommand, ReportScheduleDto>
{
    private readonly IReportScheduleRepository _scheduleRepo;

    public CreateScheduleHandler(IReportScheduleRepository scheduleRepo)
    {
        _scheduleRepo = scheduleRepo;
    }

    public async Task<ReportScheduleDto> Handle(CreateScheduleCommand request, CancellationToken ct)
    {
        var schedule = new ReportSchedule
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ReportType = request.ReportType,
            Frequency = request.Frequency,
            Format = request.Format,
            Destination = request.Destination,
            CronExpression = request.CronExpression,
            AutoSubmit = request.AutoSubmit,
            NotificationEmail = request.NotificationEmail,
            ParametersJson = request.ParametersJson,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.UserId,
            NextRunAt = CalculateNextRun(request.CronExpression)
        };

        await _scheduleRepo.AddAsync(schedule, ct);

        return new ReportScheduleDto(
            schedule.Id, schedule.Name, schedule.ReportType, schedule.Frequency,
            schedule.Format, schedule.Destination, schedule.CronExpression,
            schedule.NextRunAt, schedule.LastRunAt, schedule.AutoSubmit,
            schedule.NotificationEmail, schedule.IsActive);
    }

    private DateTime? CalculateNextRun(string cron)
    {
        // Simplificado - en producción usar NCronTab o similar
        return DateTime.UtcNow.AddDays(1);
    }
}

public class ToggleScheduleHandler : IRequestHandler<ToggleScheduleCommand, bool>
{
    private readonly IReportScheduleRepository _scheduleRepo;

    public ToggleScheduleHandler(IReportScheduleRepository scheduleRepo)
    {
        _scheduleRepo = scheduleRepo;
    }

    public async Task<bool> Handle(ToggleScheduleCommand request, CancellationToken ct)
    {
        var schedule = await _scheduleRepo.GetByIdAsync(request.ScheduleId, ct);
        if (schedule == null) return false;

        schedule.IsActive = request.IsActive;
        if (request.IsActive)
            schedule.NextRunAt = DateTime.UtcNow.AddDays(1);

        await _scheduleRepo.UpdateAsync(schedule, ct);
        return true;
    }
}

#endregion

#region Template Command Handlers

public class CreateTemplateHandler : IRequestHandler<CreateTemplateCommand, ReportTemplateDto>
{
    private readonly IReportTemplateRepository _templateRepo;

    public CreateTemplateHandler(IReportTemplateRepository templateRepo)
    {
        _templateRepo = templateRepo;
    }

    public async Task<ReportTemplateDto> Handle(CreateTemplateCommand request, CancellationToken ct)
    {
        var template = new ReportTemplate
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            ForReportType = request.ForReportType,
            Description = request.Description,
            TemplateContent = request.TemplateContent,
            QueryDefinition = request.QueryDefinition,
            ParametersSchema = request.ParametersSchema,
            Version = "1.0",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.UserId
        };

        await _templateRepo.AddAsync(template, ct);

        return new ReportTemplateDto(
            template.Id, template.Code, template.Name, template.ForReportType,
            template.Description, template.Version, template.IsActive);
    }
}

#endregion

#region Subscription Command Handlers

public class CreateSubscriptionHandler : IRequestHandler<CreateSubscriptionCommand, ReportSubscriptionDto>
{
    private readonly IReportSubscriptionRepository _subscriptionRepo;

    public CreateSubscriptionHandler(IReportSubscriptionRepository subscriptionRepo)
    {
        _subscriptionRepo = subscriptionRepo;
    }

    public async Task<ReportSubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken ct)
    {
        var subscription = new ReportSubscription
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ReportType = request.ReportType,
            Frequency = request.Frequency,
            DeliveryMethod = request.DeliveryMethod,
            DeliveryAddress = request.DeliveryAddress,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _subscriptionRepo.AddAsync(subscription, ct);

        return new ReportSubscriptionDto(
            subscription.Id, subscription.UserId, subscription.ReportType,
            subscription.Frequency, subscription.DeliveryMethod,
            subscription.DeliveryAddress, subscription.IsActive);
    }
}

#endregion

#region DGII Command Handlers

public class GenerateDGII606Handler : IRequestHandler<GenerateDGII606Command, DGIISubmissionDto>
{
    private readonly IReportRepository _reportRepo;
    private readonly IDGIISubmissionRepository _dgiiRepo;
    private readonly IMediator _mediator;

    public GenerateDGII606Handler(
        IReportRepository reportRepo,
        IDGIISubmissionRepository dgiiRepo,
        IMediator mediator)
    {
        _reportRepo = reportRepo;
        _dgiiRepo = dgiiRepo;
        _mediator = mediator;
    }

    public async Task<DGIISubmissionDto> Handle(GenerateDGII606Command request, CancellationToken ct)
    {
        // Generar reporte base
        var reportDto = await _mediator.Send(new GenerateReportCommand(
            ReportType.DGII_606, request.PeriodStart, request.PeriodEnd,
            ReportFormat.TXT, DestinationType.DGII, "Formato 606 - Compras",
            null, request.UserId), ct);

        var submission = new DGIISubmission
        {
            Id = Guid.NewGuid(),
            ReportId = reportDto.Id,
            ReportCode = "606",
            RNC = request.RNC,
            Period = $"{request.PeriodStart:yyyyMM}",
            SubmissionDate = DateTime.UtcNow,
            Status = request.AutoSubmit ? "Enviado" : "Pendiente",
            Attempts = request.AutoSubmit ? 1 : 0
        };

        await _dgiiRepo.AddAsync(submission, ct);

        if (request.AutoSubmit)
        {
            await _mediator.Send(new SubmitReportCommand(reportDto.Id, request.UserId), ct);
        }

        return new DGIISubmissionDto(
            submission.Id, submission.ReportId, submission.ReportCode, submission.RNC,
            submission.Period, submission.SubmissionDate, submission.Status,
            submission.ConfirmationNumber, submission.ResponseMessage, submission.Attempts);
    }
}

#endregion

#region UAF Command Handlers

public class GenerateROSHandler : IRequestHandler<GenerateROSCommand, UAFSubmissionDto>
{
    private readonly IReportRepository _reportRepo;
    private readonly IUAFSubmissionRepository _uafRepo;
    private readonly IMediator _mediator;

    public GenerateROSHandler(
        IReportRepository reportRepo,
        IUAFSubmissionRepository uafRepo,
        IMediator mediator)
    {
        _reportRepo = reportRepo;
        _uafRepo = uafRepo;
        _mediator = mediator;
    }

    public async Task<UAFSubmissionDto> Handle(GenerateROSCommand request, CancellationToken ct)
    {
        // ROS es urgente por Ley 155-17
        var reportDto = await _mediator.Send(new GenerateReportCommand(
            ReportType.UAF_ROS, DateTime.UtcNow.Date, DateTime.UtcNow.Date,
            ReportFormat.XML, DestinationType.UAF, 
            $"ROS - {request.SubjectName} - {request.TransactionType}",
            System.Text.Json.JsonSerializer.Serialize(new
            {
                request.SubjectName,
                request.SubjectIdType,
                request.SubjectIdNumber,
                request.TransactionType,
                request.Amount,
                request.Currency,
                request.TransactionDate,
                request.SuspicionIndicators,
                request.Narrative
            }),
            request.UserId), ct);

        var submission = new UAFSubmission
        {
            Id = Guid.NewGuid(),
            ReportId = reportDto.Id,
            ReportCode = "ROS",
            EntityRNC = "101010101", // Configuración
            ReportingPeriod = $"{DateTime.UtcNow:yyyyMM}",
            SubmissionDate = DateTime.UtcNow,
            Status = "Pendiente",
            IsUrgent = request.IsUrgent
        };

        await _uafRepo.AddAsync(submission, ct);

        return new UAFSubmissionDto(
            submission.Id, submission.ReportId, submission.ReportCode, submission.EntityRNC,
            submission.ReportingPeriod, submission.SubmissionDate, submission.Status,
            submission.UAFCaseNumber, submission.IsUrgent);
    }
}

#endregion

#region Compliance Command Handlers

public class RecordMetricHandler : IRequestHandler<RecordMetricCommand, ComplianceMetricDto>
{
    private readonly IComplianceMetricRepository _metricRepo;

    public RecordMetricHandler(IComplianceMetricRepository metricRepo)
    {
        _metricRepo = metricRepo;
    }

    public async Task<ComplianceMetricDto> Handle(RecordMetricCommand request, CancellationToken ct)
    {
        var isAlert = request.Threshold.HasValue && request.Value > request.Threshold.Value;

        var metric = new ComplianceMetric
        {
            Id = Guid.NewGuid(),
            MetricCode = request.MetricCode,
            MetricName = request.MetricName,
            Category = request.Category,
            Value = request.Value,
            Threshold = request.Threshold,
            Unit = request.Unit,
            MeasuredAt = DateTime.UtcNow,
            IsAlert = isAlert,
            AlertMessage = isAlert ? $"Valor {request.Value} excede umbral {request.Threshold}" : null,
            RecordedBy = request.UserId
        };

        await _metricRepo.AddAsync(metric, ct);

        return new ComplianceMetricDto(
            metric.Id, metric.MetricCode, metric.MetricName, metric.Category,
            metric.Value, metric.Threshold, metric.Unit, metric.MeasuredAt,
            metric.IsAlert, metric.AlertMessage);
    }
}

#endregion

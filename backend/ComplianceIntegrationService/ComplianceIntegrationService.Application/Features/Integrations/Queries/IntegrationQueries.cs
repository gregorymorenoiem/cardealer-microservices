// =====================================================
// C12: ComplianceIntegrationService - Queries (CQRS)
// Consultas para operaciones de lectura
// =====================================================

using ComplianceIntegrationService.Application.DTOs;
using ComplianceIntegrationService.Domain.Enums;
using ComplianceIntegrationService.Domain.Interfaces;
using MediatR;

namespace ComplianceIntegrationService.Application.Features.Integrations.Queries;

#region Integration Config Queries

/// <summary>
/// Query para obtener todas las integraciones
/// </summary>
public record GetAllIntegrationsQuery : IRequest<List<IntegrationConfigDto>>;

public class GetAllIntegrationsHandler : IRequestHandler<GetAllIntegrationsQuery, List<IntegrationConfigDto>>
{
    private readonly IIntegrationConfigRepository _repository;

    public GetAllIntegrationsHandler(IIntegrationConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<IntegrationConfigDto>> Handle(GetAllIntegrationsQuery request, CancellationToken ct)
    {
        var configs = await _repository.GetAllAsync(ct);
        
        return configs.Select(c => new IntegrationConfigDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            RegulatoryBody = c.RegulatoryBody,
            RegulatoryBodyName = c.RegulatoryBody.ToString(),
            IntegrationType = c.IntegrationType,
            IntegrationTypeName = c.IntegrationType.ToString(),
            Status = c.Status,
            StatusName = c.Status.ToString(),
            EndpointUrl = c.EndpointUrl,
            IsSandboxMode = c.IsSandboxMode,
            SyncFrequency = c.SyncFrequency,
            SyncFrequencyName = c.SyncFrequency.ToString(),
            ScheduledTime = c.ScheduledTime,
            TimeoutSeconds = c.TimeoutSeconds,
            MaxRetries = c.MaxRetries,
            LastSuccessfulSync = c.LastSuccessfulSync,
            LastFailedSync = c.LastFailedSync,
            ConsecutiveErrors = c.ConsecutiveErrors,
            CreatedAt = c.CreatedAt,
            IsActive = c.IsActive
        }).ToList();
    }
}

/// <summary>
/// Query para obtener integración por ID
/// </summary>
public record GetIntegrationByIdQuery(Guid Id) : IRequest<IntegrationConfigDto?>;

public class GetIntegrationByIdHandler : IRequestHandler<GetIntegrationByIdQuery, IntegrationConfigDto?>
{
    private readonly IIntegrationConfigRepository _repository;
    private readonly IIntegrationCredentialRepository _credentialRepo;
    private readonly IDataTransmissionRepository _transmissionRepo;

    public GetIntegrationByIdHandler(
        IIntegrationConfigRepository repository,
        IIntegrationCredentialRepository credentialRepo,
        IDataTransmissionRepository transmissionRepo)
    {
        _repository = repository;
        _credentialRepo = credentialRepo;
        _transmissionRepo = transmissionRepo;
    }

    public async Task<IntegrationConfigDto?> Handle(GetIntegrationByIdQuery request, CancellationToken ct)
    {
        var config = await _repository.GetByIdAsync(request.Id, ct);
        if (config == null) return null;

        var credentials = await _credentialRepo.GetByIntegrationIdAsync(request.Id, ct);
        var transmissionCount = await _transmissionRepo.CountByIntegrationAndPeriodAsync(
            request.Id, DateTime.UtcNow.AddYears(-1), DateTime.UtcNow, ct);

        return new IntegrationConfigDto
        {
            Id = config.Id,
            Name = config.Name,
            Description = config.Description,
            RegulatoryBody = config.RegulatoryBody,
            RegulatoryBodyName = config.RegulatoryBody.ToString(),
            IntegrationType = config.IntegrationType,
            IntegrationTypeName = config.IntegrationType.ToString(),
            Status = config.Status,
            StatusName = config.Status.ToString(),
            EndpointUrl = config.EndpointUrl,
            IsSandboxMode = config.IsSandboxMode,
            SyncFrequency = config.SyncFrequency,
            SyncFrequencyName = config.SyncFrequency.ToString(),
            ScheduledTime = config.ScheduledTime,
            TimeoutSeconds = config.TimeoutSeconds,
            MaxRetries = config.MaxRetries,
            LastSuccessfulSync = config.LastSuccessfulSync,
            LastFailedSync = config.LastFailedSync,
            ConsecutiveErrors = config.ConsecutiveErrors,
            CreatedAt = config.CreatedAt,
            IsActive = config.IsActive,
            TransmissionCount = transmissionCount,
            Credentials = credentials.Select(cred => new IntegrationCredentialDto
            {
                Id = cred.Id,
                IntegrationConfigId = cred.IntegrationConfigId,
                Name = cred.Name,
                CredentialType = cred.CredentialType,
                CredentialTypeName = cred.CredentialType.ToString(),
                Username = cred.Username,
                HasPassword = !string.IsNullOrEmpty(cred.PasswordHash),
                HasApiKey = !string.IsNullOrEmpty(cred.ApiKeyHash),
                HasCertificate = !string.IsNullOrEmpty(cred.CertificateData),
                CertificateThumbprint = cred.CertificateThumbprint,
                ExpiresAt = cred.ExpiresAt,
                IsExpired = cred.ExpiresAt.HasValue && cred.ExpiresAt < DateTime.UtcNow,
                IsPrimary = cred.IsPrimary,
                Environment = cred.Environment,
                CreatedAt = cred.CreatedAt
            }).ToList()
        };
    }
}

/// <summary>
/// Query para obtener integraciones por ente regulador
/// </summary>
public record GetIntegrationsByRegulatoryBodyQuery(RegulatoryBody Body) : IRequest<List<IntegrationConfigDto>>;

public class GetIntegrationsByRegulatoryBodyHandler : IRequestHandler<GetIntegrationsByRegulatoryBodyQuery, List<IntegrationConfigDto>>
{
    private readonly IIntegrationConfigRepository _repository;

    public GetIntegrationsByRegulatoryBodyHandler(IIntegrationConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<IntegrationConfigDto>> Handle(GetIntegrationsByRegulatoryBodyQuery request, CancellationToken ct)
    {
        var configs = await _repository.GetByRegulatoryBodyAsync(request.Body, ct);

        return configs.Select(c => new IntegrationConfigDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            RegulatoryBody = c.RegulatoryBody,
            RegulatoryBodyName = c.RegulatoryBody.ToString(),
            IntegrationType = c.IntegrationType,
            IntegrationTypeName = c.IntegrationType.ToString(),
            Status = c.Status,
            StatusName = c.Status.ToString(),
            EndpointUrl = c.EndpointUrl,
            IsSandboxMode = c.IsSandboxMode,
            SyncFrequency = c.SyncFrequency,
            SyncFrequencyName = c.SyncFrequency.ToString(),
            CreatedAt = c.CreatedAt,
            IsActive = c.IsActive
        }).ToList();
    }
}

/// <summary>
/// Query para obtener integraciones activas
/// </summary>
public record GetActiveIntegrationsQuery : IRequest<List<IntegrationConfigDto>>;

public class GetActiveIntegrationsHandler : IRequestHandler<GetActiveIntegrationsQuery, List<IntegrationConfigDto>>
{
    private readonly IIntegrationConfigRepository _repository;

    public GetActiveIntegrationsHandler(IIntegrationConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<IntegrationConfigDto>> Handle(GetActiveIntegrationsQuery request, CancellationToken ct)
    {
        var configs = await _repository.GetActiveIntegrationsAsync(ct);

        return configs.Select(c => new IntegrationConfigDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            RegulatoryBody = c.RegulatoryBody,
            RegulatoryBodyName = c.RegulatoryBody.ToString(),
            IntegrationType = c.IntegrationType,
            IntegrationTypeName = c.IntegrationType.ToString(),
            Status = c.Status,
            StatusName = c.Status.ToString(),
            LastSuccessfulSync = c.LastSuccessfulSync,
            ConsecutiveErrors = c.ConsecutiveErrors,
            CreatedAt = c.CreatedAt,
            IsActive = c.IsActive
        }).ToList();
    }
}

#endregion

#region Transmission Queries

/// <summary>
/// Query para obtener transmisiones por integración
/// </summary>
public record GetTransmissionsByIntegrationQuery(Guid IntegrationId, int? Limit = null) : IRequest<List<DataTransmissionDto>>;

public class GetTransmissionsByIntegrationHandler : IRequestHandler<GetTransmissionsByIntegrationQuery, List<DataTransmissionDto>>
{
    private readonly IDataTransmissionRepository _repository;
    private readonly IIntegrationConfigRepository _configRepo;

    public GetTransmissionsByIntegrationHandler(
        IDataTransmissionRepository repository,
        IIntegrationConfigRepository configRepo)
    {
        _repository = repository;
        _configRepo = configRepo;
    }

    public async Task<List<DataTransmissionDto>> Handle(GetTransmissionsByIntegrationQuery request, CancellationToken ct)
    {
        var transmissions = request.Limit.HasValue
            ? await _repository.GetRecentByIntegrationAsync(request.IntegrationId, request.Limit.Value, ct)
            : await _repository.GetByIntegrationIdAsync(request.IntegrationId, ct);

        var config = await _configRepo.GetByIdAsync(request.IntegrationId, ct);

        return transmissions.Select(t => new DataTransmissionDto
        {
            Id = t.Id,
            IntegrationConfigId = t.IntegrationConfigId,
            IntegrationName = config?.Name ?? "Unknown",
            TransmissionCode = t.TransmissionCode,
            ReportType = t.ReportType,
            ReportTypeName = t.ReportType.ToString(),
            Direction = t.Direction,
            DirectionName = t.Direction.ToString(),
            Status = t.Status,
            StatusName = t.Status.ToString(),
            PeriodStart = t.PeriodStart,
            PeriodEnd = t.PeriodEnd,
            FileName = t.FileName,
            FileSizeBytes = t.FileSizeBytes,
            RecordCount = t.RecordCount,
            TransmissionStartedAt = t.TransmissionStartedAt,
            TransmissionCompletedAt = t.TransmissionCompletedAt,
            DurationMs = t.TransmissionStartedAt.HasValue && t.TransmissionCompletedAt.HasValue
                ? (long)(t.TransmissionCompletedAt.Value - t.TransmissionStartedAt.Value).TotalMilliseconds
                : null,
            ConfirmationNumber = t.ConfirmationNumber,
            ResponseCode = t.ResponseCode,
            ErrorMessage = t.ErrorMessage,
            AttemptCount = t.AttemptCount,
            NextRetryAt = t.NextRetryAt,
            CreatedAt = t.CreatedAt
        }).ToList();
    }
}

/// <summary>
/// Query para obtener transmisión por ID
/// </summary>
public record GetTransmissionByIdQuery(Guid Id) : IRequest<DataTransmissionDto?>;

public class GetTransmissionByIdHandler : IRequestHandler<GetTransmissionByIdQuery, DataTransmissionDto?>
{
    private readonly IDataTransmissionRepository _repository;
    private readonly IIntegrationConfigRepository _configRepo;

    public GetTransmissionByIdHandler(
        IDataTransmissionRepository repository,
        IIntegrationConfigRepository configRepo)
    {
        _repository = repository;
        _configRepo = configRepo;
    }

    public async Task<DataTransmissionDto?> Handle(GetTransmissionByIdQuery request, CancellationToken ct)
    {
        var transmission = await _repository.GetByIdAsync(request.Id, ct);
        if (transmission == null) return null;

        var config = await _configRepo.GetByIdAsync(transmission.IntegrationConfigId, ct);

        return new DataTransmissionDto
        {
            Id = transmission.Id,
            IntegrationConfigId = transmission.IntegrationConfigId,
            IntegrationName = config?.Name ?? "Unknown",
            TransmissionCode = transmission.TransmissionCode,
            ReportType = transmission.ReportType,
            ReportTypeName = transmission.ReportType.ToString(),
            Direction = transmission.Direction,
            DirectionName = transmission.Direction.ToString(),
            Status = transmission.Status,
            StatusName = transmission.Status.ToString(),
            PeriodStart = transmission.PeriodStart,
            PeriodEnd = transmission.PeriodEnd,
            FileName = transmission.FileName,
            FileSizeBytes = transmission.FileSizeBytes,
            RecordCount = transmission.RecordCount,
            TransmissionStartedAt = transmission.TransmissionStartedAt,
            TransmissionCompletedAt = transmission.TransmissionCompletedAt,
            DurationMs = transmission.TransmissionStartedAt.HasValue && transmission.TransmissionCompletedAt.HasValue
                ? (long)(transmission.TransmissionCompletedAt.Value - transmission.TransmissionStartedAt.Value).TotalMilliseconds
                : null,
            ConfirmationNumber = transmission.ConfirmationNumber,
            ResponseCode = transmission.ResponseCode,
            ErrorMessage = transmission.ErrorMessage,
            AttemptCount = transmission.AttemptCount,
            NextRetryAt = transmission.NextRetryAt,
            CreatedAt = transmission.CreatedAt
        };
    }
}

/// <summary>
/// Query para obtener transmisiones pendientes de reintento
/// </summary>
public record GetPendingRetriesQuery : IRequest<List<DataTransmissionDto>>;

public class GetPendingRetriesHandler : IRequestHandler<GetPendingRetriesQuery, List<DataTransmissionDto>>
{
    private readonly IDataTransmissionRepository _repository;

    public GetPendingRetriesHandler(IDataTransmissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<DataTransmissionDto>> Handle(GetPendingRetriesQuery request, CancellationToken ct)
    {
        var transmissions = await _repository.GetPendingRetriesAsync(DateTime.UtcNow, ct);

        return transmissions.Select(t => new DataTransmissionDto
        {
            Id = t.Id,
            IntegrationConfigId = t.IntegrationConfigId,
            TransmissionCode = t.TransmissionCode,
            ReportType = t.ReportType,
            ReportTypeName = t.ReportType.ToString(),
            Status = t.Status,
            StatusName = t.Status.ToString(),
            ErrorMessage = t.ErrorMessage,
            AttemptCount = t.AttemptCount,
            NextRetryAt = t.NextRetryAt,
            CreatedAt = t.CreatedAt
        }).ToList();
    }
}

/// <summary>
/// Query para obtener transmisiones por rango de fechas
/// </summary>
public record GetTransmissionsByDateRangeQuery(DateTime From, DateTime To) : IRequest<List<DataTransmissionDto>>;

public class GetTransmissionsByDateRangeHandler : IRequestHandler<GetTransmissionsByDateRangeQuery, List<DataTransmissionDto>>
{
    private readonly IDataTransmissionRepository _repository;

    public GetTransmissionsByDateRangeHandler(IDataTransmissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<DataTransmissionDto>> Handle(GetTransmissionsByDateRangeQuery request, CancellationToken ct)
    {
        var transmissions = await _repository.GetByDateRangeAsync(request.From, request.To, ct);

        return transmissions.Select(t => new DataTransmissionDto
        {
            Id = t.Id,
            IntegrationConfigId = t.IntegrationConfigId,
            TransmissionCode = t.TransmissionCode,
            ReportType = t.ReportType,
            ReportTypeName = t.ReportType.ToString(),
            Direction = t.Direction,
            DirectionName = t.Direction.ToString(),
            Status = t.Status,
            StatusName = t.Status.ToString(),
            RecordCount = t.RecordCount,
            ConfirmationNumber = t.ConfirmationNumber,
            CreatedAt = t.CreatedAt
        }).ToList();
    }
}

#endregion

#region Field Mapping Queries

/// <summary>
/// Query para obtener mapeos por integración
/// </summary>
public record GetFieldMappingsByIntegrationQuery(Guid IntegrationId, ReportType? ReportType = null) : IRequest<List<FieldMappingDto>>;

public class GetFieldMappingsByIntegrationHandler : IRequestHandler<GetFieldMappingsByIntegrationQuery, List<FieldMappingDto>>
{
    private readonly IFieldMappingRepository _repository;

    public GetFieldMappingsByIntegrationHandler(IFieldMappingRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<FieldMappingDto>> Handle(GetFieldMappingsByIntegrationQuery request, CancellationToken ct)
    {
        var mappings = request.ReportType.HasValue
            ? await _repository.GetByReportTypeAsync(request.IntegrationId, request.ReportType.Value, ct)
            : await _repository.GetByIntegrationIdAsync(request.IntegrationId, ct);

        return mappings.OrderBy(m => m.FieldOrder).Select(m => new FieldMappingDto
        {
            Id = m.Id,
            IntegrationConfigId = m.IntegrationConfigId,
            ReportType = m.ReportType,
            ReportTypeName = m.ReportType.ToString(),
            SourceField = m.SourceField,
            TargetField = m.TargetField,
            SourceDataType = m.SourceDataType,
            TargetDataType = m.TargetDataType,
            Transformation = m.Transformation,
            DefaultValue = m.DefaultValue,
            IsRequired = m.IsRequired,
            MaxLength = m.MaxLength,
            ValidationPattern = m.ValidationPattern,
            FieldOrder = m.FieldOrder,
            Description = m.Description
        }).ToList();
    }
}

#endregion

#region Integration Log Queries

/// <summary>
/// Query para obtener logs por integración
/// </summary>
public record GetLogsByIntegrationQuery(Guid IntegrationId, int Limit = 100) : IRequest<List<IntegrationLogDto>>;

public class GetLogsByIntegrationHandler : IRequestHandler<GetLogsByIntegrationQuery, List<IntegrationLogDto>>
{
    private readonly IIntegrationLogRepository _repository;

    public GetLogsByIntegrationHandler(IIntegrationLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<IntegrationLogDto>> Handle(GetLogsByIntegrationQuery request, CancellationToken ct)
    {
        var logs = await _repository.GetByIntegrationIdAsync(request.IntegrationId, request.Limit, ct);

        return logs.Select(l => new IntegrationLogDto
        {
            Id = l.Id,
            IntegrationConfigId = l.IntegrationConfigId,
            DataTransmissionId = l.DataTransmissionId,
            Severity = l.Severity,
            SeverityName = l.Severity.ToString(),
            Category = l.Category,
            Message = l.Message,
            Details = l.Details,
            DurationMs = l.DurationMs,
            CorrelationId = l.CorrelationId,
            CreatedAt = l.CreatedAt
        }).ToList();
    }
}

/// <summary>
/// Query para obtener errores recientes
/// </summary>
public record GetRecentErrorsQuery(int Count = 50) : IRequest<List<IntegrationLogDto>>;

public class GetRecentErrorsHandler : IRequestHandler<GetRecentErrorsQuery, List<IntegrationLogDto>>
{
    private readonly IIntegrationLogRepository _repository;

    public GetRecentErrorsHandler(IIntegrationLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<IntegrationLogDto>> Handle(GetRecentErrorsQuery request, CancellationToken ct)
    {
        var logs = await _repository.GetRecentErrorsAsync(request.Count, ct);

        return logs.Select(l => new IntegrationLogDto
        {
            Id = l.Id,
            IntegrationConfigId = l.IntegrationConfigId,
            DataTransmissionId = l.DataTransmissionId,
            Severity = l.Severity,
            SeverityName = l.Severity.ToString(),
            Category = l.Category,
            Message = l.Message,
            Details = l.Details,
            DurationMs = l.DurationMs,
            CorrelationId = l.CorrelationId,
            CreatedAt = l.CreatedAt
        }).ToList();
    }
}

#endregion

#region Webhook Queries

/// <summary>
/// Query para obtener webhooks por integración
/// </summary>
public record GetWebhooksByIntegrationQuery(Guid IntegrationId) : IRequest<List<WebhookConfigDto>>;

public class GetWebhooksByIntegrationHandler : IRequestHandler<GetWebhooksByIntegrationQuery, List<WebhookConfigDto>>
{
    private readonly IWebhookConfigRepository _repository;

    public GetWebhooksByIntegrationHandler(IWebhookConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<WebhookConfigDto>> Handle(GetWebhooksByIntegrationQuery request, CancellationToken ct)
    {
        var webhooks = await _repository.GetByIntegrationIdAsync(request.IntegrationId, ct);

        return webhooks.Select(w => new WebhookConfigDto
        {
            Id = w.Id,
            IntegrationConfigId = w.IntegrationConfigId,
            Name = w.Name,
            WebhookUrl = w.WebhookUrl,
            SubscribedEvents = w.SubscribedEvents,
            IsEnabled = w.IsEnabled,
            LastEventReceivedAt = w.LastEventReceivedAt,
            EventCount = w.EventCount,
            CreatedAt = w.CreatedAt
        }).ToList();
    }
}

#endregion

#region Statistics Queries

/// <summary>
/// Query para obtener estadísticas generales
/// </summary>
public record GetIntegrationStatisticsQuery : IRequest<IntegrationStatisticsDto>;

public class GetIntegrationStatisticsHandler : IRequestHandler<GetIntegrationStatisticsQuery, IntegrationStatisticsDto>
{
    private readonly IIntegrationConfigRepository _configRepo;
    private readonly IDataTransmissionRepository _transmissionRepo;
    private readonly IIntegrationCredentialRepository _credentialRepo;
    private readonly IIntegrationLogRepository _logRepo;

    public GetIntegrationStatisticsHandler(
        IIntegrationConfigRepository configRepo,
        IDataTransmissionRepository transmissionRepo,
        IIntegrationCredentialRepository credentialRepo,
        IIntegrationLogRepository logRepo)
    {
        _configRepo = configRepo;
        _transmissionRepo = transmissionRepo;
        _credentialRepo = credentialRepo;
        _logRepo = logRepo;
    }

    public async Task<IntegrationStatisticsDto> Handle(GetIntegrationStatisticsQuery request, CancellationToken ct)
    {
        var allConfigs = await _configRepo.GetAllAsync(ct);
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var next30Days = DateTime.UtcNow.AddDays(30);

        var todayTransmissions = await _transmissionRepo.GetByDateRangeAsync(today, tomorrow, ct);
        var expiringCredentials = await _credentialRepo.GetExpiringCredentialsAsync(next30Days, ct);
        var recentErrors = await _logRepo.GetRecentErrorsAsync(10, ct);
        var pendingTransmissions = await _transmissionRepo.CountByStatusAsync(TransmissionStatus.Pending, ct);

        return new IntegrationStatisticsDto
        {
            TotalIntegrations = allConfigs.Count,
            ActiveIntegrations = allConfigs.Count(c => c.Status == IntegrationStatus.Active),
            InErrorIntegrations = allConfigs.Count(c => c.Status == IntegrationStatus.Error),
            TotalTransmissionsToday = todayTransmissions.Count,
            SuccessfulTransmissionsToday = todayTransmissions.Count(t => t.Status == TransmissionStatus.Success),
            FailedTransmissionsToday = todayTransmissions.Count(t => t.Status == TransmissionStatus.Failed),
            PendingTransmissions = pendingTransmissions,
            ExpiringCredentials = expiringCredentials.Count,
            RecentErrors = recentErrors.Count,
            TransmissionsByRegulatoryBody = allConfigs
                .GroupBy(c => c.RegulatoryBody.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            TransmissionsByStatus = todayTransmissions
                .GroupBy(t => t.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}

/// <summary>
/// Query para obtener estadísticas de integración específica
/// </summary>
public record GetIntegrationDetailStatisticsQuery(Guid IntegrationId) : IRequest<IntegrationDetailStatisticsDto?>;

public class GetIntegrationDetailStatisticsHandler : IRequestHandler<GetIntegrationDetailStatisticsQuery, IntegrationDetailStatisticsDto?>
{
    private readonly IIntegrationConfigRepository _configRepo;
    private readonly IDataTransmissionRepository _transmissionRepo;

    public GetIntegrationDetailStatisticsHandler(
        IIntegrationConfigRepository configRepo,
        IDataTransmissionRepository transmissionRepo)
    {
        _configRepo = configRepo;
        _transmissionRepo = transmissionRepo;
    }

    public async Task<IntegrationDetailStatisticsDto?> Handle(GetIntegrationDetailStatisticsQuery request, CancellationToken ct)
    {
        var config = await _configRepo.GetByIdAsync(request.IntegrationId, ct);
        if (config == null) return null;

        var now = DateTime.UtcNow;
        var transmissions = await _transmissionRepo.GetByIntegrationIdAsync(request.IntegrationId, ct);

        var last24Hours = transmissions.Count(t => t.CreatedAt >= now.AddHours(-24));
        var last7Days = transmissions.Count(t => t.CreatedAt >= now.AddDays(-7));
        var last30Days = transmissions.Count(t => t.CreatedAt >= now.AddDays(-30));

        var successful = transmissions.Count(t => t.Status == TransmissionStatus.Success);
        var failed = transmissions.Count(t => t.Status == TransmissionStatus.Failed);
        var total = transmissions.Count;

        var completedTransmissions = transmissions
            .Where(t => t.TransmissionStartedAt.HasValue && t.TransmissionCompletedAt.HasValue)
            .ToList();

        var avgDuration = completedTransmissions.Any()
            ? (long)completedTransmissions.Average(t => 
                (t.TransmissionCompletedAt!.Value - t.TransmissionStartedAt!.Value).TotalMilliseconds)
            : 0;

        // Transmisiones por día (últimos 30 días)
        var byDay = transmissions
            .Where(t => t.CreatedAt >= now.AddDays(-30))
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new TransmissionsByDayDto
            {
                Date = g.Key,
                Total = g.Count(),
                Successful = g.Count(t => t.Status == TransmissionStatus.Success),
                Failed = g.Count(t => t.Status == TransmissionStatus.Failed)
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new IntegrationDetailStatisticsDto
        {
            IntegrationId = config.Id,
            IntegrationName = config.Name,
            TotalTransmissions = total,
            SuccessfulTransmissions = successful,
            FailedTransmissions = failed,
            SuccessRate = total > 0 ? Math.Round((double)successful / total * 100, 2) : 0,
            AverageDurationMs = avgDuration,
            LastSuccessfulTransmission = config.LastSuccessfulSync,
            LastFailedTransmission = config.LastFailedSync,
            TransmissionsLast24Hours = last24Hours,
            TransmissionsLast7Days = last7Days,
            TransmissionsLast30Days = last30Days,
            TransmissionsByDay = byDay
        };
    }
}

/// <summary>
/// Query para verificar salud de integraciones
/// </summary>
public record GetIntegrationsHealthQuery : IRequest<List<IntegrationHealthDto>>;

public class GetIntegrationsHealthHandler : IRequestHandler<GetIntegrationsHealthQuery, List<IntegrationHealthDto>>
{
    private readonly IIntegrationConfigRepository _configRepo;
    private readonly IIntegrationCredentialRepository _credentialRepo;

    public GetIntegrationsHealthHandler(
        IIntegrationConfigRepository configRepo,
        IIntegrationCredentialRepository credentialRepo)
    {
        _configRepo = configRepo;
        _credentialRepo = credentialRepo;
    }

    public async Task<List<IntegrationHealthDto>> Handle(GetIntegrationsHealthQuery request, CancellationToken ct)
    {
        var configs = await _configRepo.GetActiveIntegrationsAsync(ct);
        var result = new List<IntegrationHealthDto>();

        foreach (var config in configs)
        {
            var credentials = await _credentialRepo.GetByIntegrationIdAsync(config.Id, ct);
            var primaryCred = credentials.FirstOrDefault(c => c.IsPrimary);
            var credentialsValid = primaryCred != null && 
                (!primaryCred.ExpiresAt.HasValue || primaryCred.ExpiresAt > DateTime.UtcNow);

            result.Add(new IntegrationHealthDto
            {
                IntegrationId = config.Id,
                IntegrationName = config.Name,
                RegulatoryBody = config.RegulatoryBody,
                RegulatoryBodyName = config.RegulatoryBody.ToString(),
                Status = config.Status,
                IsHealthy = config.Status == IntegrationStatus.Active && config.ConsecutiveErrors < 3,
                LastError = config.ConsecutiveErrors > 0 ? "Errores consecutivos detectados" : null,
                LastSuccessfulSync = config.LastSuccessfulSync,
                ConsecutiveErrors = config.ConsecutiveErrors,
                CredentialsValid = credentialsValid,
                EndpointReachable = true // En producción: verificar conectividad real
            });
        }

        return result;
    }
}

#endregion

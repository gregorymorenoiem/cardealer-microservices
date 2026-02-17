// =====================================================
// C12: ComplianceIntegrationService - Commands (CQRS)
// Comandos para operaciones de escritura
// =====================================================

using ComplianceIntegrationService.Application.DTOs;
using ComplianceIntegrationService.Domain.Entities;
using ComplianceIntegrationService.Domain.Enums;
using ComplianceIntegrationService.Domain.Interfaces;
using MediatR;

namespace ComplianceIntegrationService.Application.Features.Integrations.Commands;

#region Integration Config Commands

/// <summary>
/// Comando para crear una nueva configuración de integración
/// </summary>
public record CreateIntegrationConfigCommand(CreateIntegrationConfigDto Dto, string UserId) : IRequest<IntegrationConfigDto>;

public class CreateIntegrationConfigHandler : IRequestHandler<CreateIntegrationConfigCommand, IntegrationConfigDto>
{
    private readonly IIntegrationConfigRepository _repository;

    public CreateIntegrationConfigHandler(IIntegrationConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<IntegrationConfigDto> Handle(CreateIntegrationConfigCommand request, CancellationToken ct)
    {
        var config = new IntegrationConfig
        {
            Name = request.Dto.Name,
            Description = request.Dto.Description,
            RegulatoryBody = request.Dto.RegulatoryBody,
            IntegrationType = request.Dto.IntegrationType,
            Status = IntegrationStatus.Configuring,
            EndpointUrl = request.Dto.EndpointUrl,
            SandboxUrl = request.Dto.SandboxUrl,
            Port = request.Dto.Port,
            IsSandboxMode = request.Dto.IsSandboxMode,
            SyncFrequency = request.Dto.SyncFrequency,
            ScheduledTime = request.Dto.ScheduledTime,
            ScheduledDays = request.Dto.ScheduledDays,
            TimeoutSeconds = request.Dto.TimeoutSeconds,
            MaxRetries = request.Dto.MaxRetries,
            RetryIntervalSeconds = request.Dto.RetryIntervalSeconds,
            RequiresSsl = request.Dto.RequiresSsl,
            ProtocolVersion = request.Dto.ProtocolVersion,
            AdditionalConfig = request.Dto.AdditionalConfig,
            Notes = request.Dto.Notes,
            CreatedBy = request.UserId
        };

        var created = await _repository.AddAsync(config, ct);

        return MapToDto(created);
    }

    private static IntegrationConfigDto MapToDto(IntegrationConfig entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        RegulatoryBody = entity.RegulatoryBody,
        RegulatoryBodyName = entity.RegulatoryBody.ToString(),
        IntegrationType = entity.IntegrationType,
        IntegrationTypeName = entity.IntegrationType.ToString(),
        Status = entity.Status,
        StatusName = entity.Status.ToString(),
        EndpointUrl = entity.EndpointUrl,
        IsSandboxMode = entity.IsSandboxMode,
        SyncFrequency = entity.SyncFrequency,
        SyncFrequencyName = entity.SyncFrequency.ToString(),
        ScheduledTime = entity.ScheduledTime,
        TimeoutSeconds = entity.TimeoutSeconds,
        MaxRetries = entity.MaxRetries,
        LastSuccessfulSync = entity.LastSuccessfulSync,
        LastFailedSync = entity.LastFailedSync,
        ConsecutiveErrors = entity.ConsecutiveErrors,
        CreatedAt = entity.CreatedAt,
        IsActive = entity.IsActive
    };
}

/// <summary>
/// Comando para actualizar configuración de integración
/// </summary>
public record UpdateIntegrationConfigCommand(Guid Id, UpdateIntegrationConfigDto Dto, string UserId) : IRequest<IntegrationConfigDto?>;

public class UpdateIntegrationConfigHandler : IRequestHandler<UpdateIntegrationConfigCommand, IntegrationConfigDto?>
{
    private readonly IIntegrationConfigRepository _repository;

    public UpdateIntegrationConfigHandler(IIntegrationConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<IntegrationConfigDto?> Handle(UpdateIntegrationConfigCommand request, CancellationToken ct)
    {
        var config = await _repository.GetByIdAsync(request.Id, ct);
        if (config == null) return null;

        config.Name = request.Dto.Name;
        config.Description = request.Dto.Description;
        config.EndpointUrl = request.Dto.EndpointUrl;
        config.SandboxUrl = request.Dto.SandboxUrl;
        config.Port = request.Dto.Port;
        config.IsSandboxMode = request.Dto.IsSandboxMode;
        config.SyncFrequency = request.Dto.SyncFrequency;
        config.ScheduledTime = request.Dto.ScheduledTime;
        config.ScheduledDays = request.Dto.ScheduledDays;
        config.TimeoutSeconds = request.Dto.TimeoutSeconds;
        config.MaxRetries = request.Dto.MaxRetries;
        config.RetryIntervalSeconds = request.Dto.RetryIntervalSeconds;
        config.RequiresSsl = request.Dto.RequiresSsl;
        config.ProtocolVersion = request.Dto.ProtocolVersion;
        config.AdditionalConfig = request.Dto.AdditionalConfig;
        config.Notes = request.Dto.Notes;
        config.UpdatedAt = DateTime.UtcNow;
        config.UpdatedBy = request.UserId;

        await _repository.UpdateAsync(config, ct);

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
            CreatedAt = config.CreatedAt,
            IsActive = config.IsActive
        };
    }
}

/// <summary>
/// Comando para cambiar estado de integración
/// </summary>
public record ChangeIntegrationStatusCommand(Guid Id, IntegrationStatus NewStatus, string? Reason, string UserId) : IRequest<bool>;

public class ChangeIntegrationStatusHandler : IRequestHandler<ChangeIntegrationStatusCommand, bool>
{
    private readonly IIntegrationConfigRepository _configRepo;
    private readonly IIntegrationStatusHistoryRepository _historyRepo;

    public ChangeIntegrationStatusHandler(
        IIntegrationConfigRepository configRepo,
        IIntegrationStatusHistoryRepository historyRepo)
    {
        _configRepo = configRepo;
        _historyRepo = historyRepo;
    }

    public async Task<bool> Handle(ChangeIntegrationStatusCommand request, CancellationToken ct)
    {
        var config = await _configRepo.GetByIdAsync(request.Id, ct);
        if (config == null) return false;

        var previousStatus = config.Status;

        // Registrar historial
        var history = new IntegrationStatusHistory
        {
            IntegrationConfigId = config.Id,
            PreviousStatus = previousStatus,
            NewStatus = request.NewStatus,
            Reason = request.Reason,
            ChangedByUserId = Guid.TryParse(request.UserId, out var uid) ? uid : null,
            CreatedBy = request.UserId
        };
        await _historyRepo.AddAsync(history, ct);

        // Actualizar estado
        config.Status = request.NewStatus;
        config.UpdatedAt = DateTime.UtcNow;
        config.UpdatedBy = request.UserId;

        // Resetear contador de errores si se activa
        if (request.NewStatus == IntegrationStatus.Active)
        {
            config.ConsecutiveErrors = 0;
        }

        await _configRepo.UpdateAsync(config, ct);
        return true;
    }
}

/// <summary>
/// Comando para eliminar (soft delete) integración
/// </summary>
public record DeleteIntegrationConfigCommand(Guid Id, string UserId) : IRequest<bool>;

public class DeleteIntegrationConfigHandler : IRequestHandler<DeleteIntegrationConfigCommand, bool>
{
    private readonly IIntegrationConfigRepository _repository;

    public DeleteIntegrationConfigHandler(IIntegrationConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteIntegrationConfigCommand request, CancellationToken ct)
    {
        var exists = await _repository.ExistsAsync(request.Id, ct);
        if (!exists) return false;

        await _repository.DeleteAsync(request.Id, ct);
        return true;
    }
}

#endregion

#region Credential Commands

/// <summary>
/// Comando para crear credencial
/// </summary>
public record CreateCredentialCommand(CreateCredentialDto Dto, string UserId) : IRequest<IntegrationCredentialDto>;

public class CreateCredentialHandler : IRequestHandler<CreateCredentialCommand, IntegrationCredentialDto>
{
    private readonly IIntegrationCredentialRepository _repository;

    public CreateCredentialHandler(IIntegrationCredentialRepository repository)
    {
        _repository = repository;
    }

    public async Task<IntegrationCredentialDto> Handle(CreateCredentialCommand request, CancellationToken ct)
    {
        var credential = new IntegrationCredential
        {
            IntegrationConfigId = request.Dto.IntegrationConfigId,
            Name = request.Dto.Name,
            CredentialType = request.Dto.CredentialType,
            Username = request.Dto.Username,
            PasswordHash = request.Dto.Password, // En producción: encriptar
            ApiKeyHash = request.Dto.ApiKey, // En producción: encriptar
            CertificateData = request.Dto.CertificateBase64, // En producción: encriptar
            ExpiresAt = request.Dto.ExpiresAt,
            IsPrimary = request.Dto.IsPrimary,
            Environment = request.Dto.Environment,
            Notes = request.Dto.Notes,
            CreatedBy = request.UserId
        };

        // Si es primary, desmarcar otras como primary
        if (credential.IsPrimary)
        {
            await _repository.SetAsPrimaryAsync(credential.Id, credential.IntegrationConfigId, ct);
        }

        var created = await _repository.AddAsync(credential, ct);

        return new IntegrationCredentialDto
        {
            Id = created.Id,
            IntegrationConfigId = created.IntegrationConfigId,
            Name = created.Name,
            CredentialType = created.CredentialType,
            CredentialTypeName = created.CredentialType.ToString(),
            Username = created.Username,
            HasPassword = !string.IsNullOrEmpty(created.PasswordHash),
            HasApiKey = !string.IsNullOrEmpty(created.ApiKeyHash),
            HasCertificate = !string.IsNullOrEmpty(created.CertificateData),
            CertificateThumbprint = created.CertificateThumbprint,
            ExpiresAt = created.ExpiresAt,
            IsExpired = created.ExpiresAt.HasValue && created.ExpiresAt < DateTime.UtcNow,
            IsPrimary = created.IsPrimary,
            Environment = created.Environment,
            CreatedAt = created.CreatedAt
        };
    }
}

/// <summary>
/// Comando para eliminar credencial
/// </summary>
public record DeleteCredentialCommand(Guid Id) : IRequest<bool>;

public class DeleteCredentialHandler : IRequestHandler<DeleteCredentialCommand, bool>
{
    private readonly IIntegrationCredentialRepository _repository;

    public DeleteCredentialHandler(IIntegrationCredentialRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteCredentialCommand request, CancellationToken ct)
    {
        var credential = await _repository.GetByIdAsync(request.Id, ct);
        if (credential == null) return false;

        await _repository.DeleteAsync(request.Id, ct);
        return true;
    }
}

#endregion

#region Transmission Commands

/// <summary>
/// Comando para crear transmisión
/// </summary>
public record CreateTransmissionCommand(CreateTransmissionDto Dto, string UserId) : IRequest<DataTransmissionDto>;

public class CreateTransmissionHandler : IRequestHandler<CreateTransmissionCommand, DataTransmissionDto>
{
    private readonly IDataTransmissionRepository _repository;
    private readonly IIntegrationConfigRepository _configRepo;

    public CreateTransmissionHandler(
        IDataTransmissionRepository repository,
        IIntegrationConfigRepository configRepo)
    {
        _repository = repository;
        _configRepo = configRepo;
    }

    public async Task<DataTransmissionDto> Handle(CreateTransmissionCommand request, CancellationToken ct)
    {
        var config = await _configRepo.GetByIdAsync(request.Dto.IntegrationConfigId, ct);
        
        var transmission = new DataTransmission
        {
            IntegrationConfigId = request.Dto.IntegrationConfigId,
            TransmissionCode = $"TX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            ReportType = request.Dto.ReportType,
            Direction = request.Dto.Direction,
            Status = TransmissionStatus.Pending,
            PeriodStart = request.Dto.PeriodStart,
            PeriodEnd = request.Dto.PeriodEnd,
            FileName = request.Dto.FileName,
            Notes = request.Dto.Notes,
            InitiatedByUserId = Guid.TryParse(request.UserId, out var uid) ? uid : null,
            CreatedBy = request.UserId
        };

        var created = await _repository.AddAsync(transmission, ct);

        return new DataTransmissionDto
        {
            Id = created.Id,
            IntegrationConfigId = created.IntegrationConfigId,
            IntegrationName = config?.Name ?? "Unknown",
            TransmissionCode = created.TransmissionCode,
            ReportType = created.ReportType,
            ReportTypeName = created.ReportType.ToString(),
            Direction = created.Direction,
            DirectionName = created.Direction.ToString(),
            Status = created.Status,
            StatusName = created.Status.ToString(),
            PeriodStart = created.PeriodStart,
            PeriodEnd = created.PeriodEnd,
            FileName = created.FileName,
            RecordCount = created.RecordCount,
            AttemptCount = created.AttemptCount,
            CreatedAt = created.CreatedAt
        };
    }
}

/// <summary>
/// Comando para actualizar estado de transmisión
/// </summary>
public record UpdateTransmissionStatusCommand(Guid Id, UpdateTransmissionStatusDto Dto, string UserId) : IRequest<bool>;

public class UpdateTransmissionStatusHandler : IRequestHandler<UpdateTransmissionStatusCommand, bool>
{
    private readonly IDataTransmissionRepository _repository;
    private readonly IIntegrationConfigRepository _configRepo;

    public UpdateTransmissionStatusHandler(
        IDataTransmissionRepository repository,
        IIntegrationConfigRepository configRepo)
    {
        _repository = repository;
        _configRepo = configRepo;
    }

    public async Task<bool> Handle(UpdateTransmissionStatusCommand request, CancellationToken ct)
    {
        var transmission = await _repository.GetByIdAsync(request.Id, ct);
        if (transmission == null) return false;

        var previousStatus = transmission.Status;
        transmission.Status = request.Dto.Status;
        transmission.ConfirmationNumber = request.Dto.ConfirmationNumber;
        transmission.ResponseData = request.Dto.ResponseData;
        transmission.ResponseCode = request.Dto.ResponseCode;
        transmission.ErrorMessage = request.Dto.ErrorMessage;
        transmission.ErrorDetails = request.Dto.ErrorDetails;
        transmission.RecordCount = request.Dto.RecordCount;
        transmission.FileSizeBytes = request.Dto.FileSizeBytes;
        transmission.FileHash = request.Dto.FileHash;
        transmission.UpdatedAt = DateTime.UtcNow;
        transmission.UpdatedBy = request.UserId;

        // Marcar tiempos según estado
        if (request.Dto.Status == TransmissionStatus.InProgress && !transmission.TransmissionStartedAt.HasValue)
        {
            transmission.TransmissionStartedAt = DateTime.UtcNow;
        }

        if (request.Dto.Status == TransmissionStatus.Success || request.Dto.Status == TransmissionStatus.Failed)
        {
            transmission.TransmissionCompletedAt = DateTime.UtcNow;

            // Actualizar estadísticas de la integración
            var config = await _configRepo.GetByIdAsync(transmission.IntegrationConfigId, ct);
            if (config != null)
            {
                if (request.Dto.Status == TransmissionStatus.Success)
                {
                    config.LastSuccessfulSync = DateTime.UtcNow;
                    config.ConsecutiveErrors = 0;
                }
                else
                {
                    config.LastFailedSync = DateTime.UtcNow;
                    config.ConsecutiveErrors++;
                }
                config.UpdatedAt = DateTime.UtcNow;
                await _configRepo.UpdateAsync(config, ct);
            }
        }

        await _repository.UpdateAsync(transmission, ct);
        return true;
    }
}

/// <summary>
/// Comando para reintentar transmisión fallida
/// </summary>
public record RetryTransmissionCommand(Guid Id, string UserId) : IRequest<DataTransmissionDto?>;

public class RetryTransmissionHandler : IRequestHandler<RetryTransmissionCommand, DataTransmissionDto?>
{
    private readonly IDataTransmissionRepository _repository;
    private readonly IIntegrationConfigRepository _configRepo;

    public RetryTransmissionHandler(
        IDataTransmissionRepository repository,
        IIntegrationConfigRepository configRepo)
    {
        _repository = repository;
        _configRepo = configRepo;
    }

    public async Task<DataTransmissionDto?> Handle(RetryTransmissionCommand request, CancellationToken ct)
    {
        var transmission = await _repository.GetByIdAsync(request.Id, ct);
        if (transmission == null) return null;

        if (transmission.Status != TransmissionStatus.Failed)
        {
            return null; // Solo se pueden reintentar transmisiones fallidas
        }

        var config = await _configRepo.GetByIdAsync(transmission.IntegrationConfigId, ct);
        if (config != null && transmission.AttemptCount >= config.MaxRetries)
        {
            return null; // Excedido máximo de reintentos
        }

        transmission.Status = TransmissionStatus.Retrying;
        transmission.AttemptCount++;
        transmission.ErrorMessage = null;
        transmission.ErrorDetails = null;
        transmission.NextRetryAt = null;
        transmission.UpdatedAt = DateTime.UtcNow;
        transmission.UpdatedBy = request.UserId;

        await _repository.UpdateAsync(transmission, ct);

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
            AttemptCount = transmission.AttemptCount,
            CreatedAt = transmission.CreatedAt
        };
    }
}

#endregion

#region Field Mapping Commands

/// <summary>
/// Comando para crear mapeo de campos
/// </summary>
public record CreateFieldMappingCommand(CreateFieldMappingDto Dto, string UserId) : IRequest<FieldMappingDto>;

public class CreateFieldMappingHandler : IRequestHandler<CreateFieldMappingCommand, FieldMappingDto>
{
    private readonly IFieldMappingRepository _repository;

    public CreateFieldMappingHandler(IFieldMappingRepository repository)
    {
        _repository = repository;
    }

    public async Task<FieldMappingDto> Handle(CreateFieldMappingCommand request, CancellationToken ct)
    {
        var mapping = new FieldMapping
        {
            IntegrationConfigId = request.Dto.IntegrationConfigId,
            ReportType = request.Dto.ReportType,
            SourceField = request.Dto.SourceField,
            TargetField = request.Dto.TargetField,
            SourceDataType = request.Dto.SourceDataType,
            TargetDataType = request.Dto.TargetDataType,
            Transformation = request.Dto.Transformation,
            DefaultValue = request.Dto.DefaultValue,
            IsRequired = request.Dto.IsRequired,
            MaxLength = request.Dto.MaxLength,
            ValidationPattern = request.Dto.ValidationPattern,
            FieldOrder = request.Dto.FieldOrder,
            Description = request.Dto.Description,
            CreatedBy = request.UserId
        };

        var created = await _repository.AddAsync(mapping, ct);

        return new FieldMappingDto
        {
            Id = created.Id,
            IntegrationConfigId = created.IntegrationConfigId,
            ReportType = created.ReportType,
            ReportTypeName = created.ReportType.ToString(),
            SourceField = created.SourceField,
            TargetField = created.TargetField,
            SourceDataType = created.SourceDataType,
            TargetDataType = created.TargetDataType,
            Transformation = created.Transformation,
            DefaultValue = created.DefaultValue,
            IsRequired = created.IsRequired,
            MaxLength = created.MaxLength,
            ValidationPattern = created.ValidationPattern,
            FieldOrder = created.FieldOrder,
            Description = created.Description
        };
    }
}

/// <summary>
/// Comando para eliminar mapeo de campos
/// </summary>
public record DeleteFieldMappingCommand(Guid Id) : IRequest<bool>;

public class DeleteFieldMappingHandler : IRequestHandler<DeleteFieldMappingCommand, bool>
{
    private readonly IFieldMappingRepository _repository;

    public DeleteFieldMappingHandler(IFieldMappingRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteFieldMappingCommand request, CancellationToken ct)
    {
        var mapping = await _repository.GetByIdAsync(request.Id, ct);
        if (mapping == null) return false;

        await _repository.DeleteAsync(request.Id, ct);
        return true;
    }
}

#endregion

#region Webhook Commands

/// <summary>
/// Comando para crear webhook
/// </summary>
public record CreateWebhookCommand(CreateWebhookDto Dto, string UserId) : IRequest<WebhookConfigDto>;

public class CreateWebhookHandler : IRequestHandler<CreateWebhookCommand, WebhookConfigDto>
{
    private readonly IWebhookConfigRepository _repository;

    public CreateWebhookHandler(IWebhookConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<WebhookConfigDto> Handle(CreateWebhookCommand request, CancellationToken ct)
    {
        var webhook = new WebhookConfig
        {
            IntegrationConfigId = request.Dto.IntegrationConfigId,
            Name = request.Dto.Name,
            WebhookUrl = request.Dto.WebhookUrl,
            SecretHash = Guid.NewGuid().ToString("N"), // Generar secret
            SubscribedEvents = request.Dto.SubscribedEvents,
            IsEnabled = request.Dto.IsEnabled,
            CreatedBy = request.UserId
        };

        var created = await _repository.AddAsync(webhook, ct);

        return new WebhookConfigDto
        {
            Id = created.Id,
            IntegrationConfigId = created.IntegrationConfigId,
            Name = created.Name,
            WebhookUrl = created.WebhookUrl,
            SubscribedEvents = created.SubscribedEvents,
            IsEnabled = created.IsEnabled,
            EventCount = 0,
            CreatedAt = created.CreatedAt
        };
    }
}

/// <summary>
/// Comando para toggle webhook
/// </summary>
public record ToggleWebhookCommand(Guid Id, bool IsEnabled, string UserId) : IRequest<bool>;

public class ToggleWebhookHandler : IRequestHandler<ToggleWebhookCommand, bool>
{
    private readonly IWebhookConfigRepository _repository;

    public ToggleWebhookHandler(IWebhookConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(ToggleWebhookCommand request, CancellationToken ct)
    {
        var webhook = await _repository.GetByIdAsync(request.Id, ct);
        if (webhook == null) return false;

        webhook.IsEnabled = request.IsEnabled;
        webhook.UpdatedAt = DateTime.UtcNow;
        webhook.UpdatedBy = request.UserId;

        await _repository.UpdateAsync(webhook, ct);
        return true;
    }
}

#endregion

#region Integration Log Commands

/// <summary>
/// Comando para registrar log de integración
/// </summary>
public record CreateIntegrationLogCommand(
    Guid IntegrationConfigId,
    Guid? DataTransmissionId,
    LogSeverity Severity,
    string Category,
    string Message,
    string? Details = null,
    string? RequestData = null,
    string? ResponseData = null,
    long? DurationMs = null,
    string? CorrelationId = null
) : IRequest<Guid>;

public class CreateIntegrationLogHandler : IRequestHandler<CreateIntegrationLogCommand, Guid>
{
    private readonly IIntegrationLogRepository _repository;

    public CreateIntegrationLogHandler(IIntegrationLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateIntegrationLogCommand request, CancellationToken ct)
    {
        var log = new IntegrationLog
        {
            IntegrationConfigId = request.IntegrationConfigId,
            DataTransmissionId = request.DataTransmissionId,
            Severity = request.Severity,
            Category = request.Category,
            Message = request.Message,
            Details = request.Details,
            RequestData = request.RequestData,
            ResponseData = request.ResponseData,
            DurationMs = request.DurationMs,
            CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString()
        };

        var created = await _repository.AddAsync(log, ct);
        return created.Id;
    }
}

/// <summary>
/// Comando para purgar logs antiguos
/// </summary>
public record PurgeOldLogsCommand(int RetentionDays) : IRequest<int>;

public class PurgeOldLogsHandler : IRequestHandler<PurgeOldLogsCommand, int>
{
    private readonly IIntegrationLogRepository _repository;

    public PurgeOldLogsHandler(IIntegrationLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(PurgeOldLogsCommand request, CancellationToken ct)
    {
        var beforeDate = DateTime.UtcNow.AddDays(-request.RetentionDays);
        await _repository.PurgeOldLogsAsync(beforeDate, ct);
        return 0; // Retornar cantidad de logs eliminados si el repositorio lo soporta
    }
}

#endregion

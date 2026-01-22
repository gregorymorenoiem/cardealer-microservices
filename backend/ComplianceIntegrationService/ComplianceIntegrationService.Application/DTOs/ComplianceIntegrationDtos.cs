// =====================================================
// C12: ComplianceIntegrationService - DTOs
// Data Transfer Objects para API
// =====================================================

using ComplianceIntegrationService.Domain.Enums;

namespace ComplianceIntegrationService.Application.DTOs;

#region Integration Config DTOs

/// <summary>
/// DTO para mostrar configuración de integración
/// </summary>
public record IntegrationConfigDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public RegulatoryBody RegulatoryBody { get; init; }
    public string RegulatoryBodyName { get; init; } = string.Empty;
    public IntegrationType IntegrationType { get; init; }
    public string IntegrationTypeName { get; init; } = string.Empty;
    public IntegrationStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public string? EndpointUrl { get; init; }
    public bool IsSandboxMode { get; init; }
    public SyncFrequency SyncFrequency { get; init; }
    public string SyncFrequencyName { get; init; } = string.Empty;
    public string? ScheduledTime { get; init; }
    public int TimeoutSeconds { get; init; }
    public int MaxRetries { get; init; }
    public DateTime? LastSuccessfulSync { get; init; }
    public DateTime? LastFailedSync { get; init; }
    public int ConsecutiveErrors { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsActive { get; init; }
    public List<IntegrationCredentialDto> Credentials { get; init; } = new();
    public int TransmissionCount { get; init; }
}

/// <summary>
/// DTO para crear configuración de integración
/// </summary>
public record CreateIntegrationConfigDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public RegulatoryBody RegulatoryBody { get; init; }
    public IntegrationType IntegrationType { get; init; }
    public string? EndpointUrl { get; init; }
    public string? SandboxUrl { get; init; }
    public int? Port { get; init; }
    public bool IsSandboxMode { get; init; } = true;
    public SyncFrequency SyncFrequency { get; init; } = SyncFrequency.Daily;
    public string? ScheduledTime { get; init; }
    public string? ScheduledDays { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
    public int MaxRetries { get; init; } = 3;
    public int RetryIntervalSeconds { get; init; } = 60;
    public bool RequiresSsl { get; init; } = true;
    public string? ProtocolVersion { get; init; }
    public string? AdditionalConfig { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO para actualizar configuración de integración
/// </summary>
public record UpdateIntegrationConfigDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? EndpointUrl { get; init; }
    public string? SandboxUrl { get; init; }
    public int? Port { get; init; }
    public bool IsSandboxMode { get; init; }
    public SyncFrequency SyncFrequency { get; init; }
    public string? ScheduledTime { get; init; }
    public string? ScheduledDays { get; init; }
    public int TimeoutSeconds { get; init; }
    public int MaxRetries { get; init; }
    public int RetryIntervalSeconds { get; init; }
    public bool RequiresSsl { get; init; }
    public string? ProtocolVersion { get; init; }
    public string? AdditionalConfig { get; init; }
    public string? Notes { get; init; }
}

#endregion

#region Credential DTOs

/// <summary>
/// DTO para mostrar credencial (sin datos sensibles)
/// </summary>
public record IntegrationCredentialDto
{
    public Guid Id { get; init; }
    public Guid IntegrationConfigId { get; init; }
    public string Name { get; init; } = string.Empty;
    public CredentialType CredentialType { get; init; }
    public string CredentialTypeName { get; init; } = string.Empty;
    public string? Username { get; init; }
    public bool HasPassword { get; init; }
    public bool HasApiKey { get; init; }
    public bool HasCertificate { get; init; }
    public string? CertificateThumbprint { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool IsExpired { get; init; }
    public bool IsPrimary { get; init; }
    public string Environment { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO para crear credencial
/// </summary>
public record CreateCredentialDto
{
    public Guid IntegrationConfigId { get; init; }
    public string Name { get; init; } = string.Empty;
    public CredentialType CredentialType { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? ApiKey { get; init; }
    public string? CertificateBase64 { get; init; }
    public string? CertificatePassword { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool IsPrimary { get; init; } = true;
    public string Environment { get; init; } = "Sandbox";
    public string? Notes { get; init; }
}

#endregion

#region Transmission DTOs

/// <summary>
/// DTO para mostrar transmisión de datos
/// </summary>
public record DataTransmissionDto
{
    public Guid Id { get; init; }
    public Guid IntegrationConfigId { get; init; }
    public string IntegrationName { get; init; } = string.Empty;
    public string TransmissionCode { get; init; } = string.Empty;
    public ReportType ReportType { get; init; }
    public string ReportTypeName { get; init; } = string.Empty;
    public DataDirection Direction { get; init; }
    public string DirectionName { get; init; } = string.Empty;
    public TransmissionStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public DateTime? PeriodStart { get; init; }
    public DateTime? PeriodEnd { get; init; }
    public string? FileName { get; init; }
    public long? FileSizeBytes { get; init; }
    public int RecordCount { get; init; }
    public DateTime? TransmissionStartedAt { get; init; }
    public DateTime? TransmissionCompletedAt { get; init; }
    public long? DurationMs { get; init; }
    public string? ConfirmationNumber { get; init; }
    public int? ResponseCode { get; init; }
    public string? ErrorMessage { get; init; }
    public int AttemptCount { get; init; }
    public DateTime? NextRetryAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO para crear transmisión de datos
/// </summary>
public record CreateTransmissionDto
{
    public Guid IntegrationConfigId { get; init; }
    public ReportType ReportType { get; init; }
    public DataDirection Direction { get; init; } = DataDirection.Outbound;
    public DateTime? PeriodStart { get; init; }
    public DateTime? PeriodEnd { get; init; }
    public string? FileName { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO para actualizar estado de transmisión
/// </summary>
public record UpdateTransmissionStatusDto
{
    public TransmissionStatus Status { get; init; }
    public string? ConfirmationNumber { get; init; }
    public string? ResponseData { get; init; }
    public int? ResponseCode { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorDetails { get; init; }
    public int RecordCount { get; init; }
    public long? FileSizeBytes { get; init; }
    public string? FileHash { get; init; }
}

#endregion

#region Field Mapping DTOs

/// <summary>
/// DTO para mapeo de campos
/// </summary>
public record FieldMappingDto
{
    public Guid Id { get; init; }
    public Guid IntegrationConfigId { get; init; }
    public ReportType ReportType { get; init; }
    public string ReportTypeName { get; init; } = string.Empty;
    public string SourceField { get; init; } = string.Empty;
    public string TargetField { get; init; } = string.Empty;
    public string? SourceDataType { get; init; }
    public string? TargetDataType { get; init; }
    public string? Transformation { get; init; }
    public string? DefaultValue { get; init; }
    public bool IsRequired { get; init; }
    public int? MaxLength { get; init; }
    public string? ValidationPattern { get; init; }
    public int FieldOrder { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// DTO para crear mapeo de campos
/// </summary>
public record CreateFieldMappingDto
{
    public Guid IntegrationConfigId { get; init; }
    public ReportType ReportType { get; init; }
    public string SourceField { get; init; } = string.Empty;
    public string TargetField { get; init; } = string.Empty;
    public string? SourceDataType { get; init; }
    public string? TargetDataType { get; init; }
    public string? Transformation { get; init; }
    public string? DefaultValue { get; init; }
    public bool IsRequired { get; init; }
    public int? MaxLength { get; init; }
    public string? ValidationPattern { get; init; }
    public int FieldOrder { get; init; }
    public string? Description { get; init; }
}

#endregion

#region Integration Log DTOs

/// <summary>
/// DTO para log de integración
/// </summary>
public record IntegrationLogDto
{
    public Guid Id { get; init; }
    public Guid IntegrationConfigId { get; init; }
    public Guid? DataTransmissionId { get; init; }
    public LogSeverity Severity { get; init; }
    public string SeverityName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Details { get; init; }
    public long? DurationMs { get; init; }
    public string? CorrelationId { get; init; }
    public DateTime CreatedAt { get; init; }
}

#endregion

#region Webhook DTOs

/// <summary>
/// DTO para configuración de webhook
/// </summary>
public record WebhookConfigDto
{
    public Guid Id { get; init; }
    public Guid IntegrationConfigId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string WebhookUrl { get; init; } = string.Empty;
    public string? SubscribedEvents { get; init; }
    public bool IsEnabled { get; init; }
    public DateTime? LastEventReceivedAt { get; init; }
    public int EventCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO para crear webhook
/// </summary>
public record CreateWebhookDto
{
    public Guid IntegrationConfigId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string WebhookUrl { get; init; } = string.Empty;
    public string? SubscribedEvents { get; init; }
    public bool IsEnabled { get; init; } = true;
}

#endregion

#region Statistics DTOs

/// <summary>
/// Estadísticas generales de integraciones
/// </summary>
public record IntegrationStatisticsDto
{
    public int TotalIntegrations { get; init; }
    public int ActiveIntegrations { get; init; }
    public int InErrorIntegrations { get; init; }
    public int TotalTransmissionsToday { get; init; }
    public int SuccessfulTransmissionsToday { get; init; }
    public int FailedTransmissionsToday { get; init; }
    public int PendingTransmissions { get; init; }
    public int ExpiringCredentials { get; init; }
    public int RecentErrors { get; init; }
    public Dictionary<string, int> TransmissionsByRegulatoryBody { get; init; } = new();
    public Dictionary<string, int> TransmissionsByStatus { get; init; } = new();
}

/// <summary>
/// Estadísticas de una integración específica
/// </summary>
public record IntegrationDetailStatisticsDto
{
    public Guid IntegrationId { get; init; }
    public string IntegrationName { get; init; } = string.Empty;
    public int TotalTransmissions { get; init; }
    public int SuccessfulTransmissions { get; init; }
    public int FailedTransmissions { get; init; }
    public double SuccessRate { get; init; }
    public long AverageDurationMs { get; init; }
    public DateTime? LastSuccessfulTransmission { get; init; }
    public DateTime? LastFailedTransmission { get; init; }
    public int TransmissionsLast24Hours { get; init; }
    public int TransmissionsLast7Days { get; init; }
    public int TransmissionsLast30Days { get; init; }
    public List<TransmissionsByDayDto> TransmissionsByDay { get; init; } = new();
}

/// <summary>
/// Transmisiones por día
/// </summary>
public record TransmissionsByDayDto
{
    public DateTime Date { get; init; }
    public int Total { get; init; }
    public int Successful { get; init; }
    public int Failed { get; init; }
}

#endregion

#region Health Check DTOs

/// <summary>
/// Estado de salud de una integración
/// </summary>
public record IntegrationHealthDto
{
    public Guid IntegrationId { get; init; }
    public string IntegrationName { get; init; } = string.Empty;
    public RegulatoryBody RegulatoryBody { get; init; }
    public string RegulatoryBodyName { get; init; } = string.Empty;
    public IntegrationStatus Status { get; init; }
    public bool IsHealthy { get; init; }
    public string? LastError { get; init; }
    public DateTime? LastCheckAt { get; init; }
    public DateTime? LastSuccessfulSync { get; init; }
    public int ConsecutiveErrors { get; init; }
    public bool CredentialsValid { get; init; }
    public bool EndpointReachable { get; init; }
    public long? LatencyMs { get; init; }
}

#endregion

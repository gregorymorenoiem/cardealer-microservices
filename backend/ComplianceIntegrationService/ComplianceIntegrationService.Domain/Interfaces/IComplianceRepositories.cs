// =====================================================
// C12: ComplianceIntegrationService - Interfaces de Repositorio
// Contratos para acceso a datos
// =====================================================

using ComplianceIntegrationService.Domain.Entities;
using ComplianceIntegrationService.Domain.Enums;

namespace ComplianceIntegrationService.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de configuraciones de integración
/// </summary>
public interface IIntegrationConfigRepository
{
    Task<IntegrationConfig?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IntegrationConfig?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<List<IntegrationConfig>> GetAllAsync(CancellationToken ct = default);
    Task<List<IntegrationConfig>> GetByRegulatoryBodyAsync(RegulatoryBody body, CancellationToken ct = default);
    Task<List<IntegrationConfig>> GetByStatusAsync(IntegrationStatus status, CancellationToken ct = default);
    Task<List<IntegrationConfig>> GetActiveIntegrationsAsync(CancellationToken ct = default);
    Task<List<IntegrationConfig>> GetDueForSyncAsync(DateTime asOf, CancellationToken ct = default);
    Task<IntegrationConfig> AddAsync(IntegrationConfig config, CancellationToken ct = default);
    Task UpdateAsync(IntegrationConfig config, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<int> CountByStatusAsync(IntegrationStatus status, CancellationToken ct = default);
}

/// <summary>
/// Repositorio para gestión de credenciales de integración
/// </summary>
public interface IIntegrationCredentialRepository
{
    Task<IntegrationCredential?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<IntegrationCredential>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken ct = default);
    Task<IntegrationCredential?> GetPrimaryByIntegrationIdAsync(Guid integrationId, CancellationToken ct = default);
    Task<List<IntegrationCredential>> GetExpiringCredentialsAsync(DateTime beforeDate, CancellationToken ct = default);
    Task<IntegrationCredential> AddAsync(IntegrationCredential credential, CancellationToken ct = default);
    Task UpdateAsync(IntegrationCredential credential, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SetAsPrimaryAsync(Guid id, Guid integrationId, CancellationToken ct = default);
}

/// <summary>
/// Repositorio para gestión de transmisiones de datos
/// </summary>
public interface IDataTransmissionRepository
{
    Task<DataTransmission?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DataTransmission?> GetByTransmissionCodeAsync(string code, CancellationToken ct = default);
    Task<List<DataTransmission>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken ct = default);
    Task<List<DataTransmission>> GetByStatusAsync(TransmissionStatus status, CancellationToken ct = default);
    Task<List<DataTransmission>> GetByReportTypeAsync(ReportType reportType, CancellationToken ct = default);
    Task<List<DataTransmission>> GetPendingRetriesAsync(DateTime asOf, CancellationToken ct = default);
    Task<List<DataTransmission>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<List<DataTransmission>> GetRecentByIntegrationAsync(Guid integrationId, int count, CancellationToken ct = default);
    Task<DataTransmission> AddAsync(DataTransmission transmission, CancellationToken ct = default);
    Task UpdateAsync(DataTransmission transmission, CancellationToken ct = default);
    Task<int> CountByStatusAsync(TransmissionStatus status, CancellationToken ct = default);
    Task<int> CountByIntegrationAndPeriodAsync(Guid integrationId, DateTime from, DateTime to, CancellationToken ct = default);
}

/// <summary>
/// Repositorio para gestión de mapeos de campos
/// </summary>
public interface IFieldMappingRepository
{
    Task<FieldMapping?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<FieldMapping>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken ct = default);
    Task<List<FieldMapping>> GetByReportTypeAsync(Guid integrationId, ReportType reportType, CancellationToken ct = default);
    Task<FieldMapping> AddAsync(FieldMapping mapping, CancellationToken ct = default);
    Task AddRangeAsync(List<FieldMapping> mappings, CancellationToken ct = default);
    Task UpdateAsync(FieldMapping mapping, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task DeleteByIntegrationAsync(Guid integrationId, CancellationToken ct = default);
}

/// <summary>
/// Repositorio para gestión de logs de integración
/// </summary>
public interface IIntegrationLogRepository
{
    Task<IntegrationLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<IntegrationLog>> GetByIntegrationIdAsync(Guid integrationId, int limit = 100, CancellationToken ct = default);
    Task<List<IntegrationLog>> GetByTransmissionIdAsync(Guid transmissionId, CancellationToken ct = default);
    Task<List<IntegrationLog>> GetBySeverityAsync(LogSeverity severity, DateTime? from = null, CancellationToken ct = default);
    Task<List<IntegrationLog>> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default);
    Task<List<IntegrationLog>> GetRecentErrorsAsync(int count, CancellationToken ct = default);
    Task<IntegrationLog> AddAsync(IntegrationLog log, CancellationToken ct = default);
    Task AddRangeAsync(List<IntegrationLog> logs, CancellationToken ct = default);
    Task<int> CountBySeverityAsync(LogSeverity severity, DateTime from, DateTime to, CancellationToken ct = default);
    Task PurgeOldLogsAsync(DateTime beforeDate, CancellationToken ct = default);
}

/// <summary>
/// Repositorio para gestión de webhooks
/// </summary>
public interface IWebhookConfigRepository
{
    Task<WebhookConfig?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<WebhookConfig>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken ct = default);
    Task<List<WebhookConfig>> GetActiveWebhooksAsync(CancellationToken ct = default);
    Task<WebhookConfig?> GetByUrlAsync(string url, CancellationToken ct = default);
    Task<WebhookConfig> AddAsync(WebhookConfig webhook, CancellationToken ct = default);
    Task UpdateAsync(WebhookConfig webhook, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task IncrementEventCountAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Repositorio para historial de estados
/// </summary>
public interface IIntegrationStatusHistoryRepository
{
    Task<List<IntegrationStatusHistory>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken ct = default);
    Task<IntegrationStatusHistory> AddAsync(IntegrationStatusHistory history, CancellationToken ct = default);
    Task<IntegrationStatusHistory?> GetLastChangeAsync(Guid integrationId, CancellationToken ct = default);
}

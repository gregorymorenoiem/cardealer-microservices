// =====================================================
// C12: ComplianceIntegrationService - Repositories
// Implementaciones de repositorios para acceso a datos
// =====================================================

using ComplianceIntegrationService.Domain.Entities;
using ComplianceIntegrationService.Domain.Enums;
using ComplianceIntegrationService.Domain.Interfaces;
using ComplianceIntegrationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComplianceIntegrationService.Infrastructure.Repositories;

#region Integration Config Repository

public class IntegrationConfigRepository : IIntegrationConfigRepository
{
    private readonly ComplianceIntegrationDbContext _context;

    public IntegrationConfigRepository(ComplianceIntegrationDbContext context)
    {
        _context = context;
    }

    public async Task<IntegrationConfig?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.IntegrationConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, ct);
    }

    public async Task<IntegrationConfig?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.IntegrationConfigs
            .Include(c => c.Credentials.Where(cr => cr.IsActive))
            .Include(c => c.Transmissions.Where(t => t.IsActive).OrderByDescending(t => t.CreatedAt).Take(10))
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, ct);
    }

    public async Task<List<IntegrationConfig>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.IntegrationConfigs
            .Where(c => c.IsActive)
            .OrderBy(c => c.RegulatoryBody)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<List<IntegrationConfig>> GetByRegulatoryBodyAsync(RegulatoryBody body, CancellationToken ct = default)
    {
        return await _context.IntegrationConfigs
            .Where(c => c.RegulatoryBody == body && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<List<IntegrationConfig>> GetByStatusAsync(IntegrationStatus status, CancellationToken ct = default)
    {
        return await _context.IntegrationConfigs
            .Where(c => c.Status == status && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<List<IntegrationConfig>> GetActiveIntegrationsAsync(CancellationToken ct = default)
    {
        return await _context.IntegrationConfigs
            .Where(c => c.Status == IntegrationStatus.Active && c.IsActive)
            .OrderBy(c => c.RegulatoryBody)
            .ToListAsync(ct);
    }

    public async Task<List<IntegrationConfig>> GetDueForSyncAsync(DateTime asOf, CancellationToken ct = default)
    {
        // Obtener integraciones activas que no se han sincronizado recientemente
        return await _context.IntegrationConfigs
            .Where(c => c.Status == IntegrationStatus.Active && c.IsActive)
            .Where(c => !c.LastSuccessfulSync.HasValue || c.LastSuccessfulSync < asOf.AddHours(-24))
            .ToListAsync(ct);
    }

    public async Task<IntegrationConfig> AddAsync(IntegrationConfig config, CancellationToken ct = default)
    {
        _context.IntegrationConfigs.Add(config);
        await _context.SaveChangesAsync(ct);
        return config;
    }

    public async Task UpdateAsync(IntegrationConfig config, CancellationToken ct = default)
    {
        _context.IntegrationConfigs.Update(config);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var config = await _context.IntegrationConfigs.FindAsync(new object[] { id }, ct);
        if (config != null)
        {
            config.IsActive = false;
            config.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.IntegrationConfigs.AnyAsync(c => c.Id == id && c.IsActive, ct);
    }

    public async Task<int> CountByStatusAsync(IntegrationStatus status, CancellationToken ct = default)
    {
        return await _context.IntegrationConfigs
            .CountAsync(c => c.Status == status && c.IsActive, ct);
    }
}

#endregion

#region Integration Credential Repository

public class IntegrationCredentialRepository : IIntegrationCredentialRepository
{
    private readonly ComplianceIntegrationDbContext _context;

    public IntegrationCredentialRepository(ComplianceIntegrationDbContext context)
    {
        _context = context;
    }

    public async Task<IntegrationCredential?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.IntegrationCredentials
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, ct);
    }

    public async Task<List<IntegrationCredential>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken ct = default)
    {
        return await _context.IntegrationCredentials
            .Where(c => c.IntegrationConfigId == integrationId && c.IsActive)
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<IntegrationCredential?> GetPrimaryByIntegrationIdAsync(Guid integrationId, CancellationToken ct = default)
    {
        return await _context.IntegrationCredentials
            .FirstOrDefaultAsync(c => c.IntegrationConfigId == integrationId && c.IsPrimary && c.IsActive, ct);
    }

    public async Task<List<IntegrationCredential>> GetExpiringCredentialsAsync(DateTime beforeDate, CancellationToken ct = default)
    {
        return await _context.IntegrationCredentials
            .Where(c => c.ExpiresAt.HasValue && c.ExpiresAt < beforeDate && c.IsActive)
            .Include(c => c.IntegrationConfig)
            .ToListAsync(ct);
    }

    public async Task<IntegrationCredential> AddAsync(IntegrationCredential credential, CancellationToken ct = default)
    {
        _context.IntegrationCredentials.Add(credential);
        await _context.SaveChangesAsync(ct);
        return credential;
    }

    public async Task UpdateAsync(IntegrationCredential credential, CancellationToken ct = default)
    {
        _context.IntegrationCredentials.Update(credential);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var credential = await _context.IntegrationCredentials.FindAsync(new object[] { id }, ct);
        if (credential != null)
        {
            credential.IsActive = false;
            credential.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task SetAsPrimaryAsync(Guid id, Guid integrationId, CancellationToken ct = default)
    {
        // Desmarcar todas las credenciales como no-primary
        var allCredentials = await _context.IntegrationCredentials
            .Where(c => c.IntegrationConfigId == integrationId && c.IsActive)
            .ToListAsync(ct);

        foreach (var cred in allCredentials)
        {
            cred.IsPrimary = cred.Id == id;
            cred.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Data Transmission Repository

public class DataTransmissionRepository : IDataTransmissionRepository
{
    private readonly ComplianceIntegrationDbContext _context;

    public DataTransmissionRepository(ComplianceIntegrationDbContext context)
    {
        _context = context;
    }

    public async Task<DataTransmission?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.DataTransmissions
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive, ct);
    }

    public async Task<DataTransmission?> GetByTransmissionCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.DataTransmissions
            .FirstOrDefaultAsync(t => t.TransmissionCode == code && t.IsActive, ct);
    }

    public async Task<List<DataTransmission>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken ct = default)
    {
        return await _context.DataTransmissions
            .Where(t => t.IntegrationConfigId == integrationId && t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<DataTransmission>> GetByStatusAsync(TransmissionStatus status, CancellationToken ct = default)
    {
        return await _context.DataTransmissions
            .Where(t => t.Status == status && t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<DataTransmission>> GetByReportTypeAsync(ReportType reportType, CancellationToken ct = default)
    {
        return await _context.DataTransmissions
            .Where(t => t.ReportType == reportType && t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<DataTransmission>> GetPendingRetriesAsync(DateTime asOf, CancellationToken ct = default)
    {
        return await _context.DataTransmissions
            .Where(t => t.Status == TransmissionStatus.Failed && t.IsActive)
            .Where(t => t.NextRetryAt.HasValue && t.NextRetryAt <= asOf)
            .Include(t => t.IntegrationConfig)
            .OrderBy(t => t.NextRetryAt)
            .ToListAsync(ct);
    }

    public async Task<List<DataTransmission>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _context.DataTransmissions
            .Where(t => t.CreatedAt >= from && t.CreatedAt < to && t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<DataTransmission>> GetRecentByIntegrationAsync(Guid integrationId, int count, CancellationToken ct = default)
    {
        return await _context.DataTransmissions
            .Where(t => t.IntegrationConfigId == integrationId && t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<DataTransmission> AddAsync(DataTransmission transmission, CancellationToken ct = default)
    {
        _context.DataTransmissions.Add(transmission);
        await _context.SaveChangesAsync(ct);
        return transmission;
    }

    public async Task UpdateAsync(DataTransmission transmission, CancellationToken ct = default)
    {
        _context.DataTransmissions.Update(transmission);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountByStatusAsync(TransmissionStatus status, CancellationToken ct = default)
    {
        return await _context.DataTransmissions
            .CountAsync(t => t.Status == status && t.IsActive, ct);
    }

    public async Task<int> CountByIntegrationAndPeriodAsync(Guid integrationId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _context.DataTransmissions
            .CountAsync(t => t.IntegrationConfigId == integrationId 
                          && t.CreatedAt >= from && t.CreatedAt < to 
                          && t.IsActive, ct);
    }
}

#endregion

#region Field Mapping Repository

public class FieldMappingRepository : IFieldMappingRepository
{
    private readonly ComplianceIntegrationDbContext _context;

    public FieldMappingRepository(ComplianceIntegrationDbContext context)
    {
        _context = context;
    }

    public async Task<FieldMapping?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.FieldMappings
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, ct);
    }

    public async Task<List<FieldMapping>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken ct = default)
    {
        return await _context.FieldMappings
            .Where(m => m.IntegrationConfigId == integrationId && m.IsActive)
            .OrderBy(m => m.ReportType)
            .ThenBy(m => m.FieldOrder)
            .ToListAsync(ct);
    }

    public async Task<List<FieldMapping>> GetByReportTypeAsync(Guid integrationId, ReportType reportType, CancellationToken ct = default)
    {
        return await _context.FieldMappings
            .Where(m => m.IntegrationConfigId == integrationId 
                     && m.ReportType == reportType 
                     && m.IsActive)
            .OrderBy(m => m.FieldOrder)
            .ToListAsync(ct);
    }

    public async Task<FieldMapping> AddAsync(FieldMapping mapping, CancellationToken ct = default)
    {
        _context.FieldMappings.Add(mapping);
        await _context.SaveChangesAsync(ct);
        return mapping;
    }

    public async Task AddRangeAsync(List<FieldMapping> mappings, CancellationToken ct = default)
    {
        _context.FieldMappings.AddRange(mappings);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(FieldMapping mapping, CancellationToken ct = default)
    {
        _context.FieldMappings.Update(mapping);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var mapping = await _context.FieldMappings.FindAsync(new object[] { id }, ct);
        if (mapping != null)
        {
            mapping.IsActive = false;
            mapping.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteByIntegrationAsync(Guid integrationId, CancellationToken ct = default)
    {
        var mappings = await _context.FieldMappings
            .Where(m => m.IntegrationConfigId == integrationId && m.IsActive)
            .ToListAsync(ct);

        foreach (var mapping in mappings)
        {
            mapping.IsActive = false;
            mapping.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Integration Log Repository

public class IntegrationLogRepository : IIntegrationLogRepository
{
    private readonly ComplianceIntegrationDbContext _context;

    public IntegrationLogRepository(ComplianceIntegrationDbContext context)
    {
        _context = context;
    }

    public async Task<IntegrationLog?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.IntegrationLogs.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<IntegrationLog>> GetByIntegrationIdAsync(Guid integrationId, int limit = 100, CancellationToken ct = default)
    {
        return await _context.IntegrationLogs
            .Where(l => l.IntegrationConfigId == integrationId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<List<IntegrationLog>> GetByTransmissionIdAsync(Guid transmissionId, CancellationToken ct = default)
    {
        return await _context.IntegrationLogs
            .Where(l => l.DataTransmissionId == transmissionId)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<IntegrationLog>> GetBySeverityAsync(LogSeverity severity, DateTime? from = null, CancellationToken ct = default)
    {
        var query = _context.IntegrationLogs.Where(l => l.Severity == severity);
        
        if (from.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= from.Value);
        }

        return await query.OrderByDescending(l => l.CreatedAt).Take(500).ToListAsync(ct);
    }

    public async Task<List<IntegrationLog>> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default)
    {
        return await _context.IntegrationLogs
            .Where(l => l.CorrelationId == correlationId)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<IntegrationLog>> GetRecentErrorsAsync(int count, CancellationToken ct = default)
    {
        return await _context.IntegrationLogs
            .Where(l => l.Severity >= LogSeverity.Error)
            .OrderByDescending(l => l.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<IntegrationLog> AddAsync(IntegrationLog log, CancellationToken ct = default)
    {
        _context.IntegrationLogs.Add(log);
        await _context.SaveChangesAsync(ct);
        return log;
    }

    public async Task AddRangeAsync(List<IntegrationLog> logs, CancellationToken ct = default)
    {
        _context.IntegrationLogs.AddRange(logs);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountBySeverityAsync(LogSeverity severity, DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _context.IntegrationLogs
            .CountAsync(l => l.Severity == severity && l.CreatedAt >= from && l.CreatedAt < to, ct);
    }

    public async Task PurgeOldLogsAsync(DateTime beforeDate, CancellationToken ct = default)
    {
        // Eliminar logs físicamente (no soft delete para logs)
        var oldLogs = await _context.IntegrationLogs
            .Where(l => l.CreatedAt < beforeDate && l.Severity < LogSeverity.Error)
            .ToListAsync(ct);

        _context.IntegrationLogs.RemoveRange(oldLogs);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Webhook Config Repository

public class WebhookConfigRepository : IWebhookConfigRepository
{
    private readonly ComplianceIntegrationDbContext _context;

    public WebhookConfigRepository(ComplianceIntegrationDbContext context)
    {
        _context = context;
    }

    public async Task<WebhookConfig?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.WebhookConfigs
            .FirstOrDefaultAsync(w => w.Id == id && w.IsActive, ct);
    }

    public async Task<List<WebhookConfig>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken ct = default)
    {
        return await _context.WebhookConfigs
            .Where(w => w.IntegrationConfigId == integrationId && w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(ct);
    }

    public async Task<List<WebhookConfig>> GetActiveWebhooksAsync(CancellationToken ct = default)
    {
        return await _context.WebhookConfigs
            .Where(w => w.IsEnabled && w.IsActive)
            .Include(w => w.IntegrationConfig)
            .ToListAsync(ct);
    }

    public async Task<WebhookConfig?> GetByUrlAsync(string url, CancellationToken ct = default)
    {
        return await _context.WebhookConfigs
            .FirstOrDefaultAsync(w => w.WebhookUrl == url && w.IsActive, ct);
    }

    public async Task<WebhookConfig> AddAsync(WebhookConfig webhook, CancellationToken ct = default)
    {
        _context.WebhookConfigs.Add(webhook);
        await _context.SaveChangesAsync(ct);
        return webhook;
    }

    public async Task UpdateAsync(WebhookConfig webhook, CancellationToken ct = default)
    {
        _context.WebhookConfigs.Update(webhook);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var webhook = await _context.WebhookConfigs.FindAsync(new object[] { id }, ct);
        if (webhook != null)
        {
            webhook.IsActive = false;
            webhook.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task IncrementEventCountAsync(Guid id, CancellationToken ct = default)
    {
        var webhook = await _context.WebhookConfigs.FindAsync(new object[] { id }, ct);
        if (webhook != null)
        {
            webhook.EventCount++;
            webhook.LastEventReceivedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }
}

#endregion

#region Integration Status History Repository

public class IntegrationStatusHistoryRepository : IIntegrationStatusHistoryRepository
{
    private readonly ComplianceIntegrationDbContext _context;

    public IntegrationStatusHistoryRepository(ComplianceIntegrationDbContext context)
    {
        _context = context;
    }

    public async Task<List<IntegrationStatusHistory>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken ct = default)
    {
        return await _context.IntegrationStatusHistories
            .Where(h => h.IntegrationConfigId == integrationId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IntegrationStatusHistory> AddAsync(IntegrationStatusHistory history, CancellationToken ct = default)
    {
        _context.IntegrationStatusHistories.Add(history);
        await _context.SaveChangesAsync(ct);
        return history;
    }

    public async Task<IntegrationStatusHistory?> GetLastChangeAsync(Guid integrationId, CancellationToken ct = default)
    {
        return await _context.IntegrationStatusHistories
            .Where(h => h.IntegrationConfigId == integrationId)
            .OrderByDescending(h => h.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }
}

#endregion

using BackupDRService.Core.Entities;

namespace BackupDRService.Core.Repositories;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(int id);
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId);
    Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType);
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count);
    Task<AuditLog> CreateAsync(AuditLog auditLog);
    Task DeleteOlderThanAsync(DateTime cutoffDate);
}

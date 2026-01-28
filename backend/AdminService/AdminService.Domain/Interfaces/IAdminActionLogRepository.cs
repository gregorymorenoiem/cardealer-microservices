using AdminService.Domain.Entities;

namespace AdminService.Domain.Interfaces;

/// <summary>
/// Interface para repositorio de action logs
/// </summary>
public interface IAdminActionLogRepository
{
    Task<IEnumerable<AdminActionLog>> GetRecentByAdminIdAsync(Guid adminId, int count);
    Task<IEnumerable<AdminActionLog>> GetRecentAsync(int count);
    Task<int> GetCountByAdminIdAsync(Guid adminId, DateTime from, DateTime to);
    Task<(IEnumerable<AdminActionLog> Items, int TotalCount)> GetAllAsync(
        Guid? adminId = null,
        string? action = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 20);
    Task LogAsync(AdminActionLog log);
}
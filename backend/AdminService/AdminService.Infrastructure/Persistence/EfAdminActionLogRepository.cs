using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AdminService.Infrastructure.Persistence;

/// <summary>
/// Implementation of IAdminActionLogRepository
/// WARNING: Uses in-memory storage — data is lost on pod restart.
/// TODO: Connect to ApplicationDbContext when database is configured.
/// Performance: Changed to ConcurrentQueue with max-size cap to prevent unbounded growth.
/// </summary>
public class EfAdminActionLogRepository : IAdminActionLogRepository
{
    private readonly ILogger<EfAdminActionLogRepository> _logger;
    private static readonly ConcurrentQueue<AdminActionLog> s_inMemoryLogs = new();
    private const int MaxLogSize = 5_000;

    public EfAdminActionLogRepository(ILogger<EfAdminActionLogRepository> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<AdminActionLog>> GetRecentByAdminIdAsync(Guid adminId, int count)
    {
        _logger.LogDebug("Getting recent logs for admin: {AdminId}, count: {Count}", adminId, count);
        await Task.CompletedTask;
        return s_inMemoryLogs.Where(l => l.AdminId == adminId).OrderByDescending(l => l.Timestamp).Take(count);
    }

    public async Task<IEnumerable<AdminActionLog>> GetRecentAsync(int count)
    {
        _logger.LogDebug("Getting recent logs, count: {Count}", count);
        await Task.CompletedTask;
        return s_inMemoryLogs.OrderByDescending(l => l.Timestamp).Take(count);
    }

    public async Task<int> GetCountByAdminIdAsync(Guid adminId, DateTime from, DateTime to)
    {
        _logger.LogDebug("Getting log count for admin: {AdminId}, from: {From}, to: {To}", adminId, from, to);
        await Task.CompletedTask;
        return s_inMemoryLogs.Count(l => l.AdminId == adminId && l.Timestamp >= from && l.Timestamp <= to);
    }

    public async Task<(IEnumerable<AdminActionLog> Items, int TotalCount)> GetAllAsync(
        Guid? adminId = null,
        string? action = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 20)
    {
        _logger.LogDebug("Getting all logs with filters");
        await Task.CompletedTask;

        var query = s_inMemoryLogs.AsEnumerable();

        if (adminId.HasValue)
            query = query.Where(l => l.AdminId == adminId.Value);
        if (!string.IsNullOrEmpty(action))
            query = query.Where(l => l.Action == action);
        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        var total = query.Count();
        var items = query.OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return (items, total);
    }

    public async Task LogAsync(AdminActionLog log)
    {
        _logger.LogDebug("Logging admin action: {Action}", log.Action);
        await Task.CompletedTask;

        // Evict oldest entries if at capacity
        while (s_inMemoryLogs.Count >= MaxLogSize)
            s_inMemoryLogs.TryDequeue(out _);

        s_inMemoryLogs.Enqueue(log);
    }
}

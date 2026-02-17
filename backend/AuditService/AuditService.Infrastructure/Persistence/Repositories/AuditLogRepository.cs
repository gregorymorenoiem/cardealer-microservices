using AuditService.Domain.Entities;
using AuditService.Domain.Interfaces.Repositories;
using AuditService.Infrastructure.Persistence;
using AuditService.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AuditService.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AuditDbContext _context;

    public AuditLogRepository(AuditDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.Where(x => x.UserId == userId);

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt <= toDate.Value);

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.Where(x => x.Action == action);

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt <= toDate.Value);

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByResourceAsync(string resource, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.Where(x => x.Resource == resource);

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt <= toDate.Value);

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(x => x.CreatedAt >= fromDate && x.CreatedAt <= toDate)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetBySeverityAsync(string severity, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        // Convert string severity to enum
        if (Enum.TryParse<AuditSeverity>(severity, true, out var severityEnum))
        {
            query = query.Where(x => x.Severity == severityEnum);
        }

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt <= toDate.Value);

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<AuditLog> items, int totalCount)> GetPaginatedAsync(
        string? userId = null,
        string? action = null,
        string? resource = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 50,
        string? sortBy = null,
        bool sortDescending = true,
        string? searchText = null,
        string? serviceName = null,
        string? severity = null,
        bool? success = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(x => x.UserId == userId);
        if (!string.IsNullOrEmpty(action))
            query = query.Where(x => x.Action == action);
        if (!string.IsNullOrEmpty(resource))
            query = query.Where(x => x.Resource == resource);
        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt <= toDate.Value);

        // Search text filter — searches across action, resource, userId, serviceName, errorMessage
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim().ToLower();
            query = query.Where(x =>
                x.Action.ToLower().Contains(term) ||
                x.Resource.ToLower().Contains(term) ||
                x.UserId.ToLower().Contains(term) ||
                x.ServiceName.ToLower().Contains(term) ||
                (x.ErrorMessage != null && x.ErrorMessage.ToLower().Contains(term)) ||
                (x.CorrelationId != null && x.CorrelationId.ToLower().Contains(term)));
        }

        // Service name filter
        if (!string.IsNullOrEmpty(serviceName))
            query = query.Where(x => x.ServiceName == serviceName);

        // Severity filter
        if (!string.IsNullOrEmpty(severity) && Enum.TryParse<AuditSeverity>(severity, true, out var severityEnum))
            query = query.Where(x => x.Severity == severityEnum);

        // Success filter
        if (success.HasValue)
            query = query.Where(x => x.Success == success.Value);

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortDescending
                ? query.OrderByDescending(GetSortProperty(sortBy))
                : query.OrderBy(GetSortProperty(sortBy));
        }
        else
        {
            query = query.OrderByDescending(x => x.CreatedAt);
        }

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.AddRange(auditLogs);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.Update(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.CountAsync(cancellationToken);
    }

    public async Task<AuditStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt <= toDate.Value);

        var totalLogs = await query.CountAsync();
        var successfulLogs = await query.CountAsync(x => x.Success);
        var failedLogs = totalLogs - successfulLogs;

        return new AuditStatistics
        {
            TotalLogs = totalLogs,
            SuccessfulLogs = successfulLogs,
            FailedLogs = failedLogs,
            FirstLogDate = await query.MinAsync(x => (DateTime?)x.CreatedAt),
            LastLogDate = await query.MaxAsync(x => (DateTime?)x.CreatedAt)
        };
    }

    public async Task<IEnumerable<ActionFrequency>> GetTopActionsAsync(int top = 10, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt <= toDate.Value);

        var topActions = await query
            .GroupBy(x => x.Action)
            .Select(g => new ActionFrequency
            {
                Action = g.Key,
                Count = g.Count(),
                SuccessCount = g.Count(x => x.Success),
                FailureCount = g.Count(x => !x.Success)
            })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync(cancellationToken);

        return topActions;
    }

    public async Task<IEnumerable<UserActivity>> GetTopUsersAsync(int top = 10, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt <= toDate.Value);

        var topUsers = await query
            .Where(x => x.UserId != "system")
            .GroupBy(x => x.UserId)
            .Select(g => new UserActivity
            {
                UserId = g.Key,
                TotalActions = g.Count(),
                SuccessfulActions = g.Count(x => x.Success),
                FailedActions = g.Count(x => !x.Success),
                FirstActivity = g.Min(x => x.CreatedAt),
                LastActivity = g.Max(x => x.CreatedAt)
            })
            .OrderByDescending(x => x.TotalActions)
            .Take(top)
            .ToListAsync(cancellationToken);

        return topUsers;
    }

    public async Task<int> DeleteOldLogsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        var logsToDelete = _context.AuditLogs.Where(x => x.CreatedAt < olderThan);
        var count = await logsToDelete.CountAsync(cancellationToken);
        _context.AuditLogs.RemoveRange(logsToDelete);
        await _context.SaveChangesAsync(cancellationToken);
        return count;
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AnyAsync(x => x.Id == id, cancellationToken);
    }

    private static Expression<Func<AuditLog, object>> GetSortProperty(string sortBy)
    {
        return sortBy?.ToLower() switch
        {
            "userid" => x => x.UserId,
            "action" => x => x.Action,
            "resource" => x => x.Resource,
            "serviceName" => x => x.ServiceName,
            "severity" => x => x.Severity,
            _ => x => x.CreatedAt
        };
    }
}
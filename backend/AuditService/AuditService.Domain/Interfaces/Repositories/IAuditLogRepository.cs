using AuditService.Domain.Entities;

namespace AuditService.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for audit log operations
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Gets an audit log by its unique identifier
    /// </summary>
    Task<AuditLog?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all audit logs for a specific user
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by action type
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by resource type
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByResourceAsync(string resource, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs within a specific date range
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by severity level
    /// </summary>
    Task<IEnumerable<AuditLog>> GetBySeverityAsync(string severity, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated audit logs with filtering and sorting
    /// </summary>
    Task<(IEnumerable<AuditLog> items, int totalCount)> GetPaginatedAsync(
        string? userId = null,
        string? action = null,
        string? resource = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 50,
        string? sortBy = null,
        bool sortDescending = true);

    /// <summary>
    /// Adds a new audit log entry
    /// </summary>
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple audit log entries in a batch
    /// </summary>
    Task AddRangeAsync(IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing audit log entry
    /// </summary>
    Task UpdateAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of audit logs
    /// </summary>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics about audit logs
    /// </summary>
    Task<AuditStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most frequent actions
    /// </summary>
    Task<IEnumerable<ActionFrequency>> GetTopActionsAsync(int top = 10, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most active users
    /// </summary>
    Task<IEnumerable<UserActivity>> GetTopUsersAsync(int top = 10, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes audit logs older than the specified date
    /// </summary>
    Task<int> DeleteOldLogsAsync(DateTime olderThan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an audit log exists with the given ID
    /// </summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics about audit logs
/// </summary>
public class AuditStatistics
{
    public int TotalLogs { get; set; }
    public int SuccessfulLogs { get; set; }
    public int FailedLogs { get; set; }
    public int SystemLogs { get; set; }
    public int UserLogs { get; set; }
    public int AnonymousLogs { get; set; }
    public double SuccessRate => TotalLogs > 0 ? (SuccessfulLogs * 100.0) / TotalLogs : 0;
    public DateTime? FirstLogDate { get; set; }
    public DateTime? LastLogDate { get; set; }
    public Dictionary<string, int> LogsBySeverity { get; set; } = new();
    public Dictionary<string, int> LogsByService { get; set; } = new();
    public Dictionary<string, int> LogsByAction { get; set; } = new();
}

/// <summary>
/// Action frequency statistics
/// </summary>
public class ActionFrequency
{
    public string Action { get; set; } = string.Empty;
    public int Count { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double SuccessRate => Count > 0 ? (SuccessCount * 100.0) / Count : 0;
}

/// <summary>
/// User activity statistics
/// </summary>
public class UserActivity
{
    public string UserId { get; set; } = string.Empty;
    public int TotalActions { get; set; }
    public int SuccessfulActions { get; set; }
    public int FailedActions { get; set; }
    public DateTime FirstActivity { get; set; }
    public DateTime LastActivity { get; set; }
    public double SuccessRate => TotalActions > 0 ? (SuccessfulActions * 100.0) / TotalActions : 0;
    public List<string> MostFrequentActions { get; set; } = new();
}
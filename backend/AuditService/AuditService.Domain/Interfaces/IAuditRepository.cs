using AuditService.Domain.Entities;

namespace AuditService.Domain.Interfaces;

/// <summary>
/// Repository interface for persisting and querying audit events.
/// </summary>
public interface IAuditRepository
{
    /// <summary>
    /// Persists an audit event to the database.
    /// </summary>
    /// <param name="auditEvent">The audit event to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    Task SaveAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit events by event type.
    /// </summary>
    /// <param name="eventType">The event type to filter by (routing key).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching audit events.</returns>
    Task<IEnumerable<AuditEvent>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit events by source microservice.
    /// </summary>
    /// <param name="source">The source microservice name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching audit events.</returns>
    Task<IEnumerable<AuditEvent>> GetBySourceAsync(string source, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit events within a time range.
    /// </summary>
    /// <param name="startDate">Start date (inclusive).</param>
    /// <param name="endDate">End date (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of audit events in the time range.</returns>
    Task<IEnumerable<AuditEvent>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

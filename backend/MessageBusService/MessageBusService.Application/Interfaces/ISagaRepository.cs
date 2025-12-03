using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Interfaces;

/// <summary>
/// Interface for Saga persistence
/// </summary>
public interface ISagaRepository
{
    /// <summary>
    /// Creates a new saga
    /// </summary>
    Task<Saga> CreateAsync(Saga saga, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing saga
    /// </summary>
    Task<Saga> UpdateAsync(Saga saga, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets saga by ID with all steps
    /// </summary>
    Task<Saga?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets saga by correlation ID
    /// </summary>
    Task<Saga?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sagas with a specific status
    /// </summary>
    Task<List<Saga>> GetByStatusAsync(Domain.Enums.SagaStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sagas that have timed out
    /// </summary>
    Task<List<Saga>> GetTimedOutSagasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a saga
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

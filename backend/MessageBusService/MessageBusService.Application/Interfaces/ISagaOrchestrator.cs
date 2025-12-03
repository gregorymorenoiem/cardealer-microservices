using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Interfaces;

/// <summary>
/// Interface for Saga orchestration
/// </summary>
public interface ISagaOrchestrator
{
    /// <summary>
    /// Starts a saga execution
    /// </summary>
    Task<Saga> StartSagaAsync(Saga saga, CancellationToken cancellationToken = default);

    /// <summary>
    /// Continues saga execution (processes next step)
    /// </summary>
    Task<Saga> ContinueSagaAsync(Guid sagaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compensates a failed saga (rollback)
    /// </summary>
    Task<Saga> CompensateSagaAsync(Guid sagaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aborts a saga execution
    /// </summary>
    Task<Saga> AbortSagaAsync(Guid sagaId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets saga status
    /// </summary>
    Task<Saga?> GetSagaAsync(Guid sagaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries a failed saga step
    /// </summary>
    Task<Saga> RetryStepAsync(Guid sagaId, Guid stepId, CancellationToken cancellationToken = default);
}

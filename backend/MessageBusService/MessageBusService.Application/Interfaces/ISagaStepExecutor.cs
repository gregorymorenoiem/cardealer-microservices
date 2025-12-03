using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Interfaces;

/// <summary>
/// Interface for executing saga step actions
/// </summary>
public interface ISagaStepExecutor
{
    /// <summary>
    /// Executes a saga step action
    /// </summary>
    Task<string?> ExecuteAsync(SagaStep step, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes compensation for a saga step
    /// </summary>
    Task CompensateAsync(SagaStep step, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if executor can handle the action type
    /// </summary>
    bool CanHandle(string actionType);
}

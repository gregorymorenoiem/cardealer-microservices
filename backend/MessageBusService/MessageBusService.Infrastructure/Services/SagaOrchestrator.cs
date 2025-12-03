using Microsoft.Extensions.Logging;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;
using MessageBusService.Domain.Enums;

namespace MessageBusService.Infrastructure.Services;

public class SagaOrchestrator : ISagaOrchestrator
{
    private readonly ISagaRepository _sagaRepository;
    private readonly IEnumerable<ISagaStepExecutor> _stepExecutors;
    private readonly ILogger<SagaOrchestrator> _logger;

    public SagaOrchestrator(
        ISagaRepository sagaRepository,
        IEnumerable<ISagaStepExecutor> stepExecutors,
        ILogger<SagaOrchestrator> logger)
    {
        _sagaRepository = sagaRepository;
        _stepExecutors = stepExecutors;
        _logger = logger;
    }

    public async Task<Saga> StartSagaAsync(Saga saga, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting saga {SagaId} - {SagaName}", saga.Id, saga.Name);

        try
        {
            saga.Start();
            await _sagaRepository.UpdateAsync(saga, cancellationToken);

            // Execute first step
            return await ExecuteNextStepAsync(saga, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting saga {SagaId}", saga.Id);
            saga.Fail($"Failed to start saga: {ex.Message}");
            await _sagaRepository.UpdateAsync(saga, cancellationToken);
            throw;
        }
    }

    public async Task<Saga> ContinueSagaAsync(Guid sagaId, CancellationToken cancellationToken = default)
    {
        var saga = await _sagaRepository.GetByIdAsync(sagaId, cancellationToken);
        if (saga == null)
        {
            throw new InvalidOperationException($"Saga {sagaId} not found");
        }

        return await ExecuteNextStepAsync(saga, cancellationToken);
    }

    public async Task<Saga> CompensateSagaAsync(Guid sagaId, CancellationToken cancellationToken = default)
    {
        var saga = await _sagaRepository.GetByIdAsync(sagaId, cancellationToken);
        if (saga == null)
        {
            throw new InvalidOperationException($"Saga {sagaId} not found");
        }

        _logger.LogWarning("Compensating saga {SagaId} - {SagaName}", saga.Id, saga.Name);

        saga.Status = SagaStatus.Compensating;
        await _sagaRepository.UpdateAsync(saga, cancellationToken);

        // Get completed steps in reverse order
        var stepsToCompensate = saga.GetStepsToCompensate();

        foreach (var step in stepsToCompensate)
        {
            if (!step.HasCompensation())
            {
                _logger.LogWarning("Step {StepId} has no compensation action", step.Id);
                continue;
            }

            try
            {
                await CompensateStepAsync(step, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compensating step {StepId} in saga {SagaId}", step.Id, saga.Id);
                step.FailCompensation($"Compensation failed: {ex.Message}");
                await _sagaRepository.UpdateAsync(saga, cancellationToken);

                saga.Status = SagaStatus.Failed;
                saga.ErrorMessage = $"Compensation failed at step {step.Name}: {ex.Message}";
                await _sagaRepository.UpdateAsync(saga, cancellationToken);
                return saga;
            }
        }

        saga.Compensate();
        await _sagaRepository.UpdateAsync(saga, cancellationToken);

        _logger.LogInformation("Saga {SagaId} compensated successfully", saga.Id);
        return saga;
    }

    public async Task<Saga> AbortSagaAsync(Guid sagaId, string reason, CancellationToken cancellationToken = default)
    {
        var saga = await _sagaRepository.GetByIdAsync(sagaId, cancellationToken);
        if (saga == null)
        {
            throw new InvalidOperationException($"Saga {sagaId} not found");
        }

        _logger.LogWarning("Aborting saga {SagaId} - Reason: {Reason}", saga.Id, reason);

        saga.Status = SagaStatus.Aborted;
        saga.ErrorMessage = $"Aborted: {reason}";
        await _sagaRepository.UpdateAsync(saga, cancellationToken);

        return saga;
    }

    public async Task<Saga?> GetSagaAsync(Guid sagaId, CancellationToken cancellationToken = default)
    {
        return await _sagaRepository.GetByIdAsync(sagaId, cancellationToken);
    }

    public async Task<Saga> RetryStepAsync(Guid sagaId, Guid stepId, CancellationToken cancellationToken = default)
    {
        var saga = await _sagaRepository.GetByIdAsync(sagaId, cancellationToken);
        if (saga == null)
        {
            throw new InvalidOperationException($"Saga {sagaId} not found");
        }

        var step = saga.Steps.FirstOrDefault(s => s.Id == stepId);
        if (step == null)
        {
            throw new InvalidOperationException($"Step {stepId} not found in saga {sagaId}");
        }

        if (!step.CanRetry())
        {
            throw new InvalidOperationException($"Step {stepId} has exceeded max retry attempts");
        }

        _logger.LogInformation("Retrying step {StepId} in saga {SagaId}", stepId, sagaId);

        step.IncrementRetry();
        step.Status = SagaStepStatus.Pending;
        step.ErrorMessage = null;
        await _sagaRepository.UpdateAsync(saga, cancellationToken);

        return await ExecuteNextStepAsync(saga, cancellationToken);
    }

    private async Task<Saga> ExecuteNextStepAsync(Saga saga, CancellationToken cancellationToken)
    {
        // Check timeout
        if (saga.HasTimedOut())
        {
            _logger.LogWarning("Saga {SagaId} has timed out", saga.Id);
            saga.Fail("Saga execution timed out");
            await _sagaRepository.UpdateAsync(saga, cancellationToken);
            return await CompensateSagaAsync(saga.Id, cancellationToken);
        }

        var nextStep = saga.GetNextStep();
        if (nextStep == null)
        {
            // All steps completed
            saga.Complete();
            await _sagaRepository.UpdateAsync(saga, cancellationToken);
            _logger.LogInformation("Saga {SagaId} completed successfully", saga.Id);
            return saga;
        }

        try
        {
            await ExecuteStepAsync(nextStep, cancellationToken);
            saga.CurrentStepIndex = nextStep.Order + 1;
            await _sagaRepository.UpdateAsync(saga, cancellationToken);

            // Continue with next step (recursive execution)
            return await ExecuteNextStepAsync(saga, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing step {StepId} in saga {SagaId}", nextStep.Id, saga.Id);
            nextStep.Fail($"Step execution failed: {ex.Message}");
            saga.Fail($"Step {nextStep.Name} failed: {ex.Message}");
            await _sagaRepository.UpdateAsync(saga, cancellationToken);

            // Trigger compensation
            return await CompensateSagaAsync(saga.Id, cancellationToken);
        }
    }

    private async Task ExecuteStepAsync(SagaStep step, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing step {StepId} - {StepName} ({ActionType})",
            step.Id, step.Name, step.ActionType);

        var executor = _stepExecutors.FirstOrDefault(e => e.CanHandle(step.ActionType));
        if (executor == null)
        {
            throw new InvalidOperationException($"No executor found for action type: {step.ActionType}");
        }

        step.Start();

        var responsePayload = await executor.ExecuteAsync(step, cancellationToken);

        step.Complete(responsePayload);
        _logger.LogInformation("Step {StepId} completed successfully", step.Id);
    }

    private async Task CompensateStepAsync(SagaStep step, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Compensating step {StepId} - {StepName}", step.Id, step.Name);

        if (string.IsNullOrEmpty(step.CompensationActionType))
        {
            throw new InvalidOperationException($"Step {step.Id} has no compensation action defined");
        }

        var executor = _stepExecutors.FirstOrDefault(e => e.CanHandle(step.CompensationActionType));
        if (executor == null)
        {
            throw new InvalidOperationException($"No executor found for compensation type: {step.CompensationActionType}");
        }

        step.StartCompensation();

        await executor.CompensateAsync(step, cancellationToken);

        step.CompleteCompensation();
        _logger.LogInformation("Step {StepId} compensated successfully", step.Id);
    }
}

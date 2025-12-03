using Microsoft.Extensions.Logging;
using SchedulerService.Application.Interfaces;
using SchedulerService.Domain.Entities;
using SchedulerService.Domain.Enums;
using SchedulerService.Domain.Interfaces;
using SchedulerService.Domain.Models;
using System.Diagnostics;

namespace SchedulerService.Infrastructure.Services;

/// <summary>
/// Core job execution engine that orchestrates job execution
/// </summary>
public class JobExecutionEngine
{
    private readonly ILogger<JobExecutionEngine> _logger;
    private readonly IJobRepository _jobRepository;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly IEnumerable<IJobExecutor> _executors;
    private readonly SemaphoreSlim _semaphore;
    private readonly int _maxConcurrentJobs;

    public JobExecutionEngine(
        ILogger<JobExecutionEngine> logger,
        IJobRepository jobRepository,
        IJobExecutionRepository executionRepository,
        IEnumerable<IJobExecutor> executors,
        int maxConcurrentJobs = 10)
    {
        _logger = logger;
        _jobRepository = jobRepository;
        _executionRepository = executionRepository;
        _executors = executors;
        _maxConcurrentJobs = maxConcurrentJobs;
        _semaphore = new SemaphoreSlim(maxConcurrentJobs);
    }

    /// <summary>
    /// Executes a job by its ID
    /// </summary>
    public async Task<ExecutionResult> ExecuteJobAsync(Guid jobId, Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        // Acquire semaphore to limit concurrent executions
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            // Load job
            var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
            if (job == null)
            {
                _logger.LogError("Job {JobId} not found", jobId);
                return ExecutionResult.FailureResult($"Job {jobId} not found");
            }

            // Check if job is executable
            if (!job.IsExecutable())
            {
                _logger.LogWarning("Job {JobId} is not executable. Status: {Status}", jobId, job.Status);
                return ExecutionResult.FailureResult($"Job {jobId} is not executable");
            }

            // Merge parameters
            var mergedParameters = new Dictionary<string, string>(job.Parameters);
            foreach (var param in parameters)
            {
                mergedParameters[param.Key] = param.Value;
            }

            // Create execution record
            var execution = new JobExecution
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                ScheduledAt = DateTime.UtcNow,
                Status = ExecutionStatus.Scheduled,
                AttemptNumber = 1
            };

            execution = await _executionRepository.CreateAsync(execution, cancellationToken);
            _logger.LogInformation("Created execution {ExecutionId} for job {JobId}", execution.Id, jobId);

            // Execute with retry logic
            var result = await ExecuteWithRetryAsync(job, execution, mergedParameters, cancellationToken);

            // Update job last execution time
            job.UpdateLastExecution(DateTime.UtcNow);
            await _jobRepository.UpdateAsync(job, cancellationToken);

            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Executes a job with retry logic
    /// </summary>
    private async Task<ExecutionResult> ExecuteWithRetryAsync(
        Job job,
        JobExecution execution,
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        var attempt = 0;
        var maxRetries = job.RetryCount;

        while (attempt <= maxRetries)
        {
            attempt++;
            execution.AttemptNumber = attempt;

            _logger.LogInformation(
                "Executing job {JobId} (execution {ExecutionId}), attempt {Attempt}/{MaxAttempts}",
                job.Id, execution.Id, attempt, maxRetries + 1);

            try
            {
                // Mark as running
                execution.MarkAsRunning(Environment.MachineName);
                await _executionRepository.UpdateAsync(execution, cancellationToken);

                // Find appropriate executor
                var executor = FindExecutor(job);
                if (executor == null)
                {
                    var error = $"No executor found for job type '{job.JobType}'";
                    _logger.LogError(error);
                    execution.MarkAsFailed(error);
                    await _executionRepository.UpdateAsync(execution, cancellationToken);
                    return ExecutionResult.FailureResult(error);
                }

                _logger.LogInformation("Using executor: {ExecutorType}", executor.ExecutorType);

                // Execute with timeout
                var result = await ExecuteWithTimeoutAsync(executor, job, execution, cancellationToken);

                if (result.Success)
                {
                    // Success - update execution and return
                    execution.MarkAsSucceeded(result.ResultData);
                    await _executionRepository.UpdateAsync(execution, cancellationToken);

                    _logger.LogInformation(
                        "Job {JobId} (execution {ExecutionId}) completed successfully in {DurationMs}ms",
                        job.Id, execution.Id, execution.DurationMs);

                    return result;
                }
                else
                {
                    // Failure - check if we should retry
                    if (attempt > maxRetries)
                    {
                        // Max retries reached
                        execution.MarkAsFailed(result.ErrorMessage ?? "Unknown error", result.StackTrace);
                        await _executionRepository.UpdateAsync(execution, cancellationToken);

                        _logger.LogError(
                            "Job {JobId} (execution {ExecutionId}) failed after {Attempts} attempts. Error: {Error}",
                            job.Id, execution.Id, attempt, result.ErrorMessage);

                        return result;
                    }
                    else
                    {
                        // Will retry
                        execution.MarkAsRetrying();
                        await _executionRepository.UpdateAsync(execution, cancellationToken);

                        _logger.LogWarning(
                            "Job {JobId} (execution {ExecutionId}) failed on attempt {Attempt}. Retrying... Error: {Error}",
                            job.Id, execution.Id, attempt, result.ErrorMessage);

                        // Exponential backoff
                        var delaySeconds = Math.Pow(2, attempt - 1);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Job {JobId} (execution {ExecutionId}) was cancelled", job.Id, execution.Id);
                execution.MarkAsCancelled();
                await _executionRepository.UpdateAsync(execution, cancellationToken);
                return ExecutionResult.FailureResult("Job execution was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error executing job {JobId} (execution {ExecutionId})", job.Id, execution.Id);

                if (attempt > maxRetries)
                {
                    execution.MarkAsFailed(ex.Message, ex.StackTrace);
                    await _executionRepository.UpdateAsync(execution, cancellationToken);
                    return ExecutionResult.FailureResult(ex.Message, ex.StackTrace);
                }
                else
                {
                    execution.MarkAsRetrying();
                    await _executionRepository.UpdateAsync(execution, cancellationToken);

                    // Exponential backoff
                    var delaySeconds = Math.Pow(2, attempt - 1);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                }
            }
        }

        // Should not reach here
        return ExecutionResult.FailureResult("Max retries exceeded");
    }

    /// <summary>
    /// Executes a job with timeout
    /// </summary>
    private async Task<ExecutionResult> ExecuteWithTimeoutAsync(
        IJobExecutor executor,
        Job job,
        JobExecution execution,
        CancellationToken cancellationToken)
    {
        var timeoutSeconds = job.TimeoutSeconds > 0 ? job.TimeoutSeconds : 300; // Default 5 minutes
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await executor.ExecuteAsync(job, execution, timeoutCts.Token);
            stopwatch.Stop();

            _logger.LogInformation(
                "Job {JobId} execution completed in {ElapsedMs}ms",
                job.Id, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("Job {JobId} execution timed out after {TimeoutSeconds} seconds", job.Id, timeoutSeconds);
            return ExecutionResult.FailureResult($"Job execution timed out after {timeoutSeconds} seconds");
        }
    }

    /// <summary>
    /// Finds the appropriate executor for a job
    /// </summary>
    private IJobExecutor? FindExecutor(Job job)
    {
        foreach (var executor in _executors)
        {
            if (executor.CanExecute(job))
            {
                return executor;
            }
        }

        return null;
    }
}

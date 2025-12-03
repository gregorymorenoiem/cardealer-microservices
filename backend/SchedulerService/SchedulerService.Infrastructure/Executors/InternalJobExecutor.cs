using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SchedulerService.Domain.Entities;
using SchedulerService.Domain.Interfaces;
using SchedulerService.Domain.Models;
using SchedulerService.Infrastructure.Jobs;
using System.Text.Json;

namespace SchedulerService.Infrastructure.Executors;

/// <summary>
/// Executor for internal IScheduledJob implementations
/// </summary>
public class InternalJobExecutor : IJobExecutor
{
    private readonly ILogger<InternalJobExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;

    public string ExecutorType => "Internal";

    public InternalJobExecutor(
        ILogger<InternalJobExecutor> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public bool CanExecute(Job job)
    {
        // Check if JobType is a fully qualified type name that implements IScheduledJob
        if (string.IsNullOrWhiteSpace(job.JobType))
            return false;

        try
        {
            var type = Type.GetType(job.JobType);
            if (type == null)
                return false;

            return typeof(IScheduledJob).IsAssignableFrom(type);
        }
        catch
        {
            return false;
        }
    }

    public async Task<ExecutionResult> ExecuteAsync(Job job, JobExecution execution, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing internal job: {JobType}", job.JobType);

            // Load job type
            var jobType = Type.GetType(job.JobType);
            if (jobType == null)
            {
                return ExecutionResult.FailureResult($"Job type '{job.JobType}' not found");
            }

            // Resolve job instance from DI
            using var scope = _serviceProvider.CreateScope();
            var jobInstance = scope.ServiceProvider.GetService(jobType) as IScheduledJob;

            if (jobInstance == null)
            {
                return ExecutionResult.FailureResult($"Failed to resolve job type '{job.JobType}' from DI container");
            }

            // Execute job
            await jobInstance.ExecuteAsync(job.Parameters);

            _logger.LogInformation("Internal job {JobType} completed successfully", job.JobType);

            return ExecutionResult.SuccessResult($"Job {job.Name} executed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing internal job {JobType}", job.JobType);
            return ExecutionResult.FailureResult(ex.Message, ex.StackTrace);
        }
    }
}

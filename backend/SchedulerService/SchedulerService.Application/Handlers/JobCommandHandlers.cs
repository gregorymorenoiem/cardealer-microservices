using MediatR;
using SchedulerService.Application.Commands;
using SchedulerService.Application.Interfaces;
using SchedulerService.Domain.Entities;
using SchedulerService.Domain.Enums;
using SchedulerService.Domain.Interfaces;

namespace SchedulerService.Application.Handlers;

public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, Job>
{
    private readonly IJobRepository _repository;
    private readonly IJobScheduler _scheduler;

    public CreateJobCommandHandler(IJobRepository repository, IJobScheduler scheduler)
    {
        _repository = repository;
        _scheduler = scheduler;
    }

    public async Task<Job> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            JobType = request.JobType,
            CronExpression = request.CronExpression,
            RetryCount = request.RetryCount,
            TimeoutSeconds = request.TimeoutSeconds,
            Parameters = request.Parameters,
            Status = JobStatus.Enabled,
            CreatedAt = DateTime.UtcNow
        };

        var createdJob = await _repository.CreateAsync(job, cancellationToken);

        // Schedule in Hangfire
        _scheduler.ScheduleRecurringJob(createdJob);

        return createdJob;
    }
}

public class UpdateJobCommandHandler : IRequestHandler<UpdateJobCommand, Job>
{
    private readonly IJobRepository _repository;
    private readonly IJobScheduler _scheduler;

    public UpdateJobCommandHandler(IJobRepository repository, IJobScheduler scheduler)
    {
        _repository = repository;
        _scheduler = scheduler;
    }

    public async Task<Job> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Job with ID {request.Id} not found");

        if (request.Name != null) job.Name = request.Name;
        if (request.Description != null) job.Description = request.Description;
        if (request.CronExpression != null) job.CronExpression = request.CronExpression;
        if (request.RetryCount.HasValue) job.RetryCount = request.RetryCount.Value;
        if (request.TimeoutSeconds.HasValue) job.TimeoutSeconds = request.TimeoutSeconds.Value;
        if (request.Parameters != null) job.Parameters = request.Parameters;

        job.UpdatedAt = DateTime.UtcNow;

        var updatedJob = await _repository.UpdateAsync(job, cancellationToken);

        // Reschedule if cron changed
        if (request.CronExpression != null && job.IsExecutable())
        {
            _scheduler.RemoveScheduledJob(job.Id.ToString());
            _scheduler.ScheduleRecurringJob(updatedJob);
        }

        return updatedJob;
    }
}

public class DeleteJobCommandHandler : IRequestHandler<DeleteJobCommand, bool>
{
    private readonly IJobRepository _repository;
    private readonly IJobScheduler _scheduler;

    public DeleteJobCommandHandler(IJobRepository repository, IJobScheduler scheduler)
    {
        _repository = repository;
        _scheduler = scheduler;
    }

    public async Task<bool> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (job == null) return false;

        // Remove from Hangfire
        _scheduler.RemoveScheduledJob(job.Id.ToString());

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}

public class EnableJobCommandHandler : IRequestHandler<EnableJobCommand, Job>
{
    private readonly IJobRepository _repository;
    private readonly IJobScheduler _scheduler;

    public EnableJobCommandHandler(IJobRepository repository, IJobScheduler scheduler)
    {
        _repository = repository;
        _scheduler = scheduler;
    }

    public async Task<Job> Handle(EnableJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Job with ID {request.Id} not found");

        job.Enable();
        var updated = await _repository.UpdateAsync(job, cancellationToken);

        // Schedule in Hangfire
        _scheduler.ScheduleRecurringJob(updated);

        return updated;
    }
}

public class DisableJobCommandHandler : IRequestHandler<DisableJobCommand, Job>
{
    private readonly IJobRepository _repository;
    private readonly IJobScheduler _scheduler;

    public DisableJobCommandHandler(IJobRepository repository, IJobScheduler scheduler)
    {
        _repository = repository;
        _scheduler = scheduler;
    }

    public async Task<Job> Handle(DisableJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Job with ID {request.Id} not found");

        job.Disable();
        var updated = await _repository.UpdateAsync(job, cancellationToken);

        // Remove from Hangfire
        _scheduler.RemoveScheduledJob(job.Id.ToString());

        return updated;
    }
}

public class PauseJobCommandHandler : IRequestHandler<PauseJobCommand, Job>
{
    private readonly IJobRepository _repository;
    private readonly IJobScheduler _scheduler;

    public PauseJobCommandHandler(IJobRepository repository, IJobScheduler scheduler)
    {
        _repository = repository;
        _scheduler = scheduler;
    }

    public async Task<Job> Handle(PauseJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Job with ID {request.Id} not found");

        job.Pause();
        var updated = await _repository.UpdateAsync(job, cancellationToken);

        // Pause in Hangfire
        _scheduler.PauseJob(job.Id.ToString());

        return updated;
    }
}

public class TriggerJobCommandHandler : IRequestHandler<TriggerJobCommand, string>
{
    private readonly IJobRepository _repository;
    private readonly IJobScheduler _scheduler;

    public TriggerJobCommandHandler(IJobRepository repository, IJobScheduler scheduler)
    {
        _repository = repository;
        _scheduler = scheduler;
    }

    public async Task<string> Handle(TriggerJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Job with ID {request.Id} not found");

        var executionId = _scheduler.TriggerJob(job.Id);
        return executionId;
    }
}

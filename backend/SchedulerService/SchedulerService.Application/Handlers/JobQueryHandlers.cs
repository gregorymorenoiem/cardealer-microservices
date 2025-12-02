using MediatR;
using SchedulerService.Application.Interfaces;
using SchedulerService.Application.Queries;
using SchedulerService.Domain.Entities;

namespace SchedulerService.Application.Handlers;

public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, Job?>
{
    private readonly IJobRepository _repository;

    public GetJobByIdQueryHandler(IJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<Job?> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}

public class GetAllJobsQueryHandler : IRequestHandler<GetAllJobsQuery, List<Job>>
{
    private readonly IJobRepository _repository;

    public GetAllJobsQueryHandler(IJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Job>> Handle(GetAllJobsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}

public class GetActiveJobsQueryHandler : IRequestHandler<GetActiveJobsQuery, List<Job>>
{
    private readonly IJobRepository _repository;

    public GetActiveJobsQueryHandler(IJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Job>> Handle(GetActiveJobsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetActiveJobsAsync(cancellationToken);
    }
}

public class GetJobExecutionsQueryHandler : IRequestHandler<GetJobExecutionsQuery, List<JobExecution>>
{
    private readonly IJobExecutionRepository _repository;

    public GetJobExecutionsQueryHandler(IJobExecutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<JobExecution>> Handle(GetJobExecutionsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByJobIdAsync(request.JobId, request.PageSize, cancellationToken);
    }
}

public class GetRecentExecutionsQueryHandler : IRequestHandler<GetRecentExecutionsQuery, List<JobExecution>>
{
    private readonly IJobExecutionRepository _repository;

    public GetRecentExecutionsQueryHandler(IJobExecutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<JobExecution>> Handle(GetRecentExecutionsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetRecentExecutionsAsync(request.PageSize, cancellationToken);
    }
}

public class GetExecutionByIdQueryHandler : IRequestHandler<GetExecutionByIdQuery, JobExecution?>
{
    private readonly IJobExecutionRepository _repository;

    public GetExecutionByIdQueryHandler(IJobExecutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<JobExecution?> Handle(GetExecutionByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}

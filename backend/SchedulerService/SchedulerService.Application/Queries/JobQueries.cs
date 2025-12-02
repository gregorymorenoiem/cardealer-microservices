using MediatR;
using SchedulerService.Domain.Entities;

namespace SchedulerService.Application.Queries;

public record GetJobByIdQuery(Guid Id) : IRequest<Job?>;

public record GetAllJobsQuery() : IRequest<List<Job>>;

public record GetActiveJobsQuery() : IRequest<List<Job>>;

public record GetJobExecutionsQuery(Guid JobId, int PageSize = 50) : IRequest<List<JobExecution>>;

public record GetRecentExecutionsQuery(int PageSize = 100) : IRequest<List<JobExecution>>;

public record GetExecutionByIdQuery(Guid Id) : IRequest<JobExecution?>;

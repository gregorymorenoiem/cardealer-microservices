using MediatR;
using SchedulerService.Domain.Entities;

namespace SchedulerService.Application.Commands;

public record CreateJobCommand(
    string Name,
    string Description,
    string JobType,
    string CronExpression,
    int RetryCount,
    int TimeoutSeconds,
    Dictionary<string, string> Parameters
) : IRequest<Job>;

public record UpdateJobCommand(
    Guid Id,
    string? Name,
    string? Description,
    string? CronExpression,
    int? RetryCount,
    int? TimeoutSeconds,
    Dictionary<string, string>? Parameters
) : IRequest<Job>;

public record DeleteJobCommand(Guid Id) : IRequest<bool>;

public record EnableJobCommand(Guid Id) : IRequest<Job>;

public record DisableJobCommand(Guid Id) : IRequest<Job>;

public record PauseJobCommand(Guid Id) : IRequest<Job>;

public record TriggerJobCommand(Guid Id) : IRequest<string>;

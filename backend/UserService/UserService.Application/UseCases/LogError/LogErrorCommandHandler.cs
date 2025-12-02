using MediatR;
using UserService.Application.DTOs;
using UserService.Application.Metrics;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using System.Diagnostics;

namespace UserService.Application.UseCases.LogError;

public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, LogErrorResponse>
{
    private readonly IRoleRepository _repo;
    private readonly UserServiceMetrics _metrics;

    public LogErrorCommandHandler(IRoleRepository repo, UserServiceMetrics metrics)
    {
        _repo = repo;
        _metrics = metrics;
    }

    public async Task<LogErrorResponse> Handle(LogErrorCommand command, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var success = false;

        try
        {
            var role = new Role
            {
                Id = Guid.NewGuid(),
                ServiceName = command.Request.ServiceName,
                ExceptionType = command.Request.ExceptionType,
                Message = command.Request.Message,
                StackTrace = command.Request.StackTrace,
                OccurredAt = command.Request.OccurredAt ?? DateTime.UtcNow,
                Endpoint = command.Request.Endpoint,
                HttpMethod = command.Request.HttpMethod,
                StatusCode = command.Request.StatusCode,
                UserId = command.Request.UserId,
                Metadata = command.Request.Metadata ?? new System.Collections.Generic.Dictionary<string, object>()
            };

            await _repo.AddAsync(role);

            // metrics may be recorded by the real implementation; keep minimal here
            success = true;
            return new LogErrorResponse(role.Id);
        }
        finally
        {
            sw.Stop();
            _metrics.RecordProcessingDuration(sw.Elapsed.TotalMilliseconds, command.Request.ServiceName, success);
        }
    }
}

using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Application.Metrics;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UserService.Application.UseCases.LogError
{
    public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, LogErrorResponse>
    {
        private readonly IRoleRepository _RoleRepository;
        private readonly UserServiceMetrics _metrics;

        public LogErrorCommandHandler(
            IRoleRepository RoleRepository,
            UserServiceMetrics metrics)
        {
            _RoleRepository = RoleRepository;
            _metrics = metrics;
        }

        public async Task<LogErrorResponse> Handle(LogErrorCommand command, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var success = false;

            try
            {
                var Role = new Role
                {
                    Id = System.Guid.NewGuid(),
                    ServiceName = command.Request.ServiceName,
                    ExceptionType = command.Request.ExceptionType,
                    Message = command.Request.Message,
                    StackTrace = command.Request.StackTrace,
                    OccurredAt = command.Request.OccurredAt ?? System.DateTime.UtcNow,
                    Endpoint = command.Request.Endpoint,
                    HttpMethod = command.Request.HttpMethod,
                    StatusCode = command.Request.StatusCode,
                    UserId = command.Request.UserId,
                    Metadata = command.Request.Metadata ?? new System.Collections.Generic.Dictionary<string, object>()
                };

                await _RoleRepository.AddAsync(Role);

                // Registrar métricas
                _metrics.RecordRoleged(
                    serviceName: command.Request.ServiceName,
                    statusCode: command.Request.StatusCode ?? 500,
                    exceptionType: command.Request.ExceptionType);

                success = true;
                return new LogErrorResponse(Role.Id);
            }
            finally
            {
                stopwatch.Stop();
                _metrics.RecordProcessingDuration(
                    durationMs: stopwatch.Elapsed.TotalMilliseconds,
                    serviceName: command.Request.ServiceName,
                    success: success);
            }
        }
    }
}

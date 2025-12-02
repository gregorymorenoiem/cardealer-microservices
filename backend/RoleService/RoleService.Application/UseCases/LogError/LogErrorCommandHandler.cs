using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoleService.Application.DTOs;
using RoleService.Application.Metrics;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;

namespace RoleService.Application.UseCases.LogError
{
    public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, LogErrorResponse>
    {
        private readonly IRoleRepository _repository;
        private readonly RoleServiceMetrics _metrics;

        public LogErrorCommandHandler(IRoleRepository repository, RoleServiceMetrics metrics)
        {
            _repository = repository;
            _metrics = metrics;
        }

        public async Task<LogErrorResponse> Handle(LogErrorCommand command, CancellationToken cancellationToken)
        {
            var dto = command.Request;

            var role = new Role
            {
                Id = Guid.NewGuid(),
                ServiceName = dto.ServiceName,
                ExceptionType = dto.ExceptionType,
                Message = dto.Message,
                StackTrace = dto.StackTrace,
                OccurredAt = dto.OccurredAt ?? DateTime.UtcNow
            };

            await _repository.AddAsync(role);

            // record metric
            try
            {
                _metrics.RecordRoleged(dto.ServiceName, dto.StatusCode ?? 0, dto.ExceptionType);
            }
            catch { }

            return new LogErrorResponse(role.Id);
        }
    }
}

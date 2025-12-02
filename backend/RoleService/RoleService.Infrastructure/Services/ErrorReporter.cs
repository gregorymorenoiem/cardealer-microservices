using System;
using System.Threading.Tasks;
using MediatR;
using RoleService.Application.DTOs;
using RoleService.Application.UseCases.LogError;
using RoleService.Domain.Interfaces;

namespace RoleService.Infrastructure.Services
{
    public class ErrorReporter : IErrorReporter
    {
        private readonly IMediator _mediator;

        public ErrorReporter(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Guid> ReportErrorAsync(ErrorReport request)
        {
            var dto = new LogErrorRequest(
                request.ServiceName,
                request.ExceptionType,
                request.Message,
                request.StackTrace,
                request.OccurredAt,
                request.Endpoint,
                request.HttpMethod,
                request.StatusCode,
                request.UserId,
                request.Metadata
            );

            var response = await _mediator.Send(new LogErrorCommand(dto));
            return response.ErrorId;
        }
    }
}

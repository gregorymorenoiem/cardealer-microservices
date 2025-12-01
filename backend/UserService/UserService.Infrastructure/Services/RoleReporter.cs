using UserService.Application.DTOs;
using UserService.Application.UseCases.LogError;
using UserService.Domain.Interfaces;
using MediatR;
using System;
using System.Threading.Tasks;

namespace UserService.Infrastructure.Services
{
    public class ErrorReporter : IErrorReporter
    {
        private readonly IMediator _mediator;

        public ErrorReporter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Guid> ReportErrorAsync(ErrorReport request)
        {
            // Usar el constructor del record con todos los parámetros
            var logRequest = new LogErrorRequest(
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

            var command = new LogErrorCommand(logRequest);
            var result = await _mediator.Send(command);
            return result.ErrorId;
        }
    }
}

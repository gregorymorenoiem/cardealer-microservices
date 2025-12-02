using MediatR;
using UserService.Domain.Interfaces;
using UserService.Application.DTOs;
using UserService.Application.UseCases.LogError;

namespace UserService.Infrastructure.Services;

public class ErrorReporter : IErrorReporter
{
    private readonly IMediator _mediator;

    public ErrorReporter(IMediator mediator)
    {
        _mediator = mediator;
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
            request.Metadata);

        var response = await _mediator.Send(new LogErrorCommand(dto));
        return response.ErrorId;
    }
}

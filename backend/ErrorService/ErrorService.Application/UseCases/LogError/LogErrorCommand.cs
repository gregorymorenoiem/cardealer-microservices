using ErrorService.Application.DTOs;
using MediatR;

namespace ErrorService.Application.UseCases.LogError
{
    public record LogErrorCommand(LogErrorRequest Request) : IRequest<LogErrorResponse>;
}
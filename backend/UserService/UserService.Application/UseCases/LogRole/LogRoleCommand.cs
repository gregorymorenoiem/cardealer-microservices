using UserService.Application.DTOs;
using MediatR;

namespace UserService.Application.UseCases.LogError
{
    public record LogErrorCommand(LogErrorRequest Request) : IRequest<LogErrorResponse>;
}

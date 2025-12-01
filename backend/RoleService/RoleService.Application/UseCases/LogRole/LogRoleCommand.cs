using RoleService.Application.DTOs;
using MediatR;

namespace RoleService.Application.UseCases.LogError
{
    public record LogErrorCommand(LogErrorRequest Request) : IRequest<LogErrorResponse>;
}

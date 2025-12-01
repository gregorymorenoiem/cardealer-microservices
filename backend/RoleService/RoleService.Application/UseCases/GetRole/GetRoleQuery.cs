using RoleService.Application.DTOs;
using MediatR;

namespace RoleService.Application.UseCases.GetError
{
    public record GetErrorQuery(GetErrorRequest Request) : IRequest<GetErrorResponse>;
}

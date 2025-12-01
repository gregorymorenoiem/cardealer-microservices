using RoleService.Application.DTOs;
using MediatR;

namespace RoleService.Application.UseCases.GetServiceNames
{
    public record GetServiceNamesQuery(GetServiceNamesRequest Request) : IRequest<GetServiceNamesResponse>;
}

using RoleService.Application.DTOs;
using MediatR;

namespace RoleService.Application.UseCases.GetErrors
{
    public record GetErrorsQuery(GetErrorsRequest Request) : IRequest<GetErrorsResponse>;
}

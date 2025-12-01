using UserService.Application.DTOs;
using MediatR;

namespace UserService.Application.UseCases.GetServiceNames
{
    public record GetServiceNamesQuery(GetServiceNamesRequest Request) : IRequest<GetServiceNamesResponse>;
}

using ErrorService.Application.DTOs;
using MediatR;

namespace ErrorService.Application.UseCases.GetServiceNames
{
    public record GetServiceNamesQuery(GetServiceNamesRequest Request) : IRequest<GetServiceNamesResponse>;
}
using ErrorService.Application.DTOs;
using MediatR;

namespace ErrorService.Application.UseCases.GetError
{
    public record GetErrorQuery(GetErrorRequest Request) : IRequest<GetErrorResponse>;
}
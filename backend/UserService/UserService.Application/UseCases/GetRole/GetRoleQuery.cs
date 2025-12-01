using UserService.Application.DTOs;
using MediatR;

namespace UserService.Application.UseCases.GetError
{
    public record GetErrorQuery(GetErrorRequest Request) : IRequest<GetErrorResponse>;
}

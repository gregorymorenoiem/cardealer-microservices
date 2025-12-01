using UserService.Application.DTOs;
using MediatR;

namespace UserService.Application.UseCases.GetErrors
{
    public record GetErrorsQuery(GetErrorsRequest Request) : IRequest<GetErrorsResponse>;
}

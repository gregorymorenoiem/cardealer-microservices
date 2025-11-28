using ErrorService.Application.DTOs;
using MediatR;

namespace ErrorService.Application.UseCases.GetErrors
{
    public record GetErrorsQuery(GetErrorsRequest Request) : IRequest<GetErrorsResponse>;
}
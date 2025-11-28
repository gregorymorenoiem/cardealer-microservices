using ErrorService.Application.DTOs;
using MediatR;

namespace ErrorService.Application.UseCases.GetErrorStats
{
    public record GetErrorStatsQuery(GetErrorStatsRequest Request) : IRequest<GetErrorStatsResponse>;
}
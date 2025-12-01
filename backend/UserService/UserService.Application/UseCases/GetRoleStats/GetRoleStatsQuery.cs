using UserService.Application.DTOs;
using MediatR;

namespace UserService.Application.UseCases.GetErrorStats
{
    public record GetErrorStatsQuery(GetErrorStatsRequest Request) : IRequest<GetErrorStatsResponse>;
}

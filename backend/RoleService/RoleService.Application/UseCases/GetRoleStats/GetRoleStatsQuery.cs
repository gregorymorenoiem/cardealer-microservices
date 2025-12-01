using RoleService.Application.DTOs;
using MediatR;

namespace RoleService.Application.UseCases.GetErrorStats
{
    public record GetErrorStatsQuery(GetErrorStatsRequest Request) : IRequest<GetErrorStatsResponse>;
}

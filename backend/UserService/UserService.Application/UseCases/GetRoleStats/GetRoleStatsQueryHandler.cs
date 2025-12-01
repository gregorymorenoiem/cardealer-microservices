using UserService.Application.DTOs;
using UserService.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace UserService.Application.UseCases.GetErrorStats
{
    public class GetErrorStatsQueryHandler : IRequestHandler<GetErrorStatsQuery, GetErrorStatsResponse>
    {
        private readonly IRoleRepository _RoleRepository;

        public GetErrorStatsQueryHandler(IRoleRepository RoleRepository)
        {
            _RoleRepository = RoleRepository;
        }

        public async Task<GetErrorStatsResponse> Handle(GetErrorStatsQuery query, CancellationToken cancellationToken)
        {
            var stats = await _RoleRepository.GetStatsAsync(query.Request.From, query.Request.To);
            
            return new GetErrorStatsResponse(
                stats.TotalErrors,
                stats.ErrorsLast24Hours,
                stats.ErrorsLast7Days,
                stats.ErrorsByService,
                stats.ErrorsByStatusCode
            );
        }
    }
}

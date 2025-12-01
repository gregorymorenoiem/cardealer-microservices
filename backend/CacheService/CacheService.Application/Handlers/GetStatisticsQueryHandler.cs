using MediatR;
using CacheService.Application.Queries;
using CacheService.Application.Interfaces;
using CacheService.Domain;

namespace CacheService.Application.Handlers;

public class GetStatisticsQueryHandler : IRequestHandler<GetStatisticsQuery, CacheStatistics>
{
    private readonly IStatisticsManager _statisticsManager;

    public GetStatisticsQueryHandler(IStatisticsManager statisticsManager)
    {
        _statisticsManager = statisticsManager;
    }

    public async Task<CacheStatistics> Handle(GetStatisticsQuery request, CancellationToken cancellationToken)
    {
        return await _statisticsManager.GetStatisticsAsync(cancellationToken);
    }
}

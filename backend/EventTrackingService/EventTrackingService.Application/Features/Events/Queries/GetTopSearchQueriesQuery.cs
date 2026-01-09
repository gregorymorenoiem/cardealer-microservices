using EventTrackingService.Application.DTOs;
using EventTrackingService.Domain.Interfaces;
using MediatR;

namespace EventTrackingService.Application.Features.Events.Queries;

/// <summary>
/// Query para obtener las búsquedas más populares
/// </summary>
public record GetTopSearchQueriesQuery : IRequest<List<TopSearchQueryDto>>
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TopN { get; init; } = 20;
}

/// <summary>
/// Handler para GetTopSearchQueriesQuery
/// </summary>
public class GetTopSearchQueriesQueryHandler : IRequestHandler<GetTopSearchQueriesQuery, List<TopSearchQueryDto>>
{
    private readonly IEventRepository _eventRepository;

    public GetTopSearchQueriesQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<List<TopSearchQueryDto>> Handle(GetTopSearchQueriesQuery request, CancellationToken cancellationToken)
    {
        var topSearches = await _eventRepository.GetTopSearchQueriesAsync(
            request.StartDate,
            request.EndDate,
            request.TopN,
            cancellationToken
        );

        return topSearches.Select(kvp => new TopSearchQueryDto
        {
            Query = kvp.Key,
            Count = kvp.Value.Count,
            AverageResultsCount = kvp.Value.AvgResults,
            ClickThroughRate = kvp.Value.CTR
        }).ToList();
    }
}

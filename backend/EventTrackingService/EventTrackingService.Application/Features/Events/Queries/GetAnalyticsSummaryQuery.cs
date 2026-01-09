using EventTrackingService.Application.DTOs;
using EventTrackingService.Domain.Interfaces;
using MediatR;

namespace EventTrackingService.Application.Features.Events.Queries;

/// <summary>
/// Query para obtener resumen completo de analytics
/// </summary>
public record GetAnalyticsSummaryQuery : IRequest<AnalyticsSummaryDto>
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TopN { get; init; } = 10;
}

/// <summary>
/// Handler para GetAnalyticsSummaryQuery
/// </summary>
public class GetAnalyticsSummaryQueryHandler : IRequestHandler<GetAnalyticsSummaryQuery, AnalyticsSummaryDto>
{
    private readonly IEventRepository _eventRepository;

    public GetAnalyticsSummaryQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<AnalyticsSummaryDto> Handle(GetAnalyticsSummaryQuery request, CancellationToken cancellationToken)
    {
        // Ejecutar queries en paralelo para mejor performance
        var eventsByTypeTask = _eventRepository.CountEventsByTypeAsync(request.StartDate, request.EndDate, cancellationToken);
        var uniqueVisitorsTask = _eventRepository.GetUniqueVisitorsCountAsync(request.StartDate, request.EndDate, cancellationToken);
        var conversionRateTask = _eventRepository.GetConversionRateAsync(request.StartDate, request.EndDate, cancellationToken);
        var topSearchesTask = _eventRepository.GetTopSearchQueriesAsync(request.StartDate, request.EndDate, request.TopN, cancellationToken);
        var topVehiclesTask = _eventRepository.GetMostViewedVehiclesAsync(request.StartDate, request.EndDate, request.TopN, cancellationToken);

        await Task.WhenAll(eventsByTypeTask, uniqueVisitorsTask, conversionRateTask, topSearchesTask, topVehiclesTask);

        var eventsByType = await eventsByTypeTask;
        var uniqueVisitors = await uniqueVisitorsTask;
        var conversionRate = await conversionRateTask;
        var topSearches = await topSearchesTask;
        var topVehicles = await topVehiclesTask;

        // Calcular métricas
        var totalPageViews = eventsByType.TryGetValue("PageView", out var pageViews) ? pageViews : 0;
        var totalSearches = eventsByType.TryGetValue("Search", out var searches) ? searches : 0;
        var totalVehicleViews = eventsByType.TryGetValue("VehicleView", out var vehicleViews) ? vehicleViews : 0;

        // Para bounce rate y session duration necesitamos más lógica (simplificado aquí)
        var bounceRate = 0.35; // Placeholder - requiere cálculo real
        var avgSessionDuration = 180.0; // Placeholder - requiere cálculo real

        return new AnalyticsSummaryDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalPageViews = totalPageViews,
            TotalSessions = uniqueVisitors, // Simplificación
            UniqueVisitors = uniqueVisitors,
            TotalSearches = totalSearches,
            TotalVehicleViews = totalVehicleViews,
            BounceRate = bounceRate,
            AverageSessionDuration = avgSessionDuration,
            ConversionRate = conversionRate,
            EventsByType = eventsByType.Select(kvp => new EventTypeStatsDto
            {
                EventType = kvp.Key,
                Count = kvp.Value,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            }).ToList(),
            TopSearches = topSearches.Select(s => new TopSearchQueryDto
            {
                Query = s.Key,
                Count = s.Value.Count,
                AverageResultsCount = s.Value.AvgResults,
                ClickThroughRate = s.Value.CTR
            }).ToList(),
            TopVehicles = topVehicles.Select(v => new MostViewedVehicleDto
            {
                VehicleId = v.Key,
                VehicleTitle = v.Value.Title,
                ViewCount = v.Value.Views,
                AverageTimeSpent = v.Value.AvgTime,
                ContactClicks = v.Value.Contacts,
                FavoriteAdds = v.Value.Favorites,
                ConversionRate = v.Value.ConversionRate
            }).ToList()
        };
    }
}

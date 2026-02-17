using EventTrackingService.Application.DTOs;
using EventTrackingService.Domain.Interfaces;
using MediatR;

namespace EventTrackingService.Application.Features.Events.Queries;

/// <summary>
/// Query para obtener los vehículos más vistos
/// </summary>
public record GetMostViewedVehiclesQuery : IRequest<List<MostViewedVehicleDto>>
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TopN { get; init; } = 20;
}

/// <summary>
/// Handler para GetMostViewedVehiclesQuery
/// </summary>
public class GetMostViewedVehiclesQueryHandler : IRequestHandler<GetMostViewedVehiclesQuery, List<MostViewedVehicleDto>>
{
    private readonly IEventRepository _eventRepository;

    public GetMostViewedVehiclesQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<List<MostViewedVehicleDto>> Handle(GetMostViewedVehiclesQuery request, CancellationToken cancellationToken)
    {
        var topVehicles = await _eventRepository.GetMostViewedVehiclesAsync(
            request.StartDate,
            request.EndDate,
            request.TopN,
            cancellationToken
        );

        return topVehicles.Select(kvp => new MostViewedVehicleDto
        {
            VehicleId = kvp.Key,
            VehicleTitle = kvp.Value.Title,
            ViewCount = kvp.Value.Views,
            AverageTimeSpent = kvp.Value.AvgTime,
            ContactClicks = kvp.Value.Contacts,
            FavoriteAdds = kvp.Value.Favorites,
            ConversionRate = kvp.Value.ConversionRate
        }).ToList();
    }
}

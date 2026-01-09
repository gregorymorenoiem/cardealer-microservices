using EventTrackingService.Application.DTOs;
using EventTrackingService.Domain.Interfaces;
using MediatR;

namespace EventTrackingService.Application.Features.Events.Queries;

/// <summary>
/// Query para obtener eventos por tipo
/// </summary>
public record GetEventsByTypeQuery : IRequest<List<TrackedEventDto>>
{
    public string EventType { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int Limit { get; init; } = 100;
}

/// <summary>
/// Handler para GetEventsByTypeQuery
/// </summary>
public class GetEventsByTypeQueryHandler : IRequestHandler<GetEventsByTypeQuery, List<TrackedEventDto>>
{
    private readonly IEventRepository _eventRepository;

    public GetEventsByTypeQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<List<TrackedEventDto>> Handle(GetEventsByTypeQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-7);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var events = await _eventRepository.GetEventsByTypeAsync(
            request.EventType,
            startDate,
            endDate,
            request.Limit,
            cancellationToken
        );

        return events.Select(e => new TrackedEventDto
        {
            Id = e.Id,
            EventType = e.EventType,
            Timestamp = e.Timestamp,
            UserId = e.UserId,
            SessionId = e.SessionId,
            IpAddress = e.IpAddress,
            UserAgent = e.UserAgent,
            Referrer = e.Referrer,
            CurrentUrl = e.CurrentUrl,
            DeviceType = e.DeviceType,
            Browser = e.Browser,
            OperatingSystem = e.OperatingSystem,
            Country = e.Country,
            City = e.City,
            EventData = e.EventData,
            Source = e.Source,
            Campaign = e.Campaign,
            Medium = e.Medium,
            Content = e.Content
        }).ToList();
    }
}

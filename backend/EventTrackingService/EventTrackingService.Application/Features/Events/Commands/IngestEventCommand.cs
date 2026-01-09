using EventTrackingService.Application.DTOs;
using EventTrackingService.Domain.Entities;
using EventTrackingService.Domain.Interfaces;
using MediatR;

namespace EventTrackingService.Application.Features.Events.Commands;

/// <summary>
/// Command para ingerir un solo evento
/// </summary>
public record IngestEventCommand : IRequest<EventIngestionResponseDto>
{
    public CreateTrackedEventDto Event { get; init; } = null!;
}

/// <summary>
/// Handler para IngestEventCommand
/// </summary>
public class IngestEventCommandHandler : IRequestHandler<IngestEventCommand, EventIngestionResponseDto>
{
    private readonly IEventRepository _eventRepository;

    public IngestEventCommandHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<EventIngestionResponseDto> Handle(IngestEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Crear la entidad - todos los eventos se mapean al tipo base TrackedEvent
            // El EventData JSON contiene propiedades adicionales específicas del tipo
            TrackedEvent entity = MapToTrackedEvent(request.Event);

            // Ingerir el evento
            await _eventRepository.IngestEventAsync(entity, cancellationToken);

            return new EventIngestionResponseDto
            {
                Success = true,
                EventId = entity.Id,
                Message = "Event ingested successfully"
            };
        }
        catch (Exception ex)
        {
            return new EventIngestionResponseDto
            {
                Success = false,
                Message = $"Failed to ingest event: {ex.Message}"
            };
        }
    }

    private static TrackedEvent MapToTrackedEvent(CreateTrackedEventDto dto)
    {
        return new TrackedEvent
        {
            Id = Guid.NewGuid(),
            EventType = dto.EventType,
            Timestamp = DateTime.UtcNow,
            UserId = dto.UserId,
            SessionId = dto.SessionId,
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent,
            Referrer = dto.Referrer,
            CurrentUrl = dto.CurrentUrl,
            DeviceType = dto.DeviceType,
            Browser = dto.Browser,
            OperatingSystem = dto.OperatingSystem,
            Country = dto.Country,
            City = dto.City,
            EventData = dto.EventData,
            Source = dto.Source,
            Campaign = dto.Campaign,
            Medium = dto.Medium,
            Content = dto.Content
        };
    }

    private static PageViewEvent MapToPageViewEvent(CreatePageViewEventDto dto)
    {
        return new PageViewEvent
        {
            Id = Guid.NewGuid(),
            EventType = dto.EventType,
            Timestamp = DateTime.UtcNow,
            UserId = dto.UserId,
            SessionId = dto.SessionId,
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent,
            Referrer = dto.Referrer,
            CurrentUrl = dto.CurrentUrl,
            DeviceType = dto.DeviceType,
            Browser = dto.Browser,
            OperatingSystem = dto.OperatingSystem,
            Country = dto.Country,
            City = dto.City,
            EventData = dto.EventData,
            Source = dto.Source,
            Campaign = dto.Campaign,
            Medium = dto.Medium,
            Content = dto.Content,
            PageUrl = dto.PageUrl,
            PageTitle = dto.PageTitle,
            PreviousUrl = dto.PreviousUrl,
            ScrollDepth = dto.ScrollDepth,
            TimeOnPage = dto.TimeOnPage
        };
    }

    private static SearchEvent MapToSearchEvent(CreateSearchEventDto dto)
    {
        return new SearchEvent
        {
            Id = Guid.NewGuid(),
            EventType = dto.EventType,
            Timestamp = DateTime.UtcNow,
            UserId = dto.UserId,
            SessionId = dto.SessionId,
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent,
            Referrer = dto.Referrer,
            CurrentUrl = dto.CurrentUrl,
            DeviceType = dto.DeviceType,
            Browser = dto.Browser,
            OperatingSystem = dto.OperatingSystem,
            Country = dto.Country,
            City = dto.City,
            EventData = dto.EventData,
            Source = dto.Source,
            Campaign = dto.Campaign,
            Medium = dto.Medium,
            Content = dto.Content,
            SearchQuery = dto.SearchQuery,
            ResultsCount = dto.ResultsCount,
            SearchType = dto.SearchType,
            AppliedFilters = dto.AppliedFilters,
            SortBy = dto.SortBy
        };
    }

    private static VehicleViewEvent MapToVehicleViewEvent(CreateVehicleViewEventDto dto)
    {
        return new VehicleViewEvent
        {
            Id = Guid.NewGuid(),
            EventType = dto.EventType,
            Timestamp = DateTime.UtcNow,
            UserId = dto.UserId,
            SessionId = dto.SessionId,
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent,
            Referrer = dto.Referrer,
            CurrentUrl = dto.CurrentUrl,
            DeviceType = dto.DeviceType,
            Browser = dto.Browser,
            OperatingSystem = dto.OperatingSystem,
            Country = dto.Country,
            City = dto.City,
            EventData = dto.EventData,
            Source = dto.Source,
            Campaign = dto.Campaign,
            Medium = dto.Medium,
            Content = dto.Content,
            VehicleId = dto.VehicleId,
            VehicleTitle = dto.VehicleTitle,
            VehiclePrice = dto.VehiclePrice,
            VehicleMake = dto.VehicleMake,
            VehicleModel = dto.VehicleModel,
            VehicleYear = dto.VehicleYear,
            ViewSource = dto.ViewSource
        };
    }

    private static FilterEvent MapToFilterEvent(CreateFilterEventDto dto)
    {
        return new FilterEvent
        {
            Id = Guid.NewGuid(),
            EventType = dto.EventType,
            Timestamp = DateTime.UtcNow,
            UserId = dto.UserId,
            SessionId = dto.SessionId,
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent,
            Referrer = dto.Referrer,
            CurrentUrl = dto.CurrentUrl,
            DeviceType = dto.DeviceType,
            Browser = dto.Browser,
            OperatingSystem = dto.OperatingSystem,
            Country = dto.Country,
            City = dto.City,
            EventData = dto.EventData,
            Source = dto.Source,
            Campaign = dto.Campaign,
            Medium = dto.Medium,
            Content = dto.Content,
            FilterType = dto.FilterType,
            FilterValue = dto.FilterValue,
            FilterOperator = dto.FilterOperator,
            ResultsAfterFilter = dto.ResultsAfterFilter,
            PageContext = dto.PageContext
        };
    }
}

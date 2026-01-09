using EventTrackingService.Application.DTOs;
using EventTrackingService.Domain.Entities;
using EventTrackingService.Domain.Interfaces;
using MediatR;

namespace EventTrackingService.Application.Features.Events.Commands;

/// <summary>
/// Command para ingerir múltiples eventos en batch (alta performance)
/// </summary>
public record IngestBatchCommand : IRequest<BatchIngestionResponseDto>
{
    public List<CreateTrackedEventDto> Events { get; init; } = new();
}

/// <summary>
/// Handler para IngestBatchCommand
/// </summary>
public class IngestBatchCommandHandler : IRequestHandler<IngestBatchCommand, BatchIngestionResponseDto>
{
    private readonly IEventRepository _eventRepository;

    public IngestBatchCommandHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<BatchIngestionResponseDto> Handle(IngestBatchCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var entities = new List<TrackedEvent>();

        // Mapear todos los eventos
        for (int i = 0; i < request.Events.Count; i++)
        {
            try
            {
                var dto = request.Events[i];
                // Mapear todos los eventos al tipo base TrackedEvent
                // El EventData JSON contiene propiedades adicionales específicas del tipo
                TrackedEvent entity = MapToTrackedEvent(dto);
                entities.Add(entity);
            }
            catch (Exception ex)
            {
                errors.Add($"Event {i}: {ex.Message}");
            }
        }

        // Ingerir en batch
        try
        {
            if (entities.Any())
            {
                await _eventRepository.IngestBatchAsync(entities, cancellationToken);
            }

            return new BatchIngestionResponseDto
            {
                Success = errors.Count == 0,
                TotalEvents = request.Events.Count,
                SuccessfulEvents = entities.Count,
                FailedEvents = errors.Count,
                Errors = errors.Any() ? errors : null
            };
        }
        catch (Exception ex)
        {
            errors.Add($"Batch ingestion failed: {ex.Message}");
            return new BatchIngestionResponseDto
            {
                Success = false,
                TotalEvents = request.Events.Count,
                SuccessfulEvents = 0,
                FailedEvents = request.Events.Count,
                Errors = errors
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

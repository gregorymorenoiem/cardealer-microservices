using EventTrackingService.Domain.Entities;

namespace EventTrackingService.Application.DTOs;

/// <summary>
/// DTO base para eventos rastreados
/// </summary>
public record TrackedEventDto
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public Guid? UserId { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public string UserAgent { get; init; } = string.Empty;
    public string? Referrer { get; init; }
    public string CurrentUrl { get; init; } = string.Empty;
    public string DeviceType { get; init; } = string.Empty;
    public string Browser { get; init; } = string.Empty;
    public string OperatingSystem { get; init; } = string.Empty;
    public string? Country { get; init; }
    public string? City { get; init; }
    public string? EventData { get; init; }
    public string? Source { get; init; }
    public string? Campaign { get; init; }
    public string? Medium { get; init; }
    public string? Content { get; init; }
}

/// <summary>
/// DTO para crear un evento (sin ID ni Timestamp, se generan en el backend)
/// </summary>
public record CreateTrackedEventDto
{
    public string EventType { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public string UserAgent { get; init; } = string.Empty;
    public string? Referrer { get; init; }
    public string CurrentUrl { get; init; } = string.Empty;
    public string DeviceType { get; init; } = string.Empty;
    public string Browser { get; init; } = string.Empty;
    public string OperatingSystem { get; init; } = string.Empty;
    public string? Country { get; init; }
    public string? City { get; init; }
    public string? EventData { get; init; }
    public string? Source { get; init; }
    public string? Campaign { get; init; }
    public string? Medium { get; init; }
    public string? Content { get; init; }
}

/// <summary>
/// DTO para evento de vista de página
/// </summary>
public record PageViewEventDto : TrackedEventDto
{
    public string PageUrl { get; init; } = string.Empty;
    public string PageTitle { get; init; } = string.Empty;
    public string? PreviousUrl { get; init; }
    public int ScrollDepth { get; init; }
    public int TimeOnPage { get; init; }
    public bool IsExit { get; init; }
    public bool IsBounce { get; init; }
}

/// <summary>
/// DTO para crear evento de vista de página
/// </summary>
public record CreatePageViewEventDto : CreateTrackedEventDto
{
    public string PageUrl { get; init; } = string.Empty;
    public string PageTitle { get; init; } = string.Empty;
    public string? PreviousUrl { get; init; }
    public int ScrollDepth { get; init; }
    public int TimeOnPage { get; init; }
}

/// <summary>
/// DTO para evento de búsqueda
/// </summary>
public record SearchEventDto : TrackedEventDto
{
    public string SearchQuery { get; init; } = string.Empty;
    public int ResultsCount { get; init; }
    public string SearchType { get; init; } = string.Empty;
    public string? AppliedFilters { get; init; }
    public string? SortBy { get; init; }
    public int? ClickedPosition { get; init; }
    public Guid? ClickedVehicleId { get; init; }
}

/// <summary>
/// DTO para crear evento de búsqueda
/// </summary>
public record CreateSearchEventDto : CreateTrackedEventDto
{
    public string SearchQuery { get; init; } = string.Empty;
    public int ResultsCount { get; init; }
    public string SearchType { get; init; } = "General";
    public string? AppliedFilters { get; init; }
    public string? SortBy { get; init; }
}

/// <summary>
/// DTO para evento de vista de vehículo
/// </summary>
public record VehicleViewEventDto : TrackedEventDto
{
    public Guid VehicleId { get; init; }
    public string VehicleTitle { get; init; } = string.Empty;
    public decimal VehiclePrice { get; init; }
    public string VehicleMake { get; init; } = string.Empty;
    public string VehicleModel { get; init; } = string.Empty;
    public int VehicleYear { get; init; }
    public int TimeSpentSeconds { get; init; }
    public bool ViewedImages { get; init; }
    public bool ViewedSpecs { get; init; }
    public bool ClickedContact { get; init; }
    public bool AddedToFavorites { get; init; }
    public bool SharedVehicle { get; init; }
    public string? ViewSource { get; init; }
}

/// <summary>
/// DTO para crear evento de vista de vehículo
/// </summary>
public record CreateVehicleViewEventDto : CreateTrackedEventDto
{
    public Guid VehicleId { get; init; }
    public string VehicleTitle { get; init; } = string.Empty;
    public decimal VehiclePrice { get; init; }
    public string VehicleMake { get; init; } = string.Empty;
    public string VehicleModel { get; init; } = string.Empty;
    public int VehicleYear { get; init; }
    public string? ViewSource { get; init; }
}

/// <summary>
/// DTO para evento de filtro
/// </summary>
public record FilterEventDto : TrackedEventDto
{
    public string FilterType { get; init; } = string.Empty;
    public string FilterValue { get; init; } = string.Empty;
    public string FilterOperator { get; init; } = string.Empty;
    public int ResultsAfterFilter { get; init; }
    public string PageContext { get; init; } = string.Empty;
}

/// <summary>
/// DTO para crear evento de filtro
/// </summary>
public record CreateFilterEventDto : CreateTrackedEventDto
{
    public string FilterType { get; init; } = string.Empty;
    public string FilterValue { get; init; } = string.Empty;
    public string FilterOperator { get; init; } = "equals";
    public int ResultsAfterFilter { get; init; }
    public string PageContext { get; init; } = string.Empty;
}

/// <summary>
/// DTO para lote de eventos (bulk ingestion)
/// </summary>
public record BatchEventsDto
{
    public List<CreateTrackedEventDto> Events { get; init; } = new();
}

/// <summary>
/// DTO para respuesta de ingesta
/// </summary>
public record EventIngestionResponseDto
{
    public bool Success { get; init; }
    public Guid? EventId { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// DTO para respuesta de ingesta en lote
/// </summary>
public record BatchIngestionResponseDto
{
    public bool Success { get; init; }
    public int TotalEvents { get; init; }
    public int SuccessfulEvents { get; init; }
    public int FailedEvents { get; init; }
    public List<string>? Errors { get; init; }
}

/// <summary>
/// DTO para estadísticas de eventos por tipo
/// </summary>
public record EventTypeStatsDto
{
    public string EventType { get; init; } = string.Empty;
    public long Count { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

/// <summary>
/// DTO para queries más buscadas
/// </summary>
public record TopSearchQueryDto
{
    public string Query { get; init; } = string.Empty;
    public long Count { get; init; }
    public double AverageResultsCount { get; init; }
    public double ClickThroughRate { get; init; }
}

/// <summary>
/// DTO para vehículos más vistos
/// </summary>
public record MostViewedVehicleDto
{
    public Guid VehicleId { get; init; }
    public string VehicleTitle { get; init; } = string.Empty;
    public long ViewCount { get; init; }
    public double AverageTimeSpent { get; init; }
    public long ContactClicks { get; init; }
    public long FavoriteAdds { get; init; }
    public double ConversionRate { get; init; }
}

/// <summary>
/// DTO para resumen de analytics
/// </summary>
public record AnalyticsSummaryDto
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public long TotalPageViews { get; init; }
    public long TotalSessions { get; init; }
    public long UniqueVisitors { get; init; }
    public long TotalSearches { get; init; }
    public long TotalVehicleViews { get; init; }
    public double BounceRate { get; init; }
    public double AverageSessionDuration { get; init; }
    public double ConversionRate { get; init; }
    public List<EventTypeStatsDto> EventsByType { get; init; } = new();
    public List<TopSearchQueryDto> TopSearches { get; init; } = new();
    public List<MostViewedVehicleDto> TopVehicles { get; init; } = new();
}

namespace EventTrackingService.Domain.Entities;

/// <summary>
/// Specialized entity for search events
/// </summary>
public class SearchEvent : TrackedEvent
{
    public string SearchQuery { get; set; } = string.Empty;
    public int ResultsCount { get; set; }
    public bool HasResults => ResultsCount > 0;
    public string SearchType { get; set; } = "General"; // "vehicles", "dealers", "general"
    public string? AppliedFilters { get; set; } // JSON string of applied filters
    public string? SortBy { get; set; }
    public int? ClickedPosition { get; set; } // Position of clicked result (if any)
    public Guid? ClickedVehicleId { get; set; }

    public SearchEvent()
    {
        EventType = "search";
    }

    /// <summary>
    /// Records that user clicked a search result
    /// </summary>
    public void RecordClick(int position, Guid? vehicleId = null)
    {
        ClickedPosition = position;
        ClickedVehicleId = vehicleId;
    }

    /// <summary>
    /// Checks if search was successful (had results and user clicked)
    /// </summary>
    public bool IsSuccessful() => HasResults && ClickedPosition.HasValue;

    /// <summary>
    /// Checks if this is a zero-results search
    /// </summary>
    public bool IsZeroResults() => ResultsCount == 0;
}

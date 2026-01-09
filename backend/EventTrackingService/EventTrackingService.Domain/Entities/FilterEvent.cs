namespace EventTrackingService.Domain.Entities;

/// <summary>
/// Specialized entity for filter application events
/// </summary>
public class FilterEvent : TrackedEvent
{
    public string FilterType { get; set; } = string.Empty; // "make", "price", "year", "mileage", etc.
    public string FilterValue { get; set; } = string.Empty;
    public string? FilterOperator { get; set; } // "equals", "range", "in", "greater_than", "less_than"
    public int ResultsAfterFilter { get; set; }
    public string? PageContext { get; set; } // "search_page", "homepage", "dealer_page"

    public FilterEvent()
    {
        EventType = "filter_apply";
    }

    /// <summary>
    /// Checks if filter resulted in zero results
    /// </summary>
    public bool IsZeroResults() => ResultsAfterFilter == 0;

    /// <summary>
    /// Checks if filter significantly reduced results (> 90% reduction)
    /// </summary>
    public bool IsNarrowingFilter(int resultsBefore)
    {
        if (resultsBefore == 0) return false;
        var reduction = (resultsBefore - ResultsAfterFilter) / (double)resultsBefore;
        return reduction > 0.9;
    }
}

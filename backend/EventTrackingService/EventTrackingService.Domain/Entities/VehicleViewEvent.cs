namespace EventTrackingService.Domain.Entities;

/// <summary>
/// Specialized entity for vehicle view events
/// </summary>
public class VehicleViewEvent : TrackedEvent
{
    public Guid VehicleId { get; set; }
    public string VehicleTitle { get; set; } = string.Empty;
    public decimal? VehiclePrice { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public int? VehicleYear { get; set; }
    public int? TimeSpentSeconds { get; set; }
    public bool ViewedImages { get; set; }
    public bool ViewedSpecs { get; set; }
    public bool ClickedContact { get; set; }
    public bool AddedToFavorites { get; set; }
    public bool SharedVehicle { get; set; }
    public string? ViewSource { get; set; } // "search_results", "homepage", "similar_vehicles", "dealer_profile"

    public VehicleViewEvent()
    {
        EventType = "vehicle_view";
    }

    /// <summary>
    /// Records user engagement with vehicle details
    /// </summary>
    public void RecordEngagement(bool viewedImages, bool viewedSpecs, bool clickedContact, bool addedToFavorites)
    {
        ViewedImages = viewedImages;
        ViewedSpecs = viewedSpecs;
        ClickedContact = clickedContact;
        AddedToFavorites = addedToFavorites;
    }

    /// <summary>
    /// Sets time spent viewing vehicle
    /// </summary>
    public void SetTimeSpent(int seconds)
    {
        TimeSpentSeconds = seconds;
    }

    /// <summary>
    /// Checks if user showed high intent (engaged deeply)
    /// </summary>
    public bool IsHighIntent() => 
        (ViewedImages || ViewedSpecs) && 
        TimeSpentSeconds.HasValue && 
        TimeSpentSeconds.Value > 60;

    /// <summary>
    /// Checks if user converted (clicked contact or added to favorites)
    /// </summary>
    public bool IsConverted() => ClickedContact || AddedToFavorites;
}

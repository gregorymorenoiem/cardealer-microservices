namespace EventTrackingService.Domain.Entities;

/// <summary>
/// Base entity for all tracked events in the system
/// </summary>
public class TrackedEvent
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty; // "page_view", "search", "filter_apply", "vehicle_view", "favorite_add", "contact_click", etc.
    public DateTime Timestamp { get; set; }
    public Guid? UserId { get; set; } // Nullable for anonymous users
    public string? SessionId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referrer { get; set; }
    public string? CurrentUrl { get; set; }
    public string? DeviceType { get; set; } // "mobile", "tablet", "desktop"
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    
    // Event-specific data stored as JSON
    public string EventData { get; set; } = "{}"; // JSON string with event-specific properties
    
    // Metadata
    public string? Source { get; set; } // "web", "mobile_app", "api"
    public string? Campaign { get; set; } // UTM campaign
    public string? Medium { get; set; } // UTM medium
    public string? Content { get; set; } // UTM content
    
    public TrackedEvent()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if event is from authenticated user
    /// </summary>
    public bool IsAuthenticated() => UserId.HasValue;

    /// <summary>
    /// Checks if event is from mobile device
    /// </summary>
    public bool IsMobile() => DeviceType?.ToLower() == "mobile";

    /// <summary>
    /// Checks if event is from paid campaign
    /// </summary>
    public bool IsFromCampaign() => !string.IsNullOrEmpty(Campaign);

    /// <summary>
    /// Gets time since event occurred
    /// </summary>
    public TimeSpan GetAge() => DateTime.UtcNow - Timestamp;

    /// <summary>
    /// Checks if event is recent (within last hour)
    /// </summary>
    public bool IsRecent() => GetAge().TotalHours < 1;
}

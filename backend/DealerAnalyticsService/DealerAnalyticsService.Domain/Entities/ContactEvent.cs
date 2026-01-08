namespace DealerAnalyticsService.Domain.Entities;

/// <summary>
/// Tracks contact button clicks (phone, email, WhatsApp, website)
/// </summary>
public class ContactEvent
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public DateTime ClickedAt { get; set; }
    public ContactType ContactType { get; set; }
    public string? ViewerIpAddress { get; set; }
    public Guid? ViewerUserId { get; set; }
    public string? ContactValue { get; set; } // Phone number, email, etc.
    public Guid? VehicleId { get; set; } // If clicked from vehicle listing
    public string? Source { get; set; } // "profile", "vehicle_detail", "search_results"
    public string? DeviceType { get; set; }
    public bool ConvertedToInquiry { get; set; } // Did they submit an inquiry?
    public DateTime? ConversionDate { get; set; }

    public ContactEvent()
    {
        Id = Guid.NewGuid();
        ClickedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks this contact as converted to inquiry
    /// </summary>
    public void MarkAsConverted()
    {
        ConvertedToInquiry = true;
        ConversionDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets time to conversion (click to inquiry submission)
    /// </summary>
    public TimeSpan? GetTimeToConversion()
    {
        if (!ConvertedToInquiry || ConversionDate == null) return null;
        return ConversionDate.Value - ClickedAt;
    }

    /// <summary>
    /// Checks if conversion happened within X minutes
    /// </summary>
    public bool IsQuickConversion(int minutes = 30)
    {
        var timeToConversion = GetTimeToConversion();
        return timeToConversion.HasValue && timeToConversion.Value.TotalMinutes <= minutes;
    }
}

public enum ContactType
{
    Phone = 1,
    Email = 2,
    WhatsApp = 3,
    Website = 4,
    SocialMedia = 5
}

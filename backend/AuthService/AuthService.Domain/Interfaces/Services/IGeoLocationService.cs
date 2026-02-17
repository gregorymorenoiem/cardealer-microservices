namespace AuthService.Domain.Interfaces.Services;

/// <summary>
/// Service for getting geographic location from IP addresses.
/// Used for security features like new session notifications.
/// </summary>
public interface IGeoLocationService
{
    /// <summary>
    /// Gets location information from an IP address.
    /// </summary>
    /// <param name="ipAddress">The IP address to lookup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Location information or null if lookup fails</returns>
    Task<GeoLocationResult?> GetLocationFromIpAsync(string ipAddress, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from IP geolocation lookup
/// </summary>
public record GeoLocationResult(
    string Country,
    string CountryCode,
    string City,
    string Region,
    string Timezone,
    double? Latitude,
    double? Longitude,
    string Isp
)
{
    /// <summary>
    /// Gets a formatted location string like "Santo Domingo, Dominican Republic"
    /// </summary>
    public string GetFormattedLocation()
    {
        if (!string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(Country))
            return $"{City}, {Country}";
        if (!string.IsNullOrEmpty(Country))
            return Country;
        if (!string.IsNullOrEmpty(City))
            return City;
        return "Unknown location";
    }
}

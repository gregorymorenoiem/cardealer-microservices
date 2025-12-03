namespace Gateway.Domain.Entities;

/// <summary>
/// Represents a route configuration in the gateway
/// </summary>
public class Route
{
    public string DownstreamPathTemplate { get; set; } = string.Empty;
    public string UpstreamPathTemplate { get; set; } = string.Empty;
    public string[] UpstreamHttpMethod { get; set; } = Array.Empty<string>();
    public string DownstreamScheme { get; set; } = "http";
    public string DownstreamHostAndPort { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public RouteOptions Options { get; set; } = new();
}

/// <summary>
/// Route-specific options
/// </summary>
public class RouteOptions
{
    public bool RequiresAuthentication { get; set; }
    public string[] AllowedRoles { get; set; } = Array.Empty<string>();
    public int TimeoutMs { get; set; } = 30000;
    public RateLimitOptions? RateLimit { get; set; }
}

/// <summary>
/// Rate limiting configuration
/// </summary>
public class RateLimitOptions
{
    public int Limit { get; set; }
    public int PeriodSeconds { get; set; }
}

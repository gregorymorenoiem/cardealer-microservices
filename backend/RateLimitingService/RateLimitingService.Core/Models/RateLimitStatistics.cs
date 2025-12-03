namespace RateLimitingService.Core.Models;

/// <summary>
/// Statistics for rate limiting
/// </summary>
public class RateLimitStatistics
{
    /// <summary>
    /// Total number of requests processed
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// Number of allowed requests
    /// </summary>
    public long AllowedRequests { get; set; }

    /// <summary>
    /// Number of blocked requests
    /// </summary>
    public long BlockedRequests { get; set; }

    /// <summary>
    /// Statistics per client
    /// </summary>
    public List<ClientStatistics> ClientStats { get; set; } = new();

    /// <summary>
    /// Statistics per endpoint
    /// </summary>
    public List<EndpointStatistics> EndpointStats { get; set; } = new();

    /// <summary>
    /// Time period for statistics
    /// </summary>
    public DateTime From { get; set; }

    /// <summary>
    /// Time period for statistics
    /// </summary>
    public DateTime To { get; set; }

    /// <summary>
    /// Block rate percentage
    /// </summary>
    public double BlockRate => TotalRequests > 0 ? (double)BlockedRequests / TotalRequests * 100 : 0;

    // Helper properties for compatibility
    public long TotalAllowed => AllowedRequests;
    public long TotalBlocked => BlockedRequests;
    public long UniqueClients => ClientStats.Count;
}

/// <summary>
/// Statistics for a specific client
/// </summary>
public class ClientStatistics
{
    /// <summary>
    /// Client identifier
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client tier
    /// </summary>
    public string Tier { get; set; } = string.Empty;

    /// <summary>
    /// Total requests from this client
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// Blocked requests for this client
    /// </summary>
    public long BlockedRequests { get; set; }

    /// <summary>
    /// Current usage in the window
    /// </summary>
    public int CurrentUsage { get; set; }

    /// <summary>
    /// Maximum allowed requests
    /// </summary>
    public int MaxAllowed { get; set; }

    /// <summary>
    /// Window reset time
    /// </summary>
    public DateTime ResetAt { get; set; }

    /// <summary>
    /// Usage percentage
    /// </summary>
    public double UsagePercentage => MaxAllowed > 0 ? (double)CurrentUsage / MaxAllowed * 100 : 0;
}

/// <summary>
/// Statistics for a specific endpoint
/// </summary>
public class EndpointStatistics
{
    /// <summary>
    /// Endpoint path
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Total requests to this endpoint
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// Blocked requests for this endpoint
    /// </summary>
    public long BlockedRequests { get; set; }

    /// <summary>
    /// Average requests per minute
    /// </summary>
    public double AvgRequestsPerMinute { get; set; }
}

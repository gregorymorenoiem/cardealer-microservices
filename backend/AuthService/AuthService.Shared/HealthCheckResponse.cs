namespace AuthService.Shared;

/// <summary>
/// Health check response model
/// </summary>
public class HealthCheckResponse
{
    /// <summary>Overall status of the service</summary>
    public string Status { get; set; } = "Healthy";

    /// <summary>Service name</summary>
    public string Service { get; set; } = "AuthService";

    /// <summary>Timestamp of the health check</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Service version</summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>Environment name</summary>
    public string Environment { get; set; } = "Development";

    /// <summary>Detailed health checks for dependencies</summary>
    public Dictionary<string, string> Checks { get; set; } = new();

    /// <summary>Uptime of the service</summary>
    public TimeSpan Uptime { get; set; }

    /// <summary>Memory usage information</summary>
    public MemoryUsage Memory { get; set; } = new();
}

/// <summary>
/// Memory usage information
/// </summary>
public class MemoryUsage
{
    /// <summary>Total allocated memory in MB</summary>
    public double Allocated { get; set; }

    /// <summary>Working set in MB</summary>
    public double WorkingSet { get; set; }

    /// <summary>GC collections count</summary>
    public long GarbageCollections { get; set; }
}

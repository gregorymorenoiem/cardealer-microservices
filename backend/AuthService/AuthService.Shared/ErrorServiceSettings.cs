namespace AuthService.Shared;

/// <summary>
/// Error Service configuration for centralized error logging
/// </summary>
public class ErrorServiceSettings
{
    /// <summary>Base URL of the Error Service</summary>
    public string BaseUrl { get; set; } = "http://errorservice:80";

    /// <summary>Request timeout in seconds</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Whether to enable error logging to the service</summary>
    public bool EnableErrorLogging { get; set; } = true;

    /// <summary>Service name to identify the source of errors</summary>
    public string ServiceName { get; set; } = "AuthService";

    /// <summary>Environment name (Development, Staging, Production)</summary>
    public string Environment { get; set; } = "Development";
}

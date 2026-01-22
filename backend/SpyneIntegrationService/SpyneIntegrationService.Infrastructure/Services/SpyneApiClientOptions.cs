namespace SpyneIntegrationService.Infrastructure.Services;

/// <summary>
/// Configuration options for Spyne API client
/// Spyne API Documentation: https://docs.spyne.ai
/// </summary>
public class SpyneApiClientOptions
{
    public const string SectionName = "SpyneApi";
    
    /// <summary>Base URL for Spyne API (clippr.ai)</summary>
    public string BaseUrl { get; set; } = "https://www.clippr.ai/api";
    
    /// <summary>API Key (auth_key) for authentication - Get from Spyne Dashboard</summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>Webhook URL for callbacks when processing completes</summary>
    public string WebhookUrl { get; set; } = string.Empty;
    
    /// <summary>Timeout in seconds for API calls</summary>
    public int TimeoutSeconds { get; set; } = 120;
    
    /// <summary>Maximum retry attempts</summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>Retry count for failed requests</summary>
    public int RetryCount { get; set; } = 3;
    
    /// <summary>Enable detailed logging</summary>
    public bool EnableDetailedLogging { get; set; } = false;
}

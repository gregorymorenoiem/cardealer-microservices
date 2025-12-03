namespace ApiDocsService.Core.Models;

/// <summary>
/// API version information
/// </summary>
public class ApiVersion
{
    public string Version { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDeprecated { get; set; }
    public DateTime? DeprecationDate { get; set; }
    public DateTime? SunsetDate { get; set; }
    public string? MigrationGuideUrl { get; set; }
    public List<string> BreakingChanges { get; set; } = new();
}

/// <summary>
/// Versioned service information
/// </summary>
public class VersionedServiceInfo
{
    public string ServiceName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<ApiVersion> Versions { get; set; } = new();
    public string CurrentVersion { get; set; } = string.Empty;
    public string LatestVersion { get; set; } = string.Empty;
}

/// <summary>
/// Version comparison result
/// </summary>
public class VersionComparison
{
    public string ServiceName { get; set; } = string.Empty;
    public string FromVersion { get; set; } = string.Empty;
    public string ToVersion { get; set; } = string.Empty;
    public List<EndpointChange> Changes { get; set; } = new();
    public List<string> AddedEndpoints { get; set; } = new();
    public List<string> RemovedEndpoints { get; set; } = new();
    public List<string> ModifiedEndpoints { get; set; } = new();
}

/// <summary>
/// Endpoint change details
/// </summary>
public class EndpointChange
{
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public ChangeType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsBreaking { get; set; }
}

/// <summary>
/// Type of change in API
/// </summary>
public enum ChangeType
{
    Added,
    Removed,
    Modified,
    Deprecated
}

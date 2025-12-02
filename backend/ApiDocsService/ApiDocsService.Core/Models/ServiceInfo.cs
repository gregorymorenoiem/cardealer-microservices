namespace ApiDocsService.Core.Models;

/// <summary>
/// Information about a registered microservice
/// </summary>
public class ServiceInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string SwaggerUrl { get; set; } = string.Empty;
    public string Version { get; set; } = "v1";
    public string Category { get; set; } = "General";
    public bool IsHealthy { get; set; }
    public DateTime? LastChecked { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? IconUrl { get; set; }
    public int Order { get; set; } = 100;
}

/// <summary>
/// Status of a service
/// </summary>
public class ServiceStatus
{
    public string ServiceName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? HealthStatus { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public int? ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Aggregated API documentation
/// </summary>
public class AggregatedApiDoc
{
    public string ServiceName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? OpenApiSpec { get; set; }
    public List<ApiEndpointInfo> Endpoints { get; set; } = new();
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
    public bool IsAvailable { get; set; }
}

/// <summary>
/// Information about an API endpoint
/// </summary>
public class ApiEndpointInfo
{
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool RequiresAuth { get; set; }
    public string? OperationId { get; set; }
}

/// <summary>
/// Dashboard statistics
/// </summary>
public class ApiDocsDashboard
{
    public int TotalServices { get; set; }
    public int HealthyServices { get; set; }
    public int UnhealthyServices { get; set; }
    public int TotalEndpoints { get; set; }
    public List<CategoryStats> ByCategory { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Category statistics
/// </summary>
public class CategoryStats
{
    public string Category { get; set; } = string.Empty;
    public int ServiceCount { get; set; }
    public int EndpointCount { get; set; }
}

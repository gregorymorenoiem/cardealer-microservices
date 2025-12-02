using ApiDocsService.Core.Models;

namespace ApiDocsService.Core.Interfaces;

/// <summary>
/// Service for aggregating API documentation from multiple microservices
/// </summary>
public interface IApiAggregatorService
{
    /// <summary>
    /// Get all registered services
    /// </summary>
    Task<List<ServiceInfo>> GetAllServicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get service by name
    /// </summary>
    Task<ServiceInfo?> GetServiceByNameAsync(string serviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get services by category
    /// </summary>
    Task<List<ServiceInfo>> GetServicesByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check health status of all services
    /// </summary>
    Task<List<ServiceStatus>> CheckAllServicesHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check health status of a specific service
    /// </summary>
    Task<ServiceStatus> CheckServiceHealthAsync(string serviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get OpenAPI spec for a service
    /// </summary>
    Task<string?> GetOpenApiSpecAsync(string serviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get aggregated documentation for all services
    /// </summary>
    Task<List<AggregatedApiDoc>> GetAllApiDocsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    Task<ApiDocsDashboard> GetDashboardAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Search endpoints across all services
    /// </summary>
    Task<List<ApiEndpointInfo>> SearchEndpointsAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh cached documentation
    /// </summary>
    Task RefreshAllDocsAsync(CancellationToken cancellationToken = default);
}

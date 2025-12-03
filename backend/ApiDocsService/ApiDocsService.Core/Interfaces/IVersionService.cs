using ApiDocsService.Core.Models;

namespace ApiDocsService.Core.Interfaces;

/// <summary>
/// Service for managing API versions
/// </summary>
public interface IVersionService
{
    /// <summary>
    /// Get all versions of a service
    /// </summary>
    Task<VersionedServiceInfo?> GetServiceVersionsAsync(string serviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all versioned services
    /// </summary>
    Task<List<VersionedServiceInfo>> GetAllVersionedServicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Compare two versions of a service
    /// </summary>
    Task<VersionComparison?> CompareVersionsAsync(string serviceName, string fromVersion, string toVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get deprecated APIs across all services
    /// </summary>
    Task<List<ApiVersion>> GetDeprecatedApisAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a specific version is deprecated
    /// </summary>
    Task<bool> IsVersionDeprecatedAsync(string serviceName, string version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get migration path from one version to another
    /// </summary>
    Task<List<string>> GetMigrationPathAsync(string serviceName, string fromVersion, string toVersion, CancellationToken cancellationToken = default);
}

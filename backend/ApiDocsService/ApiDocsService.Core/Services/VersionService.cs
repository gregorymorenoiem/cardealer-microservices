using ApiDocsService.Core.Interfaces;
using ApiDocsService.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ApiDocsService.Core.Services;

/// <summary>
/// Implementation of version management service
/// </summary>
public class VersionService : IVersionService
{
    private readonly IApiAggregatorService _aggregatorService;
    private readonly ILogger<VersionService> _logger;
    private readonly Dictionary<string, VersionedServiceInfo> _versionCache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public VersionService(
        IApiAggregatorService aggregatorService,
        ILogger<VersionService> logger)
    {
        _aggregatorService = aggregatorService;
        _logger = logger;
    }

    public async Task<VersionedServiceInfo?> GetServiceVersionsAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        await EnsureCacheInitialized(cancellationToken);

        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            return _versionCache.GetValueOrDefault(serviceName);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<List<VersionedServiceInfo>> GetAllVersionedServicesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureCacheInitialized(cancellationToken);

        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            return _versionCache.Values.ToList();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<VersionComparison?> CompareVersionsAsync(
        string serviceName,
        string fromVersion,
        string toVersion,
        CancellationToken cancellationToken = default)
    {
        var versionInfo = await GetServiceVersionsAsync(serviceName, cancellationToken);
        if (versionInfo == null)
        {
            _logger.LogWarning("Service {ServiceName} not found for version comparison", serviceName);
            return null;
        }

        var comparison = new VersionComparison
        {
            ServiceName = serviceName,
            FromVersion = fromVersion,
            ToVersion = toVersion
        };

        try
        {
            // Get OpenAPI specs for both versions
            var fromSpec = await _aggregatorService.GetOpenApiSpecAsync($"{serviceName}/{fromVersion}", cancellationToken);
            var toSpec = await _aggregatorService.GetOpenApiSpecAsync($"{serviceName}/{toVersion}", cancellationToken);

            if (fromSpec != null && toSpec != null)
            {
                var fromEndpoints = ParseEndpointsFromSpec(fromSpec);
                var toEndpoints = ParseEndpointsFromSpec(toSpec);

                // Find added endpoints
                comparison.AddedEndpoints = toEndpoints
                    .Where(e => !fromEndpoints.Contains(e))
                    .ToList();

                // Find removed endpoints
                comparison.RemovedEndpoints = fromEndpoints
                    .Where(e => !toEndpoints.Contains(e))
                    .ToList();

                // Find modified endpoints (same path but different content)
                comparison.ModifiedEndpoints = fromEndpoints
                    .Intersect(toEndpoints)
                    .Where(e => HasEndpointChanged(e, fromSpec, toSpec))
                    .ToList();

                // Create detailed changes
                foreach (var added in comparison.AddedEndpoints)
                {
                    comparison.Changes.Add(new EndpointChange
                    {
                        Path = added,
                        Type = ChangeType.Added,
                        Description = $"New endpoint added in {toVersion}"
                    });
                }

                foreach (var removed in comparison.RemovedEndpoints)
                {
                    comparison.Changes.Add(new EndpointChange
                    {
                        Path = removed,
                        Type = ChangeType.Removed,
                        Description = $"Endpoint removed in {toVersion}",
                        IsBreaking = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing versions for {ServiceName}", serviceName);
        }

        return comparison;
    }

    public async Task<List<ApiVersion>> GetDeprecatedApisAsync(CancellationToken cancellationToken = default)
    {
        var allVersions = await GetAllVersionedServicesAsync(cancellationToken);

        return allVersions
            .SelectMany(s => s.Versions)
            .Where(v => v.IsDeprecated)
            .OrderBy(v => v.DeprecationDate)
            .ToList();
    }

    public async Task<bool> IsVersionDeprecatedAsync(string serviceName, string version, CancellationToken cancellationToken = default)
    {
        var versionInfo = await GetServiceVersionsAsync(serviceName, cancellationToken);
        if (versionInfo == null) return false;

        var apiVersion = versionInfo.Versions.FirstOrDefault(v => v.Version == version);
        return apiVersion?.IsDeprecated ?? false;
    }

    public async Task<List<string>> GetMigrationPathAsync(
        string serviceName,
        string fromVersion,
        string toVersion,
        CancellationToken cancellationToken = default)
    {
        var versionInfo = await GetServiceVersionsAsync(serviceName, cancellationToken);
        if (versionInfo == null) return new List<string>();

        var path = new List<string> { fromVersion };

        // Simple sequential path for now
        var versions = versionInfo.Versions
            .OrderBy(v => v.Version)
            .Select(v => v.Version)
            .ToList();

        var fromIndex = versions.IndexOf(fromVersion);
        var toIndex = versions.IndexOf(toVersion);

        if (fromIndex == -1 || toIndex == -1)
            return new List<string>();

        if (toIndex > fromIndex)
        {
            for (int i = fromIndex + 1; i <= toIndex; i++)
            {
                path.Add(versions[i]);
            }
        }
        else
        {
            for (int i = fromIndex - 1; i >= toIndex; i--)
            {
                path.Add(versions[i]);
            }
        }

        return path;
    }

    private async Task EnsureCacheInitialized(CancellationToken cancellationToken)
    {
        if (_versionCache.Any()) return;

        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            if (_versionCache.Any()) return;

            var services = await _aggregatorService.GetAllServicesAsync(cancellationToken);

            foreach (var service in services)
            {
                var versionedInfo = new VersionedServiceInfo
                {
                    ServiceName = service.Name,
                    DisplayName = service.DisplayName,
                    CurrentVersion = service.Version,
                    LatestVersion = service.Version,
                    Versions = new List<ApiVersion>
                    {
                        new()
                        {
                            Version = service.Version,
                            Title = $"{service.DisplayName} {service.Version}",
                            Description = service.Description,
                            IsDeprecated = false
                        }
                    }
                };

                _versionCache[service.Name] = versionedInfo;
            }

            _logger.LogInformation("Initialized version cache with {Count} services", _versionCache.Count);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private List<string> ParseEndpointsFromSpec(string spec)
    {
        var endpoints = new List<string>();

        try
        {
            using var doc = JsonDocument.Parse(spec);
            var root = doc.RootElement;

            if (root.TryGetProperty("paths", out var paths))
            {
                foreach (var path in paths.EnumerateObject())
                {
                    foreach (var method in path.Value.EnumerateObject())
                    {
                        endpoints.Add($"{method.Name.ToUpper()} {path.Name}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse endpoints from OpenAPI spec");
        }

        return endpoints;
    }

    private bool HasEndpointChanged(string endpoint, string fromSpec, string toSpec)
    {
        // Simplified comparison - could be enhanced with schema comparison
        return false;
    }
}

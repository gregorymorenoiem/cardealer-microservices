using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace CarDealer.Shared.HealthChecks.Checks;

/// <summary>
/// Health Check customizado para verificar disponibilidad de memoria
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly long _maxMemoryBytes;

    public MemoryHealthCheck(long maxMemoryBytes = 1024L * 1024L * 1024L) // 1GB default
    {
        _maxMemoryBytes = maxMemoryBytes;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var allocatedBytes = GC.GetTotalMemory(forceFullCollection: false);
        var data = new Dictionary<string, object>
        {
            { "allocated_bytes", allocatedBytes },
            { "allocated_mb", allocatedBytes / (1024 * 1024) },
            { "threshold_mb", _maxMemoryBytes / (1024 * 1024) },
            { "gen0_collections", GC.CollectionCount(0) },
            { "gen1_collections", GC.CollectionCount(1) },
            { "gen2_collections", GC.CollectionCount(2) }
        };

        var status = allocatedBytes < _maxMemoryBytes
            ? HealthStatus.Healthy
            : HealthStatus.Degraded;

        return Task.FromResult(new HealthCheckResult(
            status,
            $"Allocated memory: {allocatedBytes / (1024 * 1024)} MB / {_maxMemoryBytes / (1024 * 1024)} MB threshold",
            data: data));
    }
}

/// <summary>
/// Health Check customizado para verificar tiempo de uptime
/// </summary>
public class UptimeHealthCheck : IHealthCheck
{
    private static readonly DateTime StartTime = DateTime.UtcNow;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var uptime = DateTime.UtcNow - StartTime;
        var data = new Dictionary<string, object>
        {
            { "start_time", StartTime.ToString("O") },
            { "uptime_seconds", (long)uptime.TotalSeconds },
            { "uptime_formatted", uptime.ToString(@"dd\.hh\:mm\:ss") }
        };

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Uptime: {uptime.ToString(@"dd\.hh\:mm\:ss")}",
            data: data));
    }
}

/// <summary>
/// Health Check customizado para verificar versión de la aplicación
/// </summary>
public class VersionHealthCheck : IHealthCheck
{
    private readonly string _serviceName;
    private readonly string _version;
    private readonly string _environment;

    public VersionHealthCheck(string serviceName, string version, string environment)
    {
        _serviceName = serviceName;
        _version = version;
        _environment = environment;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "service_name", _serviceName },
            { "version", _version },
            { "environment", _environment },
            { "dotnet_version", Environment.Version.ToString() },
            { "os_description", System.Runtime.InteropServices.RuntimeInformation.OSDescription }
        };

        return Task.FromResult(HealthCheckResult.Healthy(
            $"{_serviceName} v{_version} ({_environment})",
            data: data));
    }
}

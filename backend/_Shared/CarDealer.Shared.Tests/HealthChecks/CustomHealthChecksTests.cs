using CarDealer.Shared.HealthChecks.Checks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CarDealer.Shared.Tests.HealthChecks;

/// <summary>
/// Tests for custom health checks: Memory, Uptime, Version.
/// </summary>
public class CustomHealthChecksTests
{
    // ========================================
    // MemoryHealthCheck
    // ========================================

    [Fact]
    public async Task MemoryHealthCheck_WhenBelowThreshold_ReturnsHealthy()
    {
        // 10 GB threshold — should always be healthy in tests
        var check = new MemoryHealthCheck(10L * 1024L * 1024L * 1024L);

        var result = await check.CheckHealthAsync(
            new HealthCheckContext { Registration = new HealthCheckRegistration("memory", check, null, null) });

        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("allocated_bytes");
        result.Data.Should().ContainKey("allocated_mb");
        result.Data.Should().ContainKey("threshold_mb");
        result.Data.Should().ContainKey("gen0_collections");
    }

    [Fact]
    public async Task MemoryHealthCheck_WhenAboveThreshold_ReturnsDegraded()
    {
        // 1 byte threshold — should always be degraded
        var check = new MemoryHealthCheck(1);

        var result = await check.CheckHealthAsync(
            new HealthCheckContext { Registration = new HealthCheckRegistration("memory", check, null, null) });

        result.Status.Should().Be(HealthStatus.Degraded);
    }

    [Fact]
    public async Task MemoryHealthCheck_DefaultThreshold_Is1GB()
    {
        var check = new MemoryHealthCheck();

        var result = await check.CheckHealthAsync(
            new HealthCheckContext { Registration = new HealthCheckRegistration("memory", check, null, null) });

        result.Data["threshold_mb"].Should().Be(1024L);
    }

    // ========================================
    // UptimeHealthCheck
    // ========================================

    [Fact]
    public async Task UptimeHealthCheck_ReturnsHealthyWithUptime()
    {
        var check = new UptimeHealthCheck();

        var result = await check.CheckHealthAsync(
            new HealthCheckContext { Registration = new HealthCheckRegistration("uptime", check, null, null) });

        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("start_time");
        result.Data.Should().ContainKey("uptime_seconds");
        result.Data.Should().ContainKey("uptime_formatted");
    }

    // ========================================
    // VersionHealthCheck
    // ========================================

    [Fact]
    public async Task VersionHealthCheck_ReturnsServiceInfo()
    {
        var check = new VersionHealthCheck("AuthService", "1.2.3", "Production");

        var result = await check.CheckHealthAsync(
            new HealthCheckContext { Registration = new HealthCheckRegistration("version", check, null, null) });

        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data["service_name"].Should().Be("AuthService");
        result.Data["version"].Should().Be("1.2.3");
        result.Data["environment"].Should().Be("Production");
        result.Data.Should().ContainKey("dotnet_version");
        result.Data.Should().ContainKey("os_description");
        result.Description.Should().Contain("AuthService").And.Contain("1.2.3");
    }
}

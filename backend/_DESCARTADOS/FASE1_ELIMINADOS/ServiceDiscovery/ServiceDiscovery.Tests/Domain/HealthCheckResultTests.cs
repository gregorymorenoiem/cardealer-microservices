using FluentAssertions;
using ServiceDiscovery.Domain.Entities;
using ServiceDiscovery.Domain.Enums;
using Xunit;

namespace ServiceDiscovery.Tests.Domain;

public class HealthCheckResultTests
{
    [Fact]
    public void Healthy_CreatesHealthyResult()
    {
        // Arrange & Act
        var result = HealthCheckResult.Healthy("auth-001", 150, 200);

        // Assert
        result.InstanceId.Should().Be("auth-001");
        result.Status.Should().Be(HealthStatus.Healthy);
        result.ResponseTimeMs.Should().Be(150);
        result.StatusCode.Should().Be(200);
        result.Message.Should().Contain("healthy");
    }

    [Fact]
    public void Unhealthy_CreatesUnhealthyResult()
    {
        // Arrange & Act
        var result = HealthCheckResult.Unhealthy("auth-001", "Connection refused");

        // Assert
        result.InstanceId.Should().Be("auth-001");
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Error.Should().Be("Connection refused");
        result.Message.Should().Contain("unhealthy");
    }

    [Fact]
    public void Degraded_CreatesDegradedResult()
    {
        // Arrange & Act
        var result = HealthCheckResult.Degraded("auth-001", "Slow response time");

        // Assert
        result.InstanceId.Should().Be("auth-001");
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Message.Should().Be("Slow response time");
    }

    [Fact]
    public void CheckedAt_IsSetToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var result = HealthCheckResult.Healthy("auth-001", 100, 200);

        // Assert
        result.CheckedAt.Should().BeAfter(before);
        result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}

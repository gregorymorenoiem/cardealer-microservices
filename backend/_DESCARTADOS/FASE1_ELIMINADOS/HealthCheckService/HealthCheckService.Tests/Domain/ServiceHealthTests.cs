using HealthCheckService.Domain.Entities;
using HealthCheckService.Domain.Enums;
using Xunit;

namespace HealthCheckService.Tests.Domain;

public class ServiceHealthTests
{
    [Fact]
    public void IsHealthy_ShouldReturnTrue_WhenStatusIsHealthy()
    {
        // Arrange
        var serviceHealth = new ServiceHealth { Status = HealthStatus.Healthy };

        // Act & Assert
        Assert.True(serviceHealth.IsHealthy());
        Assert.False(serviceHealth.IsDegraded());
        Assert.False(serviceHealth.IsUnhealthy());
    }

    [Fact]
    public void IsDegraded_ShouldReturnTrue_WhenStatusIsDegraded()
    {
        // Arrange
        var serviceHealth = new ServiceHealth { Status = HealthStatus.Degraded };

        // Act & Assert
        Assert.False(serviceHealth.IsHealthy());
        Assert.True(serviceHealth.IsDegraded());
        Assert.False(serviceHealth.IsUnhealthy());
    }

    [Fact]
    public void IsUnhealthy_ShouldReturnTrue_WhenStatusIsUnhealthy()
    {
        // Arrange
        var serviceHealth = new ServiceHealth { Status = HealthStatus.Unhealthy };

        // Act & Assert
        Assert.False(serviceHealth.IsHealthy());
        Assert.False(serviceHealth.IsDegraded());
        Assert.True(serviceHealth.IsUnhealthy());
    }

    [Fact]
    public void CalculateOverallStatus_ShouldBeHealthy_WhenNoDependencies()
    {
        // Arrange
        var serviceHealth = new ServiceHealth
        {
            Status = HealthStatus.Healthy,
            Dependencies = new List<DependencyHealth>()
        };

        // Act
        var status = serviceHealth.CalculateOverallStatus();

        // Assert
        Assert.Equal(HealthStatus.Healthy, status);
    }

    [Fact]
    public void CalculateOverallStatus_ShouldBeUnhealthy_WhenDependencyUnhealthy()
    {
        // Arrange
        var serviceHealth = new ServiceHealth
        {
            Status = HealthStatus.Healthy,
            Dependencies = new List<DependencyHealth>
            {
                new() { Name = "Database", Status = HealthStatus.Healthy },
                new() { Name = "Cache", Status = HealthStatus.Unhealthy }
            }
        };

        // Act
        var status = serviceHealth.CalculateOverallStatus();

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, status);
    }

    [Fact]
    public void CalculateOverallStatus_ShouldBeDegraded_WhenDependencyDegraded()
    {
        // Arrange
        var serviceHealth = new ServiceHealth
        {
            Status = HealthStatus.Healthy,
            Dependencies = new List<DependencyHealth>
            {
                new() { Name = "Database", Status = HealthStatus.Healthy },
                new() { Name = "Cache", Status = HealthStatus.Degraded }
            }
        };

        // Act
        var status = serviceHealth.CalculateOverallStatus();

        // Assert
        Assert.Equal(HealthStatus.Degraded, status);
    }
}

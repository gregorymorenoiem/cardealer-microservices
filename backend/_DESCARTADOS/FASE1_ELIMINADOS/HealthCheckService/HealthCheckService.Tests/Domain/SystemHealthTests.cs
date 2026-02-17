using HealthCheckService.Domain.Entities;
using HealthCheckService.Domain.Enums;
using Xunit;

namespace HealthCheckService.Tests.Domain;

public class SystemHealthTests
{
    [Fact]
    public void CalculateOverallStatus_ShouldBeHealthy_WhenAllServicesHealthy()
    {
        // Arrange
        var systemHealth = new SystemHealth
        {
            Services = new List<ServiceHealth>
            {
                new() { ServiceName = "Service1", Status = HealthStatus.Healthy },
                new() { ServiceName = "Service2", Status = HealthStatus.Healthy },
                new() { ServiceName = "Service3", Status = HealthStatus.Healthy }
            }
        };

        // Act
        systemHealth.CalculateOverallStatus();

        // Assert
        Assert.Equal(HealthStatus.Healthy, systemHealth.OverallStatus);
        Assert.Equal(3, systemHealth.TotalServices);
        Assert.Equal(3, systemHealth.HealthyServices);
        Assert.Equal(0, systemHealth.DegradedServices);
        Assert.Equal(0, systemHealth.UnhealthyServices);
    }

    [Fact]
    public void CalculateOverallStatus_ShouldBeDegraded_WhenOneServiceDegraded()
    {
        // Arrange
        var systemHealth = new SystemHealth
        {
            Services = new List<ServiceHealth>
            {
                new() { ServiceName = "Service1", Status = HealthStatus.Healthy },
                new() { ServiceName = "Service2", Status = HealthStatus.Degraded },
                new() { ServiceName = "Service3", Status = HealthStatus.Healthy }
            }
        };

        // Act
        systemHealth.CalculateOverallStatus();

        // Assert
        Assert.Equal(HealthStatus.Degraded, systemHealth.OverallStatus);
        Assert.Equal(3, systemHealth.TotalServices);
        Assert.Equal(2, systemHealth.HealthyServices);
        Assert.Equal(1, systemHealth.DegradedServices);
        Assert.Equal(0, systemHealth.UnhealthyServices);
    }

    [Fact]
    public void CalculateOverallStatus_ShouldBeUnhealthy_WhenOneServiceUnhealthy()
    {
        // Arrange
        var systemHealth = new SystemHealth
        {
            Services = new List<ServiceHealth>
            {
                new() { ServiceName = "Service1", Status = HealthStatus.Healthy },
                new() { ServiceName = "Service2", Status = HealthStatus.Degraded },
                new() { ServiceName = "Service3", Status = HealthStatus.Unhealthy }
            }
        };

        // Act
        systemHealth.CalculateOverallStatus();

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, systemHealth.OverallStatus);
        Assert.Equal(3, systemHealth.TotalServices);
        Assert.Equal(1, systemHealth.HealthyServices);
        Assert.Equal(1, systemHealth.DegradedServices);
        Assert.Equal(1, systemHealth.UnhealthyServices);
    }

    [Fact]
    public void HealthPercentage_ShouldReturnCorrectValue()
    {
        // Arrange
        var systemHealth = new SystemHealth
        {
            Services = new List<ServiceHealth>
            {
                new() { ServiceName = "Service1", Status = HealthStatus.Healthy },
                new() { ServiceName = "Service2", Status = HealthStatus.Degraded },
                new() { ServiceName = "Service3", Status = HealthStatus.Healthy },
                new() { ServiceName = "Service4", Status = HealthStatus.Healthy }
            }
        };
        systemHealth.CalculateOverallStatus();

        // Act
        var percentage = systemHealth.HealthPercentage();

        // Assert
        Assert.Equal(75.0, percentage); // 3 out of 4 healthy
    }

    [Fact]
    public void IsFullyOperational_ShouldReturnTrue_WhenHealthy()
    {
        // Arrange
        var systemHealth = new SystemHealth { OverallStatus = HealthStatus.Healthy };

        // Act
        var result = systemHealth.IsFullyOperational();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasIssues_ShouldReturnTrue_WhenNotHealthy()
    {
        // Arrange
        var systemHealth = new SystemHealth { OverallStatus = HealthStatus.Degraded };

        // Act
        var result = systemHealth.HasIssues();

        // Assert
        Assert.True(result);
    }
}

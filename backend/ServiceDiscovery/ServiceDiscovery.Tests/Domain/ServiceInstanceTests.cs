using FluentAssertions;
using ServiceDiscovery.Domain.Entities;
using ServiceDiscovery.Domain.Enums;
using Xunit;

namespace ServiceDiscovery.Tests.Domain;

public class ServiceInstanceTests
{
    [Fact]
    public void IsValid_WithValidData_ReturnsTrue()
    {
        // Arrange
        var instance = new ServiceInstance
        {
            Id = "auth-001",
            ServiceName = "authservice",
            Host = "localhost",
            Port = 5001
        };

        // Act
        var result = instance.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyId_ReturnsFalse()
    {
        // Arrange
        var instance = new ServiceInstance
        {
            Id = "",
            ServiceName = "authservice",
            Host = "localhost",
            Port = 5001
        };

        // Act
        var result = instance.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithInvalidPort_ReturnsFalse()
    {
        // Arrange
        var instance = new ServiceInstance
        {
            Id = "auth-001",
            ServiceName = "authservice",
            Host = "localhost",
            Port = 0
        };

        // Act
        var result = instance.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsHealthy_WhenActiveAndHealthy_ReturnsTrue()
    {
        // Arrange
        var instance = new ServiceInstance
        {
            Id = "auth-001",
            ServiceName = "authservice",
            Host = "localhost",
            Port = 5001,
            Status = ServiceStatus.Active,
            HealthStatus = HealthStatus.Healthy
        };

        // Act
        var result = instance.IsHealthy();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsHealthy_WhenUnhealthy_ReturnsFalse()
    {
        // Arrange
        var instance = new ServiceInstance
        {
            Id = "auth-001",
            ServiceName = "authservice",
            Host = "localhost",
            Port = 5001,
            Status = ServiceStatus.Active,
            HealthStatus = HealthStatus.Unhealthy
        };

        // Act
        var result = instance.IsHealthy();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateHealth_UpdatesStatusAndTimestamp()
    {
        // Arrange
        var instance = new ServiceInstance
        {
            Id = "auth-001",
            ServiceName = "authservice",
            Host = "localhost",
            Port = 5001,
            HealthStatus = HealthStatus.Unknown
        };
        var beforeUpdate = DateTime.UtcNow;

        // Act
        instance.UpdateHealth(HealthStatus.Healthy);

        // Assert
        instance.HealthStatus.Should().Be(HealthStatus.Healthy);
        instance.LastCheckedAt.Should().BeAfter(beforeUpdate);
        instance.LastHealthyAt.Should().NotBeNull();
        instance.LastHealthyAt.Should().BeAfter(beforeUpdate);
    }

    [Fact]
    public void Address_ReturnsCorrectUrl()
    {
        // Arrange
        var instance = new ServiceInstance
        {
            Id = "auth-001",
            ServiceName = "authservice",
            Host = "192.168.1.10",
            Port = 5001
        };

        // Act
        var address = instance.Address;

        // Assert
        address.Should().Be("http://192.168.1.10:5001");
    }
}

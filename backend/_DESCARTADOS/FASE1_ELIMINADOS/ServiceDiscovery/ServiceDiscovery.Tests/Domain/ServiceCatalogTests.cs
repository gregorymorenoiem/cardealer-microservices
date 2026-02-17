using FluentAssertions;
using ServiceDiscovery.Domain.Entities;
using ServiceDiscovery.Domain.Enums;
using Xunit;

namespace ServiceDiscovery.Tests.Domain;

public class ServiceCatalogTests
{
    [Fact]
    public void RegisterService_AddsServiceToCatalog()
    {
        // Arrange
        var catalog = new ServiceCatalog();
        var instance = CreateValidInstance("auth-001", "authservice");

        // Act
        catalog.RegisterService(instance);

        // Assert
        catalog.GetServiceNames().Should().Contain("authservice");
        catalog.GetServiceInstances("authservice").Should().HaveCount(1);
    }

    [Fact]
    public void RegisterService_WithInvalidInstance_ThrowsException()
    {
        // Arrange
        var catalog = new ServiceCatalog();
        var instance = new ServiceInstance
        {
            Id = "",
            ServiceName = "authservice",
            Host = "localhost",
            Port = 5001
        };

        // Act & Assert
        Action act = () => catalog.RegisterService(instance);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RegisterService_WithSameId_ReplacesExisting()
    {
        // Arrange
        var catalog = new ServiceCatalog();
        var instance1 = CreateValidInstance("auth-001", "authservice");
        instance1.Port = 5001;
        var instance2 = CreateValidInstance("auth-001", "authservice");
        instance2.Port = 5002;

        // Act
        catalog.RegisterService(instance1);
        catalog.RegisterService(instance2);

        // Assert
        var instances = catalog.GetServiceInstances("authservice");
        instances.Should().HaveCount(1);
        instances[0].Port.Should().Be(5002);
    }

    [Fact]
    public void DeregisterService_RemovesServiceFromCatalog()
    {
        // Arrange
        var catalog = new ServiceCatalog();
        var instance = CreateValidInstance("auth-001", "authservice");
        catalog.RegisterService(instance);

        // Act
        var result = catalog.DeregisterService("auth-001");

        // Assert
        result.Should().BeTrue();
        catalog.GetServiceInstances("authservice").Should().BeEmpty();
    }

    [Fact]
    public void DeregisterService_WithNonExistentId_ReturnsFalse()
    {
        // Arrange
        var catalog = new ServiceCatalog();

        // Act
        var result = catalog.DeregisterService("non-existent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHealthyInstances_ReturnsOnlyHealthyServices()
    {
        // Arrange
        var catalog = new ServiceCatalog();
        var healthy1 = CreateValidInstance("auth-001", "authservice");
        healthy1.HealthStatus = HealthStatus.Healthy;
        healthy1.Status = ServiceStatus.Active;
        
        var healthy2 = CreateValidInstance("auth-002", "authservice");
        healthy2.HealthStatus = HealthStatus.Healthy;
        healthy2.Status = ServiceStatus.Active;
        
        var unhealthy = CreateValidInstance("auth-003", "authservice");
        unhealthy.HealthStatus = HealthStatus.Unhealthy;

        catalog.RegisterService(healthy1);
        catalog.RegisterService(healthy2);
        catalog.RegisterService(unhealthy);

        // Act
        var healthyInstances = catalog.GetHealthyInstances("authservice");

        // Assert
        healthyInstances.Should().HaveCount(2);
        healthyInstances.Should().Contain(i => i.Id == "auth-001");
        healthyInstances.Should().Contain(i => i.Id == "auth-002");
    }

    [Fact]
    public void GetInstance_ReturnsCorrectInstance()
    {
        // Arrange
        var catalog = new ServiceCatalog();
        var instance = CreateValidInstance("auth-001", "authservice");
        catalog.RegisterService(instance);

        // Act
        var result = catalog.GetInstance("auth-001");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("auth-001");
        result.ServiceName.Should().Be("authservice");
    }

    [Fact]
    public void GetInstance_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var catalog = new ServiceCatalog();

        // Act
        var result = catalog.GetInstance("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTotalInstanceCount_ReturnsCorrectCount()
    {
        // Arrange
        var catalog = new ServiceCatalog();
        catalog.RegisterService(CreateValidInstance("auth-001", "authservice"));
        catalog.RegisterService(CreateValidInstance("auth-002", "authservice"));
        catalog.RegisterService(CreateValidInstance("user-001", "userservice"));

        // Act
        var count = catalog.GetTotalInstanceCount();

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public void GetHealthyInstanceCount_ReturnsCorrectCount()
    {
        // Arrange
        var catalog = new ServiceCatalog();
        var healthy1 = CreateValidInstance("auth-001", "authservice");
        healthy1.HealthStatus = HealthStatus.Healthy;
        healthy1.Status = ServiceStatus.Active;
        
        var healthy2 = CreateValidInstance("user-001", "userservice");
        healthy2.HealthStatus = HealthStatus.Healthy;
        healthy2.Status = ServiceStatus.Active;
        
        var unhealthy = CreateValidInstance("auth-002", "authservice");
        unhealthy.HealthStatus = HealthStatus.Unhealthy;

        catalog.RegisterService(healthy1);
        catalog.RegisterService(healthy2);
        catalog.RegisterService(unhealthy);

        // Act
        var count = catalog.GetHealthyInstanceCount();

        // Assert
        count.Should().Be(2);
    }

    private ServiceInstance CreateValidInstance(string id, string serviceName)
    {
        return new ServiceInstance
        {
            Id = id,
            ServiceName = serviceName,
            Host = "localhost",
            Port = 5000
        };
    }
}

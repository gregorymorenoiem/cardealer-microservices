using Consul;
using FluentAssertions;
using Gateway.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gateway.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for HealthCheckService
/// </summary>
public class HealthCheckServiceTests
{
    private readonly Mock<IConsulClient> _consulClientMock;
    private readonly Mock<ILogger<HealthCheckService>> _loggerMock;
    private readonly Mock<IHealthEndpoint> _healthEndpointMock;
    private readonly Mock<ICatalogEndpoint> _catalogEndpointMock;

    public HealthCheckServiceTests()
    {
        _consulClientMock = new Mock<IConsulClient>();
        _loggerMock = new Mock<ILogger<HealthCheckService>>();
        _healthEndpointMock = new Mock<IHealthEndpoint>();
        _catalogEndpointMock = new Mock<ICatalogEndpoint>();

        _consulClientMock.Setup(x => x.Health).Returns(_healthEndpointMock.Object);
        _consulClientMock.Setup(x => x.Catalog).Returns(_catalogEndpointMock.Object);
    }

    #region IsServiceHealthy Tests

    [Fact]
    public async Task IsServiceHealthy_WhenServiceHasHealthyInstances_ShouldReturnTrue()
    {
        // Arrange
        var serviceName = "UserService";
        var healthyServices = new ServiceEntry[]
        {
            new ServiceEntry { Service = new AgentService { Service = serviceName } }
        };

        var queryResult = new QueryResult<ServiceEntry[]>
        {
            Response = healthyServices
        };

        _healthEndpointMock
            .Setup(x => x.Service(serviceName, null, true, default))
            .ReturnsAsync(queryResult);

        var service = new HealthCheckService(_consulClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.IsServiceHealthy(serviceName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsServiceHealthy_WhenServiceHasNoInstances_ShouldReturnFalse()
    {
        // Arrange
        var serviceName = "UnhealthyService";
        var queryResult = new QueryResult<ServiceEntry[]>
        {
            Response = Array.Empty<ServiceEntry>()
        };

        _healthEndpointMock
            .Setup(x => x.Service(serviceName, null, true, default))
            .ReturnsAsync(queryResult);

        var service = new HealthCheckService(_consulClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.IsServiceHealthy(serviceName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsServiceHealthy_WhenResponseIsNull_ShouldReturnFalse()
    {
        // Arrange
        var serviceName = "NullService";
        var queryResult = new QueryResult<ServiceEntry[]>
        {
            Response = null!
        };

        _healthEndpointMock
            .Setup(x => x.Service(serviceName, null, true, default))
            .ReturnsAsync(queryResult);

        var service = new HealthCheckService(_consulClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.IsServiceHealthy(serviceName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsServiceHealthy_WhenConsulThrowsException_ShouldReturnFalse()
    {
        // Arrange
        var serviceName = "ExceptionService";

        _healthEndpointMock
            .Setup(x => x.Service(serviceName, null, true, default))
            .ThrowsAsync(new Exception("Consul connection failed"));

        var service = new HealthCheckService(_consulClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.IsServiceHealthy(serviceName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsServiceHealthy_WhenMultipleHealthyInstances_ShouldReturnTrue()
    {
        // Arrange
        var serviceName = "LoadBalancedService";
        var healthyServices = new ServiceEntry[]
        {
            new ServiceEntry { Service = new AgentService { Service = serviceName, ID = "instance-1" } },
            new ServiceEntry { Service = new AgentService { Service = serviceName, ID = "instance-2" } },
            new ServiceEntry { Service = new AgentService { Service = serviceName, ID = "instance-3" } }
        };

        var queryResult = new QueryResult<ServiceEntry[]>
        {
            Response = healthyServices
        };

        _healthEndpointMock
            .Setup(x => x.Service(serviceName, null, true, default))
            .ReturnsAsync(queryResult);

        var service = new HealthCheckService(_consulClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.IsServiceHealthy(serviceName);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GetAllServicesHealth Tests

    [Fact]
    public async Task GetAllServicesHealth_WhenMultipleServices_ShouldReturnAllHealthStatus()
    {
        // Arrange
        var catalogServices = new Dictionary<string, string[]>
        {
            { "UserService", Array.Empty<string>() },
            { "RoleService", Array.Empty<string>() },
            { "consul", Array.Empty<string>() } // Should be skipped
        };

        _catalogEndpointMock
            .Setup(x => x.Services(default))
            .ReturnsAsync(new QueryResult<Dictionary<string, string[]>> { Response = catalogServices });

        // UserService is healthy
        _healthEndpointMock
            .Setup(x => x.Service("UserService", null, true, default))
            .ReturnsAsync(new QueryResult<ServiceEntry[]>
            {
                Response = new[] { new ServiceEntry() }
            });

        // RoleService is unhealthy
        _healthEndpointMock
            .Setup(x => x.Service("RoleService", null, true, default))
            .ReturnsAsync(new QueryResult<ServiceEntry[]>
            {
                Response = Array.Empty<ServiceEntry>()
            });

        var service = new HealthCheckService(_consulClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetAllServicesHealth();

        // Assert
        result.Should().HaveCount(2); // consul should be excluded
        result.Should().ContainKey("UserService");
        result.Should().ContainKey("RoleService");
        result["UserService"].Should().BeTrue();
        result["RoleService"].Should().BeFalse();
    }

    [Fact]
    public async Task GetAllServicesHealth_WhenNoServices_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var catalogServices = new Dictionary<string, string[]>
        {
            { "consul", Array.Empty<string>() } // Only consul, which is skipped
        };

        _catalogEndpointMock
            .Setup(x => x.Services(default))
            .ReturnsAsync(new QueryResult<Dictionary<string, string[]>> { Response = catalogServices });

        var service = new HealthCheckService(_consulClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetAllServicesHealth();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllServicesHealth_WhenCatalogThrowsException_ShouldReturnEmptyDictionary()
    {
        // Arrange
        _catalogEndpointMock
            .Setup(x => x.Services(default))
            .ThrowsAsync(new Exception("Consul catalog error"));

        var service = new HealthCheckService(_consulClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetAllServicesHealth();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllServicesHealth_SkipsConsulService()
    {
        // Arrange
        var catalogServices = new Dictionary<string, string[]>
        {
            { "consul", Array.Empty<string>() },
            { "TestService", Array.Empty<string>() }
        };

        _catalogEndpointMock
            .Setup(x => x.Services(default))
            .ReturnsAsync(new QueryResult<Dictionary<string, string[]>> { Response = catalogServices });

        _healthEndpointMock
            .Setup(x => x.Service("TestService", null, true, default))
            .ReturnsAsync(new QueryResult<ServiceEntry[]>
            {
                Response = new[] { new ServiceEntry() }
            });

        var service = new HealthCheckService(_consulClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetAllServicesHealth();

        // Assert
        result.Should().NotContainKey("consul");
        result.Should().ContainKey("TestService");
    }

    #endregion
}

using FluentAssertions;
using Gateway.Application.UseCases;
using Gateway.Domain.Interfaces;
using Moq;
using Xunit;

namespace Gateway.Tests.Unit.Application;

/// <summary>
/// Unit tests for Routing Use Cases
/// </summary>
public class RoutingUseCasesTests
{
    private readonly Mock<IRoutingService> _routingServiceMock;

    public RoutingUseCasesTests()
    {
        _routingServiceMock = new Mock<IRoutingService>();
    }

    #region CheckRouteExistsUseCase Tests

    [Fact]
    public async Task CheckRouteExistsUseCase_WhenRouteExists_ShouldReturnTrue()
    {
        // Arrange
        var upstreamPath = "/api/users";
        _routingServiceMock
            .Setup(x => x.RouteExists(upstreamPath))
            .ReturnsAsync(true);

        var useCase = new CheckRouteExistsUseCase(_routingServiceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(upstreamPath);

        // Assert
        result.Should().BeTrue();
        _routingServiceMock.Verify(x => x.RouteExists(upstreamPath), Times.Once);
    }

    [Fact]
    public async Task CheckRouteExistsUseCase_WhenRouteDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var upstreamPath = "/api/nonexistent";
        _routingServiceMock
            .Setup(x => x.RouteExists(upstreamPath))
            .ReturnsAsync(false);

        var useCase = new CheckRouteExistsUseCase(_routingServiceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(upstreamPath);

        // Assert
        result.Should().BeFalse();
        _routingServiceMock.Verify(x => x.RouteExists(upstreamPath), Times.Once);
    }

    [Fact]
    public async Task CheckRouteExistsUseCase_WithPathParameters_ShouldCheckCorrectPath()
    {
        // Arrange
        var upstreamPath = "/api/users/123";
        _routingServiceMock
            .Setup(x => x.RouteExists(upstreamPath))
            .ReturnsAsync(true);

        var useCase = new CheckRouteExistsUseCase(_routingServiceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(upstreamPath);

        // Assert
        result.Should().BeTrue();
        _routingServiceMock.Verify(x => x.RouteExists(upstreamPath), Times.Once);
    }

    #endregion

    #region ResolveDownstreamPathUseCase Tests

    [Fact]
    public async Task ResolveDownstreamPathUseCase_WhenRouteExists_ShouldReturnDownstreamPath()
    {
        // Arrange
        var upstreamPath = "/api/users";
        var expectedDownstream = "http://userservice:5001/users";
        _routingServiceMock
            .Setup(x => x.ResolveDownstreamPath(upstreamPath))
            .ReturnsAsync(expectedDownstream);

        var useCase = new ResolveDownstreamPathUseCase(_routingServiceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(upstreamPath);

        // Assert
        result.Should().Be(expectedDownstream);
        _routingServiceMock.Verify(x => x.ResolveDownstreamPath(upstreamPath), Times.Once);
    }

    [Fact]
    public async Task ResolveDownstreamPathUseCase_WhenRouteDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var upstreamPath = "/api/nonexistent";
        _routingServiceMock
            .Setup(x => x.ResolveDownstreamPath(upstreamPath))
            .ReturnsAsync(string.Empty);

        var useCase = new ResolveDownstreamPathUseCase(_routingServiceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(upstreamPath);

        // Assert
        result.Should().BeEmpty();
        _routingServiceMock.Verify(x => x.ResolveDownstreamPath(upstreamPath), Times.Once);
    }

    [Fact]
    public async Task ResolveDownstreamPathUseCase_WithPathParameters_ShouldResolveCorrectly()
    {
        // Arrange
        var upstreamPath = "/api/users/456";
        var expectedDownstream = "http://userservice:5001/users/456";
        _routingServiceMock
            .Setup(x => x.ResolveDownstreamPath(upstreamPath))
            .ReturnsAsync(expectedDownstream);

        var useCase = new ResolveDownstreamPathUseCase(_routingServiceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(upstreamPath);

        // Assert
        result.Should().Be(expectedDownstream);
    }

    #endregion
}

/// <summary>
/// Unit tests for Health Check Use Cases
/// </summary>
public class HealthCheckUseCasesTests
{
    private readonly Mock<IHealthCheckService> _healthCheckServiceMock;

    public HealthCheckUseCasesTests()
    {
        _healthCheckServiceMock = new Mock<IHealthCheckService>();
    }

    #region GetServicesHealthUseCase Tests

    [Fact]
    public async Task GetServicesHealthUseCase_ShouldReturnAllServicesHealth()
    {
        // Arrange
        var expectedHealth = new Dictionary<string, bool>
        {
            { "UserService", true },
            { "RoleService", true },
            { "AuthService", false }
        };
        _healthCheckServiceMock
            .Setup(x => x.GetAllServicesHealth())
            .ReturnsAsync(expectedHealth);

        var useCase = new GetServicesHealthUseCase(_healthCheckServiceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedHealth);
        _healthCheckServiceMock.Verify(x => x.GetAllServicesHealth(), Times.Once);
    }

    [Fact]
    public async Task GetServicesHealthUseCase_WhenNoServices_ShouldReturnEmptyDictionary()
    {
        // Arrange
        _healthCheckServiceMock
            .Setup(x => x.GetAllServicesHealth())
            .ReturnsAsync(new Dictionary<string, bool>());

        var useCase = new GetServicesHealthUseCase(_healthCheckServiceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CheckServiceHealthUseCase Tests

    [Fact]
    public async Task CheckServiceHealthUseCase_WhenServiceIsHealthy_ShouldReturnTrue()
    {
        // Arrange
        var serviceName = "UserService";
        _healthCheckServiceMock
            .Setup(x => x.IsServiceHealthy(serviceName))
            .ReturnsAsync(true);

        var useCase = new CheckServiceHealthUseCase(_healthCheckServiceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(serviceName);

        // Assert
        result.Should().BeTrue();
        _healthCheckServiceMock.Verify(x => x.IsServiceHealthy(serviceName), Times.Once);
    }

    [Fact]
    public async Task CheckServiceHealthUseCase_WhenServiceIsUnhealthy_ShouldReturnFalse()
    {
        // Arrange
        var serviceName = "FailingService";
        _healthCheckServiceMock
            .Setup(x => x.IsServiceHealthy(serviceName))
            .ReturnsAsync(false);

        var useCase = new CheckServiceHealthUseCase(_healthCheckServiceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(serviceName);

        // Assert
        result.Should().BeFalse();
        _healthCheckServiceMock.Verify(x => x.IsServiceHealthy(serviceName), Times.Once);
    }

    [Fact]
    public async Task CheckServiceHealthUseCase_WithDifferentServices_ShouldCheckCorrectService()
    {
        // Arrange
        _healthCheckServiceMock
            .Setup(x => x.IsServiceHealthy("ServiceA"))
            .ReturnsAsync(true);
        _healthCheckServiceMock
            .Setup(x => x.IsServiceHealthy("ServiceB"))
            .ReturnsAsync(false);

        var useCase = new CheckServiceHealthUseCase(_healthCheckServiceMock.Object);

        // Act & Assert
        (await useCase.ExecuteAsync("ServiceA")).Should().BeTrue();
        (await useCase.ExecuteAsync("ServiceB")).Should().BeFalse();
    }

    #endregion
}

/// <summary>
/// Unit tests for Metrics Use Cases
/// </summary>
public class MetricsUseCasesTests
{
    private readonly Mock<IMetricsService> _metricsServiceMock;

    public MetricsUseCasesTests()
    {
        _metricsServiceMock = new Mock<IMetricsService>();
    }

    #region RecordRequestMetricsUseCase Tests

    [Fact]
    public void RecordRequestMetricsUseCase_ShouldRecordRequestMetrics()
    {
        // Arrange
        var route = "/api/users";
        var method = "GET";
        var statusCode = 200;
        var duration = 0.150;

        var useCase = new RecordRequestMetricsUseCase(_metricsServiceMock.Object);

        // Act
        useCase.Execute(route, method, statusCode, duration);

        // Assert
        _metricsServiceMock.Verify(
            x => x.RecordRequest(route, method, statusCode, duration),
            Times.Once);
    }

    [Fact]
    public void RecordRequestMetricsUseCase_WithErrorStatus_ShouldRecordCorrectly()
    {
        // Arrange
        var route = "/api/users";
        var method = "POST";
        var statusCode = 500;
        var duration = 1.5;

        var useCase = new RecordRequestMetricsUseCase(_metricsServiceMock.Object);

        // Act
        useCase.Execute(route, method, statusCode, duration);

        // Assert
        _metricsServiceMock.Verify(
            x => x.RecordRequest(route, method, statusCode, duration),
            Times.Once);
    }

    [Fact]
    public void RecordRequestMetricsUseCase_WithDifferentMethods_ShouldRecordEach()
    {
        // Arrange
        var useCase = new RecordRequestMetricsUseCase(_metricsServiceMock.Object);

        // Act
        useCase.Execute("/users", "GET", 200, 0.1);
        useCase.Execute("/users", "POST", 201, 0.2);
        useCase.Execute("/users/1", "DELETE", 204, 0.05);

        // Assert
        _metricsServiceMock.Verify(x => x.RecordRequest("/users", "GET", 200, 0.1), Times.Once);
        _metricsServiceMock.Verify(x => x.RecordRequest("/users", "POST", 201, 0.2), Times.Once);
        _metricsServiceMock.Verify(x => x.RecordRequest("/users/1", "DELETE", 204, 0.05), Times.Once);
    }

    #endregion

    #region RecordDownstreamCallMetricsUseCase Tests

    [Fact]
    public void RecordDownstreamCallMetricsUseCase_SuccessfulCall_ShouldRecordWithSuccessTrue()
    {
        // Arrange
        var service = "UserService";
        var latency = 0.050;
        var success = true;

        var useCase = new RecordDownstreamCallMetricsUseCase(_metricsServiceMock.Object);

        // Act
        useCase.Execute(service, latency, success);

        // Assert
        _metricsServiceMock.Verify(
            x => x.RecordDownstreamCall(service, latency, success),
            Times.Once);
    }

    [Fact]
    public void RecordDownstreamCallMetricsUseCase_FailedCall_ShouldRecordWithSuccessFalse()
    {
        // Arrange
        var service = "RoleService";
        var latency = 5.0;
        var success = false;

        var useCase = new RecordDownstreamCallMetricsUseCase(_metricsServiceMock.Object);

        // Act
        useCase.Execute(service, latency, success);

        // Assert
        _metricsServiceMock.Verify(
            x => x.RecordDownstreamCall(service, latency, success),
            Times.Once);
    }

    [Fact]
    public void RecordDownstreamCallMetricsUseCase_MultipleServices_ShouldRecordAll()
    {
        // Arrange
        var useCase = new RecordDownstreamCallMetricsUseCase(_metricsServiceMock.Object);

        // Act
        useCase.Execute("ServiceA", 0.1, true);
        useCase.Execute("ServiceB", 0.2, true);
        useCase.Execute("ServiceC", 1.0, false);

        // Assert
        _metricsServiceMock.Verify(x => x.RecordDownstreamCall("ServiceA", 0.1, true), Times.Once);
        _metricsServiceMock.Verify(x => x.RecordDownstreamCall("ServiceB", 0.2, true), Times.Once);
        _metricsServiceMock.Verify(x => x.RecordDownstreamCall("ServiceC", 1.0, false), Times.Once);
    }

    #endregion
}

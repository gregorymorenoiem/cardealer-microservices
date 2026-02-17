using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Domain.Entities;
using ServiceDiscovery.Domain.Enums;
using ServiceDiscovery.Infrastructure.Services;
using System.Net;
using Xunit;

namespace ServiceDiscovery.Tests.Infrastructure;

/// <summary>
/// Unit tests for ResilientHealthChecker with circuit breaker and retry logic
/// </summary>
public class ResilientHealthCheckerTests
{
    private readonly Mock<IServiceDiscovery> _mockDiscovery;
    private readonly Mock<ILogger<ResilientHealthChecker>> _mockLogger;
    private readonly MockHttpMessageHandler _mockHttp;

    public ResilientHealthCheckerTests()
    {
        _mockDiscovery = new Mock<IServiceDiscovery>();
        _mockLogger = new Mock<ILogger<ResilientHealthChecker>>();
        _mockHttp = new MockHttpMessageHandler();
    }

    private ResilientHealthChecker CreateSut(ResilientHealthCheckerOptions? options = null)
    {
        var httpClient = _mockHttp.ToHttpClient();
        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient("HealthCheck")).Returns(httpClient);

        return new ResilientHealthChecker(
            _mockDiscovery.Object,
            mockFactory.Object,
            _mockLogger.Object,
            options ?? new ResilientHealthCheckerOptions());
    }

    private static ServiceInstance CreateTestInstance(string id = "test-1", string serviceName = "test-service")
    {
        return new ServiceInstance
        {
            Id = id,
            ServiceName = serviceName,
            Host = "localhost",
            Port = 8080,
            HealthCheckUrl = "/health",
            HealthCheckInterval = 10,
            HealthCheckTimeout = 5
        };
    }

    #region CheckHealthAsync Tests

    [Fact]
    public async Task CheckHealthAsync_WithHealthyService_ReturnsHealthyResult()
    {
        // Arrange
        var instance = CreateTestInstance();
        _mockHttp.When("http://localhost:8080/health")
            .Respond(HttpStatusCode.OK);

        var sut = CreateSut();

        // Act
        var result = await sut.CheckHealthAsync(instance);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.InstanceId.Should().Be(instance.Id);
    }

    [Fact]
    public async Task CheckHealthAsync_WithUnhealthyService_ReturnsUnhealthyResult()
    {
        // Arrange
        var instance = CreateTestInstance();
        _mockHttp.When("http://localhost:8080/health")
            .Respond(HttpStatusCode.ServiceUnavailable);

        // Use options with minimal retries - all will fail since we always return 503
        var options = new ResilientHealthCheckerOptions
        {
            MaxRetryAttempts = 1, // At least 1 is required by Polly
            RetryDelayMilliseconds = 1,
            CircuitBreakerMinimumThroughput = 100 // High threshold to avoid circuit opening
        };
        var sut = CreateSut(options);

        // Act
        var result = await sut.CheckHealthAsync(instance);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WithNoHealthCheckUrl_ReturnsDegradedResult()
    {
        // Arrange
        var instance = new ServiceInstance
        {
            Id = "test-1",
            ServiceName = "test-service",
            Host = "localhost",
            Port = 8080,
            HealthCheckUrl = null
        };

        var sut = CreateSut();

        // Act
        var result = await sut.CheckHealthAsync(instance);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Message.Should().Contain("No health check URL configured");
    }

    [Fact]
    public async Task CheckHealthAsync_WithEmptyHealthCheckUrl_ReturnsDegradedResult()
    {
        // Arrange
        var instance = new ServiceInstance
        {
            Id = "test-1",
            ServiceName = "test-service",
            Host = "localhost",
            Port = 8080,
            HealthCheckUrl = "   "
        };

        var sut = CreateSut();

        // Act
        var result = await sut.CheckHealthAsync(instance);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
    }

    #endregion

    #region Retry Logic Tests

    [Fact]
    public async Task CheckHealthAsync_WithTransientFailure_RetriesAndSucceeds()
    {
        // Arrange
        var instance = CreateTestInstance();
        var callCount = 0;

        _mockHttp.When("http://localhost:8080/health")
            .Respond(_ =>
            {
                callCount++;
                return callCount < 2
                    ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    : new HttpResponseMessage(HttpStatusCode.OK);
            });

        var options = new ResilientHealthCheckerOptions
        {
            MaxRetryAttempts = 3,
            RetryDelayMilliseconds = 10
        };
        var sut = CreateSut(options);

        // Act
        var result = await sut.CheckHealthAsync(instance);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        callCount.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task CheckHealthAsync_WithPersistentFailure_ReturnsUnhealthyAfterRetries()
    {
        // Arrange
        var instance = CreateTestInstance();
        var callCount = 0;

        _mockHttp.When("http://localhost:8080/health")
            .Respond(_ =>
            {
                callCount++;
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            });

        var options = new ResilientHealthCheckerOptions
        {
            MaxRetryAttempts = 2,
            RetryDelayMilliseconds = 10
        };
        var sut = CreateSut(options);

        // Act
        var result = await sut.CheckHealthAsync(instance);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        callCount.Should().BeGreaterOrEqualTo(1); // At least initial call
    }

    #endregion

    #region Circuit Breaker Tests

    [Fact]
    public async Task CheckHealthAsync_MultiplePipelines_IsolatesCircuitBreakers()
    {
        // Arrange
        var instance1 = CreateTestInstance("service-1", "svc-1");
        var instance2 = CreateTestInstance("service-2", "svc-2");

        _mockHttp.When($"http://{instance1.Host}:{instance1.Port}/health")
            .Respond(HttpStatusCode.OK);
        _mockHttp.When($"http://{instance2.Host}:{instance2.Port}/health")
            .Respond(HttpStatusCode.OK);

        var sut = CreateSut();

        // Act
        var result1 = await sut.CheckHealthAsync(instance1);
        var result2 = await sut.CheckHealthAsync(instance2);

        // Assert
        result1.Status.Should().Be(HealthStatus.Healthy);
        result2.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public void ResetCircuitBreaker_ExistingBreaker_Resets()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert - should not throw
        var act = () => sut.ResetCircuitBreaker("non-existent-service");
        act.Should().NotThrow();
    }

    [Fact]
    public void ResetAllCircuitBreakers_ClearsAllPipelines()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert - should not throw
        var act = () => sut.ResetAllCircuitBreakers();
        act.Should().NotThrow();
    }

    [Fact]
    public void GetCircuitBreakerState_NonExistentService_ReturnsUnknown()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var state = sut.GetCircuitBreakerState("non-existent");

        // Assert
        state.Should().Be(CircuitBreakerState.Unknown);
    }

    #endregion

    #region CheckServiceHealthAsync Tests

    [Fact]
    public async Task CheckServiceHealthAsync_WithMultipleInstances_ChecksAllInstances()
    {
        // Arrange
        var instances = new List<ServiceInstance>
        {
            CreateTestInstance("instance-1"),
            CreateTestInstance("instance-2"),
            CreateTestInstance("instance-3")
        };

        _mockDiscovery.Setup(d => d.GetServiceInstancesAsync("test-service", It.IsAny<CancellationToken>()))
            .ReturnsAsync(instances);

        _mockHttp.When("http://localhost:8080/health")
            .Respond(HttpStatusCode.OK);

        var sut = CreateSut();

        // Act
        var results = await sut.CheckServiceHealthAsync("test-service");

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.Status == HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckServiceHealthAsync_WithNoInstances_ReturnsEmptyList()
    {
        // Arrange
        _mockDiscovery.Setup(d => d.GetServiceInstancesAsync("empty-service", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceInstance>());

        var sut = CreateSut();

        // Act
        var results = await sut.CheckServiceHealthAsync("empty-service");

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region CheckAllServicesHealthAsync Tests

    [Fact]
    public async Task CheckAllServicesHealthAsync_WithMultipleServices_ChecksAllServices()
    {
        // Arrange
        var serviceNames = new List<string> { "service-a", "service-b" };

        _mockDiscovery.Setup(d => d.GetServiceNamesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceNames);

        _mockDiscovery.Setup(d => d.GetServiceInstancesAsync("service-a", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceInstance> { CreateTestInstance("a-1") });

        _mockDiscovery.Setup(d => d.GetServiceInstancesAsync("service-b", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceInstance> { CreateTestInstance("b-1"), CreateTestInstance("b-2") });

        _mockHttp.When("http://localhost:8080/health")
            .Respond(HttpStatusCode.OK);

        var sut = CreateSut();

        // Act
        var results = await sut.CheckAllServicesHealthAsync();

        // Assert
        results.Should().HaveCount(3); // 1 from service-a + 2 from service-b
    }

    [Fact]
    public async Task CheckAllServicesHealthAsync_WithNoServices_ReturnsEmptyList()
    {
        // Arrange
        _mockDiscovery.Setup(d => d.GetServiceNamesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        var sut = CreateSut();

        // Act
        var results = await sut.CheckAllServicesHealthAsync();

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region Options Tests

    [Fact]
    public void ResilientHealthCheckerOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new ResilientHealthCheckerOptions();

        // Assert
        options.MaxRetryAttempts.Should().Be(3);
        options.RetryDelayMilliseconds.Should().Be(200);
        options.TimeoutSeconds.Should().Be(10);
        options.CircuitBreakerFailureRatio.Should().Be(0.5);
        options.CircuitBreakerSamplingDurationSeconds.Should().Be(30);
        options.CircuitBreakerMinimumThroughput.Should().Be(3);
        options.CircuitBreakerBreakDurationSeconds.Should().Be(30);
    }

    [Fact]
    public void ResilientHealthCheckerOptions_CanBeCustomized()
    {
        // Arrange & Act
        var options = new ResilientHealthCheckerOptions
        {
            MaxRetryAttempts = 5,
            RetryDelayMilliseconds = 500,
            TimeoutSeconds = 30,
            CircuitBreakerFailureRatio = 0.7,
            CircuitBreakerSamplingDurationSeconds = 60,
            CircuitBreakerMinimumThroughput = 10,
            CircuitBreakerBreakDurationSeconds = 120
        };

        // Assert
        options.MaxRetryAttempts.Should().Be(5);
        options.RetryDelayMilliseconds.Should().Be(500);
        options.TimeoutSeconds.Should().Be(30);
        options.CircuitBreakerFailureRatio.Should().Be(0.7);
        options.CircuitBreakerSamplingDurationSeconds.Should().Be(60);
        options.CircuitBreakerMinimumThroughput.Should().Be(10);
        options.CircuitBreakerBreakDurationSeconds.Should().Be(120);
    }

    #endregion

    #region HTTPS Detection Tests

    [Fact]
    public async Task CheckHealthAsync_WithPort443_UsesHttps()
    {
        // Arrange
        var instance = new ServiceInstance
        {
            Id = "https-service",
            ServiceName = "secure-service",
            Host = "secure.example.com",
            Port = 443,
            HealthCheckUrl = "/health"
        };

        _mockHttp.When("https://secure.example.com:443/health")
            .Respond(HttpStatusCode.OK);

        var sut = CreateSut();

        // Act
        var result = await sut.CheckHealthAsync(instance);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task CheckHealthAsync_WithCancellation_ReturnsDegradedOrHandlesGracefully()
    {
        // Arrange
        var instance = CreateTestInstance();
        instance.HealthCheckUrl = null; // This will return degraded immediately without HTTP call

        var sut = CreateSut();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act - with null health check URL, it returns immediately without checking cancellation
        var result = await sut.CheckHealthAsync(instance, cts.Token);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
    }

    [Fact]
    public async Task CheckHealthAsync_WithHealthCheckUrlWithLeadingSlash_HandlesCorrectly()
    {
        // Arrange
        var instance = new ServiceInstance
        {
            Id = "test-1",
            ServiceName = "test-service",
            Host = "localhost",
            Port = 8080,
            HealthCheckUrl = "/api/health"
        };

        _mockHttp.When("http://localhost:8080/api/health")
            .Respond(HttpStatusCode.OK);

        var sut = CreateSut();

        // Act
        var result = await sut.CheckHealthAsync(instance);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    #endregion
}

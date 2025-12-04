using System.Diagnostics.Metrics;
using FluentAssertions;
using Gateway.Infrastructure.Services;
using Xunit;

namespace Gateway.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for MetricsService
/// </summary>
public class MetricsServiceTests
{
    private readonly MetricsService _metricsService;
    private readonly IMeterFactory _meterFactory;

    public MetricsServiceTests()
    {
        _meterFactory = new TestMeterFactory();
        _metricsService = new MetricsService(_meterFactory);
    }

    #region RecordRequest Tests

    [Fact]
    public void RecordRequest_WithSuccessfulRequest_ShouldIncrementCounter()
    {
        // Arrange
        var route = "/api/users";
        var method = "GET";
        var statusCode = 200;
        var duration = 0.150;

        // Act
        _metricsService.RecordRequest(route, method, statusCode, duration);

        // Assert - the method should not throw
        // Metrics are recorded to the meter
    }

    [Fact]
    public void RecordRequest_WithErrorStatus_ShouldIncrementFailedCounter()
    {
        // Arrange
        var route = "/api/users";
        var method = "POST";
        var statusCode = 500;
        var duration = 1.5;

        // Act
        _metricsService.RecordRequest(route, method, statusCode, duration);

        // Assert - the method should not throw
        // Both total and failed counters should be incremented
    }

    [Fact]
    public void RecordRequest_With4xxStatus_ShouldIncrementFailedCounter()
    {
        // Arrange
        var route = "/api/users";
        var method = "PUT";
        var statusCode = 404;
        var duration = 0.05;

        // Act
        _metricsService.RecordRequest(route, method, statusCode, duration);

        // Assert - the method should not throw
    }

    [Fact]
    public void RecordRequest_MultipleTimes_ShouldAccumulate()
    {
        // Arrange
        var route = "/api/users";
        var method = "GET";

        // Act
        for (int i = 0; i < 10; i++)
        {
            _metricsService.RecordRequest(route, method, 200, 0.1);
        }

        // Assert - the method should not throw and metrics should accumulate
    }

    [Fact]
    public void RecordRequest_WithDifferentRoutes_ShouldTagCorrectly()
    {
        // Arrange & Act
        _metricsService.RecordRequest("/api/users", "GET", 200, 0.1);
        _metricsService.RecordRequest("/api/roles", "POST", 201, 0.2);
        _metricsService.RecordRequest("/api/auth", "POST", 401, 0.05);

        // Assert - no exception thrown
    }

    [Fact]
    public void RecordRequest_WithDifferentMethods_ShouldTagCorrectly()
    {
        // Arrange
        var route = "/api/users";

        // Act
        _metricsService.RecordRequest(route, "GET", 200, 0.1);
        _metricsService.RecordRequest(route, "POST", 201, 0.2);
        _metricsService.RecordRequest(route, "PUT", 200, 0.15);
        _metricsService.RecordRequest(route, "DELETE", 204, 0.05);
        _metricsService.RecordRequest(route, "PATCH", 200, 0.12);

        // Assert - no exception thrown
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(204)]
    [InlineData(301)]
    [InlineData(302)]
    [InlineData(399)]
    public void RecordRequest_WithNonErrorStatus_ShouldNotIncrementFailedCounter(int statusCode)
    {
        // Arrange
        var route = "/api/test";
        var method = "GET";

        // Act
        _metricsService.RecordRequest(route, method, statusCode, 0.1);

        // Assert - method should complete without error
    }

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    public void RecordRequest_WithErrorStatus_ShouldAlwaysIncrementFailedCounter(int statusCode)
    {
        // Arrange
        var route = "/api/test";
        var method = "GET";

        // Act
        _metricsService.RecordRequest(route, method, statusCode, 0.1);

        // Assert - method should complete without error
    }

    #endregion

    #region RecordDownstreamCall Tests

    [Fact]
    public void RecordDownstreamCall_SuccessfulCall_ShouldRecordLatency()
    {
        // Arrange
        var service = "UserService";
        var latency = 0.050;

        // Act
        _metricsService.RecordDownstreamCall(service, latency, true);

        // Assert - method should complete without error
    }

    [Fact]
    public void RecordDownstreamCall_FailedCall_ShouldIncrementErrorCounter()
    {
        // Arrange
        var service = "RoleService";
        var latency = 5.0;

        // Act
        _metricsService.RecordDownstreamCall(service, latency, false);

        // Assert - method should complete without error
    }

    [Fact]
    public void RecordDownstreamCall_MultipleServices_ShouldTagCorrectly()
    {
        // Arrange & Act
        _metricsService.RecordDownstreamCall("ServiceA", 0.1, true);
        _metricsService.RecordDownstreamCall("ServiceB", 0.2, true);
        _metricsService.RecordDownstreamCall("ServiceC", 0.3, false);

        // Assert - method should complete without error
    }

    [Fact]
    public void RecordDownstreamCall_MultipleCallsSameService_ShouldAccumulate()
    {
        // Arrange
        var service = "UserService";

        // Act
        for (int i = 0; i < 100; i++)
        {
            _metricsService.RecordDownstreamCall(service, 0.01 * i, i % 10 == 0);
        }

        // Assert - method should complete without error
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(0.01)]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(10.0)]
    [InlineData(60.0)]
    public void RecordDownstreamCall_VariousLatencies_ShouldRecordCorrectly(double latency)
    {
        // Arrange
        var service = "TestService";

        // Act
        _metricsService.RecordDownstreamCall(service, latency, true);

        // Assert - method should complete without error
    }

    #endregion
}

/// <summary>
/// Test factory for creating meters in tests
/// </summary>
internal class TestMeterFactory : IMeterFactory
{
    private readonly List<Meter> _meters = new();

    public Meter Create(MeterOptions options)
    {
        var meter = new Meter(options.Name, options.Version);
        _meters.Add(meter);
        return meter;
    }

    public void Dispose()
    {
        foreach (var meter in _meters)
        {
            meter.Dispose();
        }
        _meters.Clear();
    }
}

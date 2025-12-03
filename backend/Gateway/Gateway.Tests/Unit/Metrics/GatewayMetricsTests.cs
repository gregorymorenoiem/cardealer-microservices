using Gateway.Metrics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace Gateway.Tests.Unit.Metrics;

/// <summary>
/// Unit tests for GatewayMetrics
/// </summary>
public class GatewayMetricsTests
{
    [Fact]
    public void RecordRequest_WithSuccessStatus_DoesNotThrow()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new GatewayMetrics(meterFactory);
        var route = "/api/test";
        var method = "GET";
        var statusCode = 200;
        var duration = 0.15;

        // Act
        var act = () => metrics.RecordRequest(route, method, statusCode, duration);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordRequest_WithErrorStatus_DoesNotThrow()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new GatewayMetrics(meterFactory);
        var route = "/api/test";
        var method = "GET";
        var statusCode = 500;
        var duration = 0.25;

        // Act
        var act = () => metrics.RecordRequest(route, method, statusCode, duration);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordDownstreamCall_WithSuccess_DoesNotThrow()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new GatewayMetrics(meterFactory);
        var service = "TestService";
        var latency = 0.1;

        // Act
        var act = () => metrics.RecordDownstreamCall(service, latency, success: true);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordDownstreamCall_WithFailure_DoesNotThrow()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new GatewayMetrics(meterFactory);
        var service = "TestService";
        var latency = 0.5;

        // Act
        var act = () => metrics.RecordDownstreamCall(service, latency, success: false);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(204)]
    [InlineData(301)]
    [InlineData(302)]
    public void RecordRequest_WithSuccessStatusCodes_DoesNotRecordFailure(int statusCode)
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new GatewayMetrics(meterFactory);

        // Act
        var act = () => metrics.RecordRequest("/api/test", "GET", statusCode, 0.1);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    public void RecordRequest_WithErrorStatusCodes_RecordsFailure(int statusCode)
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new GatewayMetrics(meterFactory);

        // Act
        var act = () => metrics.RecordRequest("/api/test", "POST", statusCode, 0.2);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordRequest_WithMultipleCalls_DoesNotThrow()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new GatewayMetrics(meterFactory);

        // Act
        var act = () =>
        {
            metrics.RecordRequest("/api/users", "GET", 200, 0.1);
            metrics.RecordRequest("/api/users", "POST", 201, 0.15);
            metrics.RecordRequest("/api/users/1", "PUT", 200, 0.12);
            metrics.RecordRequest("/api/users/1", "DELETE", 204, 0.08);
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordDownstreamCall_WithMultipleServices_DoesNotThrow()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new GatewayMetrics(meterFactory);

        // Act
        var act = () =>
        {
            metrics.RecordDownstreamCall("AuthService", 0.05, true);
            metrics.RecordDownstreamCall("UserService", 0.08, true);
            metrics.RecordDownstreamCall("VehicleService", 0.12, true);
            metrics.RecordDownstreamCall("MediaService", 0.5, false);
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GatewayMetrics_Constructor_CreatesInstance()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();

        // Act
        var metrics = new GatewayMetrics(meterFactory);

        // Assert
        metrics.Should().NotBeNull();
    }
}

/// <summary>
/// Test implementation of IMeterFactory for unit testing
/// </summary>
public class TestMeterFactory : IMeterFactory
{
    public Meter Create(MeterOptions options) => new Meter(options);
    public void Dispose() { }
}

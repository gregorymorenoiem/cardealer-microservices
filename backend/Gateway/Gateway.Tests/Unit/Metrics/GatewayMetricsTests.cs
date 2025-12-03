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
}

/// <summary>
/// Test implementation of IMeterFactory for unit testing
/// </summary>
public class TestMeterFactory : IMeterFactory
{
    public Meter Create(MeterOptions options) => new Meter(options);
    public void Dispose() { }
}

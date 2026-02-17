using TracingService.Domain.Entities;
using TracingService.Domain.Enums;
using Xunit;

namespace TracingService.Tests.Domain;

public class TraceTests
{
    [Fact]
    public void Trace_ShouldCalculateDuration_WhenEndTimeIsSet()
    {
        // Arrange
        var trace = new Trace
        {
            TraceId = "trace1",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMilliseconds(1000)
        };

        // Assert
        Assert.NotNull(trace.DurationMs);
        Assert.True(trace.DurationMs >= 1000 && trace.DurationMs < 1100);
    }

    [Fact]
    public void Trace_SpanCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var trace = new Trace
        {
            TraceId = "trace1",
            Spans = new List<Span>
            {
                new Span { SpanId = "span1", TraceId = "trace1" },
                new Span { SpanId = "span2", TraceId = "trace1" },
                new Span { SpanId = "span3", TraceId = "trace1" }
            }
        };

        // Assert
        Assert.Equal(3, trace.SpanCount);
    }

    [Fact]
    public void Trace_ServiceCount_ShouldReturnDistinctServiceCount()
    {
        // Arrange
        var trace = new Trace
        {
            TraceId = "trace1",
            Spans = new List<Span>
            {
                new Span { SpanId = "span1", ServiceName = "ServiceA" },
                new Span { SpanId = "span2", ServiceName = "ServiceA" },
                new Span { SpanId = "span3", ServiceName = "ServiceB" },
                new Span { SpanId = "span4", ServiceName = "ServiceC" }
            }
        };

        // Assert
        Assert.Equal(3, trace.ServiceCount);
    }

    [Fact]
    public void Trace_ServicesInvolved_ShouldReturnDistinctServices()
    {
        // Arrange
        var trace = new Trace
        {
            TraceId = "trace1",
            Spans = new List<Span>
            {
                new Span { SpanId = "span1", ServiceName = "ServiceA" },
                new Span { SpanId = "span2", ServiceName = "ServiceB" },
                new Span { SpanId = "span3", ServiceName = "ServiceA" }
            }
        };

        // Act
        var services = trace.ServicesInvolved;

        // Assert
        Assert.Equal(2, services.Count);
        Assert.Contains("ServiceA", services);
        Assert.Contains("ServiceB", services);
    }

    [Fact]
    public void Trace_HasError_ShouldBeTrue_WhenAnySpanHasError()
    {
        // Arrange
        var trace = new Trace
        {
            TraceId = "trace1",
            Spans = new List<Span>
            {
                new Span { SpanId = "span1", Status = SpanStatus.Ok },
                new Span { SpanId = "span2", Status = SpanStatus.Error },
                new Span { SpanId = "span3", Status = SpanStatus.Ok }
            }
        };

        // Assert
        Assert.True(trace.HasError);
    }

    [Fact]
    public void Trace_HasError_ShouldBeFalse_WhenNoSpansHaveErrors()
    {
        // Arrange
        var trace = new Trace
        {
            TraceId = "trace1",
            Spans = new List<Span>
            {
                new Span { SpanId = "span1", Status = SpanStatus.Ok },
                new Span { SpanId = "span2", Status = SpanStatus.Ok }
            }
        };

        // Assert
        Assert.False(trace.HasError);
    }

    [Fact]
    public void Trace_ErrorCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var trace = new Trace
        {
            TraceId = "trace1",
            Spans = new List<Span>
            {
                new Span { SpanId = "span1", Status = SpanStatus.Ok },
                new Span { SpanId = "span2", Status = SpanStatus.Error },
                new Span { SpanId = "span3", Status = SpanStatus.Error },
                new Span { SpanId = "span4", Status = SpanStatus.Ok }
            }
        };

        // Assert
        Assert.Equal(2, trace.ErrorCount);
    }
}

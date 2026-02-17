using TracingService.Domain.Entities;
using TracingService.Domain.Enums;
using Xunit;

namespace TracingService.Tests.Domain;

public class SpanTests
{
    [Fact]
    public void Span_ShouldCalculateDuration_WhenEndTimeIsSet()
    {
        // Arrange
        var span = new Span
        {
            SpanId = "span1",
            TraceId = "trace1",
            Name = "TestSpan",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMilliseconds(500)
        };

        // Assert
        Assert.NotNull(span.DurationMs);
        Assert.True(span.DurationMs >= 500 && span.DurationMs < 600);
    }

    [Fact]
    public void Span_DurationMs_ShouldBeNull_WhenEndTimeIsNull()
    {
        // Arrange
        var span = new Span
        {
            SpanId = "span1",
            TraceId = "trace1",
            Name = "TestSpan",
            StartTime = DateTime.UtcNow,
            EndTime = null
        };

        // Assert
        Assert.Null(span.DurationMs);
    }

    [Fact]
    public void Span_HasError_ShouldBeTrue_WhenStatusIsError()
    {
        // Arrange
        var span = new Span
        {
            SpanId = "span1",
            TraceId = "trace1",
            Name = "TestSpan",
            Status = SpanStatus.Error
        };

        // Assert
        Assert.True(span.HasError);
    }

    [Fact]
    public void Span_HasError_ShouldBeFalse_WhenStatusIsOk()
    {
        // Arrange
        var span = new Span
        {
            SpanId = "span1",
            TraceId = "trace1",
            Name = "TestSpan",
            Status = SpanStatus.Ok
        };

        // Assert
        Assert.False(span.HasError);
    }

    [Fact]
    public void Span_ShouldInitializeTags_AsEmptyDictionary()
    {
        // Arrange & Act
        var span = new Span();

        // Assert
        Assert.NotNull(span.Tags);
        Assert.Empty(span.Tags);
    }

    [Fact]
    public void Span_ShouldInitializeEvents_AsEmptyList()
    {
        // Arrange & Act
        var span = new Span();

        // Assert
        Assert.NotNull(span.Events);
        Assert.Empty(span.Events);
    }
}

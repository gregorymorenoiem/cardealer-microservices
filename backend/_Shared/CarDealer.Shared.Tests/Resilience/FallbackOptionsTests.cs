using CarDealer.Shared.Resilience.Configuration;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Resilience;

public class FallbackOptionsTests
{
    [Fact]
    public void FallbackOptions_DefaultValues_ShouldBeCorrect()
    {
        var options = new FallbackOptions();

        options.Enabled.Should().BeTrue();
        options.UseCachedResponse.Should().BeTrue();
        options.CacheTtlSeconds.Should().Be(300);
        options.DegradedStatusCode.Should().Be(503);
        options.DefaultMessage.Should().Be("Service temporarily unavailable. Please try again later.");
    }

    [Fact]
    public void FallbackOptions_CustomValues_ShouldBeRetained()
    {
        var options = new FallbackOptions
        {
            Enabled = false,
            UseCachedResponse = false,
            CacheTtlSeconds = 600,
            DegradedStatusCode = 429,
            DefaultMessage = "Custom degraded message"
        };

        options.Enabled.Should().BeFalse();
        options.UseCachedResponse.Should().BeFalse();
        options.CacheTtlSeconds.Should().Be(600);
        options.DegradedStatusCode.Should().Be(429);
        options.DefaultMessage.Should().Be("Custom degraded message");
    }
}

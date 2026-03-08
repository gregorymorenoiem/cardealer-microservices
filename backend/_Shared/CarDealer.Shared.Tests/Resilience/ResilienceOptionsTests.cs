using CarDealer.Shared.Resilience.Configuration;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Resilience;

/// <summary>
/// Tests for ResilienceOptions defaults and nested configuration.
/// </summary>
public class ResilienceOptionsTests
{
    [Fact]
    public void SectionName_IsCorrect()
    {
        ResilienceOptions.SectionName.Should().Be("Resilience");
    }

    [Fact]
    public void DefaultOptions_AreReasonable()
    {
        var options = new ResilienceOptions();

        options.Enabled.Should().BeTrue();
        options.Retry.Should().NotBeNull();
        options.CircuitBreaker.Should().NotBeNull();
        options.Timeout.Should().NotBeNull();
        options.RateLimiter.Should().NotBeNull();
        options.Bulkhead.Should().NotBeNull();
        options.Hedging.Should().NotBeNull();
        options.Fallback.Should().NotBeNull();
    }

    [Fact]
    public void RetryOptions_Defaults()
    {
        var retry = new RetryOptions();

        retry.MaxRetries.Should().Be(3);
        retry.DelaySeconds.Should().Be(2);
        retry.UseExponentialBackoff.Should().BeTrue();
        retry.UseJitter.Should().BeTrue();
        retry.RetryStatusCodes.Should().Contain(new[] { 408, 429, 500, 502, 503, 504 });
    }

    [Fact]
    public void CircuitBreakerOptions_Defaults()
    {
        var cb = new CircuitBreakerOptions();

        cb.FailureRatio.Should().Be(0.5);
        cb.MinimumThroughput.Should().Be(10);
        cb.SamplingDurationSeconds.Should().Be(30);
        cb.BreakDurationSeconds.Should().Be(30);
    }

    [Fact]
    public void TimeoutOptions_Defaults()
    {
        var timeout = new TimeoutOptions();

        timeout.TimeoutSeconds.Should().Be(30);
        timeout.TotalTimeoutSeconds.Should().Be(120);
    }

    [Fact]
    public void RateLimiterOptions_DefaultsDisabled()
    {
        var rl = new RateLimiterOptions();

        rl.Enabled.Should().BeFalse();
        rl.PermitLimit.Should().Be(100);
        rl.QueueLimit.Should().Be(10);
    }
}

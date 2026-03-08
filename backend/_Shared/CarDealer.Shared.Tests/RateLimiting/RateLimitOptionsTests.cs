using CarDealer.Shared.RateLimiting.Models;
using FluentAssertions;

namespace CarDealer.Shared.Tests.RateLimiting;

public class RateLimitOptionsTests
{
    [Fact]
    public void SectionName_ShouldBeRateLimiting()
    {
        RateLimitOptions.SectionName.Should().Be("RateLimiting");
    }

    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var options = new RateLimitOptions();

        options.Enabled.Should().BeTrue();
        options.RedisConnection.Should().Be("localhost:6379");
        options.ServiceUrl.Should().Be("http://ratelimitingservice:8080");
        options.Mode.Should().Be("redis");
        options.DefaultLimit.Should().Be(100);
        options.DefaultWindowSeconds.Should().Be(60);
        options.KeyPrefix.Should().Be("ratelimit");
        options.ClientIdHeader.Should().Be("X-Client-Id");
        options.UseIpAsFallback.Should().BeTrue();
        options.IncludeUserId.Should().BeTrue();
    }

    [Fact]
    public void ExcludedPaths_ShouldContainDefaults()
    {
        var options = new RateLimitOptions();

        options.ExcludedPaths.Should().Contain("/health");
        options.ExcludedPaths.Should().Contain("/swagger");
        options.ExcludedPaths.Should().Contain("/metrics");
        options.ExcludedPaths.Should().Contain("/.well-known");
    }

    [Fact]
    public void Policies_ShouldDefaultToEmpty()
    {
        var options = new RateLimitOptions();
        options.Policies.Should().BeEmpty();
    }

    [Fact]
    public void Policies_ShouldBeSettable()
    {
        var options = new RateLimitOptions();
        options.Policies["search"] = new EndpointRateLimitPolicy
        {
            Pattern = "/api/vehicles/search",
            Limit = 30,
            WindowSeconds = 60,
            Enabled = true
        };

        options.Policies.Should().HaveCount(1);
        options.Policies["search"].Limit.Should().Be(30);
    }
}

public class EndpointRateLimitPolicyTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var policy = new EndpointRateLimitPolicy();

        policy.Pattern.Should().BeEmpty();
        policy.Limit.Should().Be(100);
        policy.WindowSeconds.Should().Be(60);
        policy.Enabled.Should().BeTrue();
        policy.TierLimits.Should().BeEmpty();
    }

    [Fact]
    public void TierLimits_ShouldBeSettable()
    {
        var policy = new EndpointRateLimitPolicy
        {
            TierLimits = new Dictionary<string, int>
            {
                ["free"] = 10,
                ["premium"] = 100,
                ["enterprise"] = 1000
            }
        };

        policy.TierLimits.Should().HaveCount(3);
        policy.TierLimits["premium"].Should().Be(100);
    }
}

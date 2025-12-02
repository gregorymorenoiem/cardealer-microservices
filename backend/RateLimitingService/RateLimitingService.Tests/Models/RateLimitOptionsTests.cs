using FluentAssertions;
using RateLimitingService.Core.Models;

namespace RateLimitingService.Tests.Models;

public class RateLimitOptionsTests
{
    [Fact]
    public void RateLimitOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new RateLimitOptions();

        // Assert
        options.Enabled.Should().BeTrue();
        options.DefaultLimit.Should().Be(100);
        options.DefaultWindowSeconds.Should().Be(60);
        options.KeyPrefix.Should().Be("ratelimit:");
        options.ClientIdHeader.Should().Be("X-API-Key");
        options.UserTierHeader.Should().Be("X-User-Tier");
        options.UseIpAsFallback.Should().BeTrue();
        options.IncludeHeaders.Should().BeTrue();
        options.RateLimitExceededMessage.Should().Be("Too many requests. Please try again later.");
        options.ExcludedPaths.Should().Contain("/health");
        options.ExcludedPaths.Should().Contain("/swagger");
        options.Policies.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void RateLimitOptions_SectionName_ShouldBeRateLimiting()
    {
        // Assert
        RateLimitOptions.SectionName.Should().Be("RateLimiting");
    }

    [Fact]
    public void RateLimitOptions_WithCustomValues_ShouldSetCorrectly()
    {
        // Arrange
        var options = new RateLimitOptions
        {
            Enabled = false,
            DefaultLimit = 200,
            DefaultWindowSeconds = 120,
            KeyPrefix = "custom:",
            ClientIdHeader = "X-Custom-Key",
            UserTierHeader = "X-Custom-Tier",
            UseIpAsFallback = false,
            IncludeHeaders = false,
            RateLimitExceededMessage = "Custom message",
            ExcludedPaths = new List<string> { "/custom" },
            Policies = new List<RateLimitPolicy>
            {
                new RateLimitPolicy { Name = "Test" }
            }
        };

        // Assert
        options.Enabled.Should().BeFalse();
        options.DefaultLimit.Should().Be(200);
        options.DefaultWindowSeconds.Should().Be(120);
        options.KeyPrefix.Should().Be("custom:");
        options.ClientIdHeader.Should().Be("X-Custom-Key");
        options.UserTierHeader.Should().Be("X-Custom-Tier");
        options.UseIpAsFallback.Should().BeFalse();
        options.IncludeHeaders.Should().BeFalse();
        options.RateLimitExceededMessage.Should().Be("Custom message");
        options.ExcludedPaths.Should().HaveCount(1);
        options.Policies.Should().HaveCount(1);
    }
}

public class UserTiersTests
{
    [Theory]
    [InlineData("free", 100)]
    [InlineData("basic", 500)]
    [InlineData("premium", 2000)]
    [InlineData("enterprise", 10000)]
    [InlineData("unlimited", int.MaxValue)]
    public void DefaultLimits_ShouldHaveCorrectValues(string tier, int expectedLimit)
    {
        // Assert
        UserTiers.DefaultLimits.Should().ContainKey(tier);
        UserTiers.DefaultLimits[tier].Should().Be(expectedLimit);
    }

    [Fact]
    public void UserTiers_Constants_ShouldBeCorrect()
    {
        // Assert
        UserTiers.Free.Should().Be("free");
        UserTiers.Basic.Should().Be("basic");
        UserTiers.Premium.Should().Be("premium");
        UserTiers.Enterprise.Should().Be("enterprise");
        UserTiers.Unlimited.Should().Be("unlimited");
    }
}

using FluentAssertions;
using RateLimitingService.Core.Models;

namespace RateLimitingService.Tests.Models;

public class RateLimitPolicyTests
{
    [Fact]
    public void RateLimitPolicy_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var policy = new RateLimitPolicy();

        // Assert
        policy.Id.Should().BeEmpty();
        policy.Name.Should().BeEmpty();
        policy.Description.Should().BeEmpty();
        policy.Tier.Should().Be(RateLimitTier.Free);
        policy.WindowSeconds.Should().Be(60);
        policy.MaxRequests.Should().Be(100);
        policy.BurstLimit.Should().Be(10);
        policy.Enabled.Should().BeTrue();
        policy.Endpoints.Should().NotBeNull().And.BeEmpty();
        policy.ExcludedIps.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData(RateLimitTier.Anonymous, 1)]
    [InlineData(RateLimitTier.Free, 2)]
    [InlineData(RateLimitTier.Basic, 3)]
    [InlineData(RateLimitTier.Premium, 4)]
    [InlineData(RateLimitTier.Enterprise, 5)]
    [InlineData(RateLimitTier.Unlimited, 6)]
    public void RateLimitTier_ShouldHaveCorrectValues(RateLimitTier tier, int expectedValue)
    {
        // Assert
        ((int)tier).Should().Be(expectedValue);
    }

    [Fact]
    public void RateLimitPolicy_WithValues_ShouldSetCorrectly()
    {
        // Arrange
        var policy = new RateLimitPolicy
        {
            Id = "test-id",
            Name = "Test Policy",
            Description = "Test description",
            Tier = RateLimitTier.Premium,
            WindowSeconds = 120,
            MaxRequests = 500,
            BurstLimit = 50,
            Enabled = false,
            Endpoints = new List<string> { "/api/test", "/api/data" },
            ExcludedIps = new List<string> { "192.168.1.1" }
        };

        // Assert
        policy.Id.Should().Be("test-id");
        policy.Name.Should().Be("Test Policy");
        policy.Description.Should().Be("Test description");
        policy.Tier.Should().Be(RateLimitTier.Premium);
        policy.WindowSeconds.Should().Be(120);
        policy.MaxRequests.Should().Be(500);
        policy.BurstLimit.Should().Be(50);
        policy.Enabled.Should().BeFalse();
        policy.Endpoints.Should().HaveCount(2);
        policy.ExcludedIps.Should().HaveCount(1);
    }

    [Fact]
    public void RateLimitPolicy_CreatedAt_ShouldBeSettable()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var policy = new RateLimitPolicy
        {
            CreatedAt = now,
            UpdatedAt = now.AddHours(1)
        };

        // Assert
        policy.CreatedAt.Should().Be(now);
        policy.UpdatedAt.Should().Be(now.AddHours(1));
    }
}

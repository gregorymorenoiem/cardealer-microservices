using FluentAssertions;
using RateLimitingService.Core.Models;

namespace RateLimitingService.Tests.Models;

public class RateLimitResultTests
{
    [Fact]
    public void Allowed_ShouldCreateAllowedResult()
    {
        // Arrange
        var clientId = "test-client";
        var policyName = "test-policy";
        var remaining = 99;
        var resetAt = DateTime.UtcNow.AddMinutes(1);
        var limit = 100;

        // Act
        var result = RateLimitResult.Allowed(clientId, policyName, remaining, resetAt, limit);

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.ClientIdentifier.Should().Be(clientId);
        result.PolicyName.Should().Be(policyName);
        result.RemainingRequests.Should().Be(remaining);
        result.ResetAt.Should().Be(resetAt);
        result.Limit.Should().Be(limit);
        result.RetryAfter.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void RateLimited_ShouldCreateRateLimitedResult()
    {
        // Arrange
        var clientId = "test-client";
        var policyName = "test-policy";
        var limit = 100;
        var resetAt = DateTime.UtcNow.AddMinutes(1);
        var retryAfter = TimeSpan.FromSeconds(30);

        // Act
        var result = RateLimitResult.RateLimited(clientId, policyName, limit, resetAt, retryAfter);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.ClientIdentifier.Should().Be(clientId);
        result.PolicyName.Should().Be(policyName);
        result.RemainingRequests.Should().Be(0);
        result.ResetAt.Should().Be(resetAt);
        result.Limit.Should().Be(limit);
        result.RetryAfter.Should().Be(retryAfter);
    }

    [Fact]
    public void GetHeaders_WhenAllowed_ShouldReturnCorrectHeaders()
    {
        // Arrange
        var result = RateLimitResult.Allowed("client", "policy", 50, DateTime.UtcNow.AddMinutes(1), 100);

        // Act
        var headers = result.GetHeaders();

        // Assert
        headers.Should().ContainKey("X-RateLimit-Limit");
        headers.Should().ContainKey("X-RateLimit-Remaining");
        headers.Should().ContainKey("X-RateLimit-Reset");
        headers["X-RateLimit-Limit"].Should().Be("100");
        headers["X-RateLimit-Remaining"].Should().Be("50");
    }

    [Fact]
    public void GetHeaders_WhenRateLimited_ShouldIncludeRetryAfter()
    {
        // Arrange
        var retryAfter = TimeSpan.FromSeconds(30);
        var result = RateLimitResult.RateLimited("client", "policy", 100, DateTime.UtcNow.AddMinutes(1), retryAfter);

        // Act
        var headers = result.GetHeaders();

        // Assert
        headers.Should().ContainKey("Retry-After");
        headers["Retry-After"].Should().Be("30");
    }

    [Fact]
    public void GetHeaders_WhenAllowed_ShouldNotIncludeRetryAfter()
    {
        // Arrange
        var result = RateLimitResult.Allowed("client", "policy", 50, DateTime.UtcNow.AddMinutes(1), 100);

        // Act
        var headers = result.GetHeaders();

        // Assert
        headers.Should().NotContainKey("Retry-After");
    }

    [Fact]
    public void RateLimitResult_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var result = new RateLimitResult();

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.RemainingRequests.Should().Be(0);
        result.Limit.Should().Be(0);
        result.ClientIdentifier.Should().BeEmpty();
        result.PolicyName.Should().BeEmpty();
    }
}

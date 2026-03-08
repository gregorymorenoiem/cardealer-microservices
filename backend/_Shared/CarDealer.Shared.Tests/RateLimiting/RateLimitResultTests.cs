using CarDealer.Shared.RateLimiting.Models;
using FluentAssertions;

namespace CarDealer.Shared.Tests.RateLimiting;

/// <summary>
/// Tests for RateLimitResult factory methods and header generation.
/// </summary>
public class RateLimitResultTests
{
    [Fact]
    public void Allowed_SetsCorrectFields()
    {
        var resetAt = DateTime.UtcNow.AddMinutes(1);

        var result = RateLimitResult.Allowed("192.168.1.1", "default", 99, resetAt, 100);

        result.IsAllowed.Should().BeTrue();
        result.ClientIdentifier.Should().Be("192.168.1.1");
        result.PolicyName.Should().Be("default");
        result.RemainingRequests.Should().Be(99);
        result.Limit.Should().Be(100);
        result.ResetAt.Should().Be(resetAt);
        result.RetryAfter.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Allowed_WithZeroLimit_ComputesFromRemaining()
    {
        var result = RateLimitResult.Allowed("client", "policy", 49, DateTime.UtcNow);

        // When limit is 0 (default), it uses remaining + 1
        result.Limit.Should().Be(50);
    }

    [Fact]
    public void RateLimited_SetsCorrectFields()
    {
        var resetAt = DateTime.UtcNow.AddMinutes(1);
        var retryAfter = TimeSpan.FromSeconds(60);

        var result = RateLimitResult.RateLimited("192.168.1.1", "strict", 100, resetAt, retryAfter);

        result.IsAllowed.Should().BeFalse();
        result.ClientIdentifier.Should().Be("192.168.1.1");
        result.PolicyName.Should().Be("strict");
        result.RemainingRequests.Should().Be(0);
        result.Limit.Should().Be(100);
        result.ResetAt.Should().Be(resetAt);
        result.RetryAfter.Should().Be(retryAfter);
    }

    [Fact]
    public void GetHeaders_Allowed_ReturnsThreeHeaders()
    {
        var resetAt = DateTime.UtcNow.AddMinutes(1);
        var result = RateLimitResult.Allowed("client", "default", 99, resetAt, 100);

        var headers = result.GetHeaders();

        headers.Should().ContainKey("X-RateLimit-Limit");
        headers["X-RateLimit-Limit"].Should().Be("100");
        headers.Should().ContainKey("X-RateLimit-Remaining");
        headers["X-RateLimit-Remaining"].Should().Be("99");
        headers.Should().ContainKey("X-RateLimit-Reset");
        headers.Should().NotContainKey("Retry-After");
    }

    [Fact]
    public void GetHeaders_RateLimited_IncludesRetryAfter()
    {
        var resetAt = DateTime.UtcNow.AddMinutes(1);
        var result = RateLimitResult.RateLimited("client", "strict", 100, resetAt, TimeSpan.FromSeconds(60));

        var headers = result.GetHeaders();

        headers.Should().ContainKey("Retry-After");
        headers["Retry-After"].Should().Be("60");
        headers["X-RateLimit-Remaining"].Should().Be("0");
    }

    [Fact]
    public void GetHeaders_Reset_IsUnixTimestamp()
    {
        var resetAt = new DateTime(2026, 3, 6, 12, 0, 0, DateTimeKind.Utc);
        var result = RateLimitResult.Allowed("client", "default", 99, resetAt, 100);

        var headers = result.GetHeaders();
        var expectedUnix = new DateTimeOffset(resetAt).ToUnixTimeSeconds().ToString();

        headers["X-RateLimit-Reset"].Should().Be(expectedUnix);
    }
}

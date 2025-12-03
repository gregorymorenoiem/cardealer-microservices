using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;
using RateLimitingService.Core.Services;

namespace RateLimitingService.Tests.Algorithms;

public class TokenBucketRateLimiterTests
{
    private readonly Mock<IRateLimitStorage> _storageMock;
    private readonly Mock<ILogger<TokenBucketRateLimiter>> _loggerMock;
    private readonly TokenBucketRateLimiter _rateLimiter;

    public TokenBucketRateLimiterTests()
    {
        _storageMock = new Mock<IRateLimitStorage>();
        _loggerMock = new Mock<ILogger<TokenBucketRateLimiter>>();
        _rateLimiter = new TokenBucketRateLimiter(_storageMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CheckAsync_FirstRequest_ShouldAllow()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Id = "test",
            Name = "Test Rule",
            Limit = 10,
            WindowSeconds = 60,
            Algorithm = RateLimitAlgorithm.TokenBucket
        };
        var key = "test-user";

        _storageMock.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(0L);
        _storageMock.Setup(s => s.SetAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _rateLimiter.CheckAsync(key, rule, cost: 1);

        // Assert
        result.Should().NotBeNull();
        result.IsAllowed.Should().BeTrue();
        result.Remaining.Should().Be(9);  // 10 - 1 = 9
        result.Limit.Should().Be(10);
    }

    [Fact]
    public async Task CheckAsync_ExceedLimit_ShouldDeny()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Id = "test",
            Name = "Test Rule",
            Limit = 5,
            WindowSeconds = 60,
            Algorithm = RateLimitAlgorithm.TokenBucket
        };
        var key = "test-user";
        var storageKey = $"ratelimit:tokenbucket:{key}";

        // Simulate bucket with 2 tokens remaining
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _storageMock.Setup(s => s.GetAsync($"{storageKey}:refill")).ReturnsAsync(now);
        _storageMock.Setup(s => s.GetAsync(storageKey)).ReturnsAsync(2L);

        // Act - Try to consume 3 tokens (should fail)
        var result = await _rateLimiter.CheckAsync(key, rule, cost: 3);

        // Assert
        result.Should().NotBeNull();
        result.IsAllowed.Should().BeFalse();
        result.Remaining.Should().Be(0);
        result.Limit.Should().Be(5);
        result.RetryAfterSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CheckAsync_AllowsBursts()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Id = "test",
            Name = "Test Rule",
            Limit = 100,
            WindowSeconds = 60,
            Algorithm = RateLimitAlgorithm.TokenBucket
        };
        var key = "burst-user";

        _storageMock.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(0L);
        _storageMock.Setup(s => s.SetAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act - Consume 50 tokens at once (burst)
        var result = await _rateLimiter.CheckAsync(key, rule, cost: 50);

        // Assert
        result.Should().NotBeNull();
        result.IsAllowed.Should().BeTrue();
        result.Remaining.Should().Be(50);  // 100 - 50 = 50 remaining
    }

    [Fact]
    public async Task CheckAsync_TokenRefill_ShouldAllowAfterTime()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Id = "test",
            Name = "Test Rule",
            Limit = 10,
            WindowSeconds = 60,  // 10 tokens per 60 seconds = 0.167 tokens/sec refill
            Algorithm = RateLimitAlgorithm.TokenBucket
        };
        var key = "refill-user";

        // Simulate: 2 tokens left, last update was 10 seconds ago
        var tenSecondsAgo = DateTimeOffset.UtcNow.AddSeconds(-10).ToUnixTimeSeconds();
        _storageMock.Setup(s => s.GetAsync($"{key}:tokens")).ReturnsAsync(2L);
        _storageMock.Setup(s => s.GetAsync($"{key}:last")).ReturnsAsync(tenSecondsAgo);
        _storageMock.Setup(s => s.SetAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act - Should have refilled ~1.67 tokens in 10 seconds
        var result = await _rateLimiter.CheckAsync(key, rule, cost: 1);

        // Assert
        result.Should().NotBeNull();
        result.IsAllowed.Should().BeTrue();
        // 2 + (10 * 10/60) - 1 = 2 + 1.67 - 1 = 2.67 tokens remaining
        result.Remaining.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task ResetAsync_ShouldClearKeys()
    {
        // Arrange
        var key = "reset-user";
        var storageKey = $"ratelimit:tokenbucket:{key}";

        _storageMock.Setup(s => s.DeleteAsync(storageKey)).Returns(Task.CompletedTask);
        _storageMock.Setup(s => s.DeleteAsync($"{storageKey}:refill")).Returns(Task.CompletedTask);

        // Act
        await _rateLimiter.ResetAsync(key);

        // Assert
        _storageMock.Verify(s => s.DeleteAsync(storageKey), Times.Once);
        _storageMock.Verify(s => s.DeleteAsync($"{storageKey}:refill"), Times.Once);
    }

    [Fact]
    public async Task GetStatusAsync_ShouldReturnCurrentTokens()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Id = "test",
            Name = "Test Rule",
            Limit = 20,
            WindowSeconds = 60,
            Algorithm = RateLimitAlgorithm.TokenBucket
        };
        var key = "status-user";
        var storageKey = $"ratelimit:tokenbucket:{key}";

        _storageMock.Setup(s => s.GetAsync(storageKey)).ReturnsAsync(15L);
        _storageMock.Setup(s => s.GetAsync($"{storageKey}:refill"))
            .ReturnsAsync(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        _storageMock.Setup(s => s.GetTtlAsync(It.IsAny<string>()))
            .ReturnsAsync(TimeSpan.FromSeconds(30));

        // Act
        var result = await _rateLimiter.GetStatusAsync(key, rule);

        // Assert
        result.Should().NotBeNull();
        result.IsAllowed.Should().BeTrue();
        result.Remaining.Should().BeGreaterThan(0);
        result.Limit.Should().Be(20);
    }
}

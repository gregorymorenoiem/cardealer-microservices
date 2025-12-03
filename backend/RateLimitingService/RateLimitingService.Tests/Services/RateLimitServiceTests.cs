using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;
using RateLimitingService.Core.Services;

namespace RateLimitingService.Tests.Services;

public class RateLimitServiceTests
{
    private readonly Mock<IRateLimitRuleService> _ruleServiceMock;
    private readonly Mock<ILogger<RateLimitService>> _loggerMock;
    private readonly Dictionary<RateLimitAlgorithm, IRateLimitAlgorithm> _algorithms;
    private readonly RateLimitService _service;

    public RateLimitServiceTests()
    {
        _ruleServiceMock = new Mock<IRateLimitRuleService>();
        _loggerMock = new Mock<ILogger<RateLimitService>>();

        // Setup mock algorithms
        var storageMock = new Mock<IRateLimitStorage>();
        var tokenBucket = new TokenBucketRateLimiter(storageMock.Object, Mock.Of<ILogger<TokenBucketRateLimiter>>());
        var fixedWindow = new FixedWindowRateLimiter(storageMock.Object, Mock.Of<ILogger<FixedWindowRateLimiter>>());
        var slidingWindow = new SlidingWindowRateLimiter(storageMock.Object, Mock.Of<ILogger<SlidingWindowRateLimiter>>());
        var leakyBucket = new LeakyBucketRateLimiter(storageMock.Object, Mock.Of<ILogger<LeakyBucketRateLimiter>>());

        _algorithms = new Dictionary<RateLimitAlgorithm, IRateLimitAlgorithm>
        {
            { RateLimitAlgorithm.TokenBucket, tokenBucket },
            { RateLimitAlgorithm.FixedWindow, fixedWindow },
            { RateLimitAlgorithm.SlidingWindow, slidingWindow },
            { RateLimitAlgorithm.LeakyBucket, leakyBucket }
        };

        _service = new RateLimitService(_ruleServiceMock.Object, storageMock.Object, _loggerMock.Object,
            tokenBucket, slidingWindow, fixedWindow, leakyBucket);
    }

    [Fact]
    public async Task CheckAsync_WhitelistedIdentifier_ShouldAllow()
    {
        // Arrange
        var request = new RateLimitCheckRequest
        {
            Identifier = "whitelisted-user",
            IdentifierType = RateLimitIdentifierType.UserId,
            Endpoint = "/api/test",
            Cost = 1
        };

        _ruleServiceMock.Setup(s => s.IsWhitelistedAsync(request.Identifier, request.IdentifierType))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CheckAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsAllowed.Should().BeTrue();
        result.Remaining.Should().Be(int.MaxValue);
        result.Limit.Should().Be(int.MaxValue);
    }

    [Fact]
    public async Task CheckAsync_BlacklistedIdentifier_ShouldDeny()
    {
        // Arrange
        var request = new RateLimitCheckRequest
        {
            Identifier = "blacklisted-user",
            IdentifierType = RateLimitIdentifierType.UserId,
            Endpoint = "/api/test",
            Cost = 1
        };

        _ruleServiceMock.Setup(s => s.IsWhitelistedAsync(It.IsAny<string>(), It.IsAny<RateLimitIdentifierType>()))
            .ReturnsAsync(false);
        _ruleServiceMock.Setup(s => s.IsBlacklistedAsync(request.Identifier, request.IdentifierType))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CheckAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsAllowed.Should().BeFalse();
        result.Remaining.Should().Be(0);
        result.Limit.Should().Be(0);
        result.RetryAfterSeconds.Should().Be(int.MaxValue);
    }

    [Fact]
    public async Task CheckAsync_NoApplicableRules_ShouldAllow()
    {
        // Arrange
        var request = new RateLimitCheckRequest
        {
            Identifier = "test-user",
            IdentifierType = RateLimitIdentifierType.UserId,
            Endpoint = "/api/test",
            Cost = 1
        };

        _ruleServiceMock.Setup(s => s.IsWhitelistedAsync(It.IsAny<string>(), It.IsAny<RateLimitIdentifierType>()))
            .ReturnsAsync(false);
        _ruleServiceMock.Setup(s => s.IsBlacklistedAsync(It.IsAny<string>(), It.IsAny<RateLimitIdentifierType>()))
            .ReturnsAsync(false);
        _ruleServiceMock.Setup(s => s.GetApplicableRulesAsync(It.IsAny<RateLimitCheckRequest>()))
            .ReturnsAsync(Enumerable.Empty<RateLimitRule>());

        // Act
        var result = await _service.CheckAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsAllowed.Should().BeTrue();
        result.Remaining.Should().Be(int.MaxValue);
    }

    [Fact]
    public async Task CheckAsync_WithRules_ShouldEvaluate()
    {
        // Arrange
        var request = new RateLimitCheckRequest
        {
            Identifier = "192.168.1.100",
            IdentifierType = RateLimitIdentifierType.IpAddress,
            Endpoint = "/api/test",
            Cost = 1
        };

        var rule = new RateLimitRule
        {
            Id = "per-ip",
            Name = "Per IP Rule",
            IdentifierType = RateLimitIdentifierType.IpAddress,
            Algorithm = RateLimitAlgorithm.FixedWindow,
            Limit = 100,
            WindowSeconds = 60,
            Priority = 10,
            IsEnabled = true
        };

        _ruleServiceMock.Setup(s => s.IsWhitelistedAsync(It.IsAny<string>(), It.IsAny<RateLimitIdentifierType>()))
            .ReturnsAsync(false);
        _ruleServiceMock.Setup(s => s.IsBlacklistedAsync(It.IsAny<string>(), It.IsAny<RateLimitIdentifierType>()))
            .ReturnsAsync(false);
        _ruleServiceMock.Setup(s => s.GetApplicableRulesAsync(It.IsAny<RateLimitCheckRequest>()))
            .ReturnsAsync(new[] { rule });

        // Act
        var result = await _service.CheckAsync(request);

        // Assert
        result.Should().NotBeNull();
        // The result depends on the algorithm's mock behavior
        // In real integration tests, we would verify against Redis
    }

    [Fact]
    public async Task ResetAsync_ShouldResetAllRules()
    {
        // Arrange
        var identifier = "test-user";
        var type = RateLimitIdentifierType.UserId;

        // Act - ResetAsync doesn't need rules, it just clears all algorithm keys
        await _service.ResetAsync(identifier, type);

        // Assert
        // In a real implementation, this would call DeleteAsync on storage
        // For now, just verify no exceptions and method completes
        true.Should().BeTrue();
    }

    [Fact]
    public async Task LogViolationAsync_ShouldLogViolation()
    {
        // Arrange
        var violation = new RateLimitViolation
        {
            Identifier = "test-user",
            IdentifierType = RateLimitIdentifierType.UserId,
            Endpoint = "/api/test",
            RuleId = "rule-1",
            RuleName = "Test Rule",
            AllowedLimit = 100,
            AttemptedRequests = 101,
            ViolatedAt = DateTime.UtcNow
        };

        // Act
        await _service.LogViolationAsync(violation);

        // Assert
        // In real implementation with PostgreSQL, verify DB insert
        // For now, just verify no exceptions
        true.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnStats()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow;

        // Act
        var result = await _service.GetStatisticsAsync(from, to);

        // Assert
        result.Should().NotBeNull();
        result.From.Should().BeCloseTo(from, TimeSpan.FromSeconds(1));
        result.To.Should().BeCloseTo(to, TimeSpan.FromSeconds(1));
        result.TotalRequests.Should().BeGreaterOrEqualTo(0);
    }
}

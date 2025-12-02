using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;
using RateLimitingService.Core.Services;
using StackExchange.Redis;

namespace RateLimitingService.Tests.Services;

public class RedisRateLimitingServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _mockRedis;
    private readonly Mock<IDatabase> _mockDatabase;
    private readonly Mock<IServer> _mockServer;
    private readonly Mock<ILogger<RedisRateLimitingService>> _mockLogger;
    private readonly IOptions<RateLimitOptions> _options;

    public RedisRateLimitingServiceTests()
    {
        _mockRedis = new Mock<IConnectionMultiplexer>();
        _mockDatabase = new Mock<IDatabase>();
        _mockServer = new Mock<IServer>();
        _mockLogger = new Mock<ILogger<RedisRateLimitingService>>();
        _options = Options.Create(new RateLimitOptions
        {
            Enabled = true,
            DefaultLimit = 100,
            DefaultWindowSeconds = 60,
            KeyPrefix = "ratelimit:"
        });

        _mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_mockDatabase.Object);
        _mockRedis.Setup(x => x.GetEndPoints(It.IsAny<bool>()))
            .Returns(new System.Net.EndPoint[] { new System.Net.IPEndPoint(0, 0) });
        _mockRedis.Setup(x => x.GetServer(It.IsAny<System.Net.EndPoint>(), It.IsAny<object>()))
            .Returns(_mockServer.Object);
    }

    [Fact]
    public void Constructor_WithNullRedis_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RedisRateLimitingService(null!, _mockLogger.Object, _options));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RedisRateLimitingService(_mockRedis.Object, null!, _options));
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, null!));
    }

    [Fact]
    public async Task CheckRateLimitAsync_WithNullClientId_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CheckRateLimitAsync(null!, "/api/test"));
    }

    [Fact]
    public async Task CheckRateLimitAsync_WhenDisabled_ReturnsAllowed()
    {
        // Arrange
        var disabledOptions = Options.Create(new RateLimitOptions { Enabled = false });
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, disabledOptions);

        // Act
        var result = await service.CheckRateLimitAsync("client1", "/api/test");

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.RemainingRequests.Should().Be(int.MaxValue);
    }

    [Fact]
    public async Task GetPolicyAsync_WithNullId_ReturnsNull()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);
        _mockDatabase.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await service.GetPolicyAsync("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreatePolicyAsync_WithNullPolicy_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreatePolicyAsync(null!));
    }

    [Fact]
    public async Task CreatePolicyAsync_WithValidPolicy_SetsIdAndTimestamps()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);
        var policy = new RateLimitPolicy { Name = "Test Policy" };

        _mockDatabase.Setup(x => x.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await service.CreatePolicyAsync(policy);

        // Assert
        result.Id.Should().NotBeNullOrEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdatePolicyAsync_WithNullPolicy_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.UpdatePolicyAsync(null!));
    }

    [Fact]
    public async Task UpdatePolicyAsync_WithNoId_ThrowsArgumentException()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);
        var policy = new RateLimitPolicy { Name = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdatePolicyAsync(policy));
    }

    [Fact]
    public async Task DeletePolicyAsync_WithNullId_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.DeletePolicyAsync(null!));
    }

    [Fact]
    public async Task DeletePolicyAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);
        _mockDatabase.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await service.DeletePolicyAsync("test-id");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetClientUsageAsync_WithNullClientId_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.GetClientUsageAsync(null!));
    }

    [Fact]
    public async Task GetClientUsageAsync_WhenNoUsage_ReturnsNull()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);
        _mockDatabase.Setup(x => x.SortedSetLengthAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<Exclude>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(0);

        // Act
        var result = await service.GetClientUsageAsync("unknown-client");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResetClientLimitAsync_WithNullClientId_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.ResetClientLimitAsync(null!));
    }

    [Fact]
    public async Task ResetClientLimitAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);
        _mockDatabase.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await service.ResetClientLimitAsync("client1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsHealthyAsync_WhenPingSucceeds_ReturnsTrue()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);
        _mockDatabase.Setup(x => x.PingAsync(It.IsAny<CommandFlags>()))
            .ReturnsAsync(TimeSpan.FromMilliseconds(10));

        // Act
        var result = await service.IsHealthyAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsHealthyAsync_WhenPingFails_ReturnsFalse()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);
        _mockDatabase.Setup(x => x.PingAsync(It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Connection failed"));

        // Act
        var result = await service.IsHealthyAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsHealthyAsync_WhenPingSlow_ReturnsFalse()
    {
        // Arrange
        var service = new RedisRateLimitingService(_mockRedis.Object, _mockLogger.Object, _options);
        _mockDatabase.Setup(x => x.PingAsync(It.IsAny<CommandFlags>()))
            .ReturnsAsync(TimeSpan.FromMilliseconds(2000));

        // Act
        var result = await service.IsHealthyAsync();

        // Assert
        result.Should().BeFalse();
    }
}

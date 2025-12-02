using System.Text;
using System.Text.Json;
using FluentAssertions;
using IdempotencyService.Core.Interfaces;
using IdempotencyService.Core.Models;
using IdempotencyService.Core.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace IdempotencyService.Tests;

public class RedisIdempotencyServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<RedisIdempotencyService>> _loggerMock;
    private readonly IOptions<IdempotencyOptions> _options;
    private readonly RedisIdempotencyService _service;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisIdempotencyServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<RedisIdempotencyService>>();
        _options = Options.Create(new IdempotencyOptions());
        _service = new RedisIdempotencyService(_cacheMock.Object, _options, _loggerMock.Object);
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    [Fact]
    public async Task CheckAsync_WhenKeyNotFound_ReturnsNotFound()
    {
        // Arrange
        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _service.CheckAsync("test-key");

        // Assert
        result.Should().NotBeNull();
        result.Exists.Should().BeFalse();
        result.IsProcessing.Should().BeFalse();
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task CheckAsync_WhenKeyExists_ReturnsCompleted()
    {
        // Arrange
        var record = new IdempotencyRecord
        {
            Key = "test-key",
            Status = IdempotencyStatus.Completed,
            ResponseStatusCode = 200,
            ResponseBody = "{\"id\":1}"
        };
        var json = JsonSerializer.Serialize(record, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _service.CheckAsync("test-key");

        // Assert
        result.Should().NotBeNull();
        result.Exists.Should().BeTrue();
        result.IsCompleted.Should().BeTrue();
        result.Record.Should().NotBeNull();
        result.Record!.ResponseStatusCode.Should().Be(200);
    }

    [Fact]
    public async Task CheckAsync_WhenKeyIsProcessing_ReturnsProcessing()
    {
        // Arrange
        var record = new IdempotencyRecord
        {
            Key = "test-key",
            Status = IdempotencyStatus.Processing
        };
        var json = JsonSerializer.Serialize(record, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _service.CheckAsync("test-key");

        // Assert
        result.Should().NotBeNull();
        result.Exists.Should().BeTrue();
        result.IsProcessing.Should().BeTrue();
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task CheckAsync_WhenRequestHashDoesNotMatch_ReturnsConflict()
    {
        // Arrange
        var record = new IdempotencyRecord
        {
            Key = "test-key",
            Status = IdempotencyStatus.Completed,
            RequestHash = "original-hash"
        };
        var json = JsonSerializer.Serialize(record, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _service.CheckAsync("test-key", "different-hash");

        // Assert
        result.Should().NotBeNull();
        result.Exists.Should().BeTrue();
        result.RequestHashMatches.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task StartProcessingAsync_CreatesRecordInCache()
    {
        // Arrange
        var record = new IdempotencyRecord
        {
            Key = "new-key",
            HttpMethod = "POST",
            Path = "/api/orders"
        };

        // Act
        var result = await _service.StartProcessingAsync(record);

        // Assert
        result.Should().BeTrue();
        _cacheMock.Verify(x => x.SetAsync(
            It.Is<string>(k => k.Contains("new-key")),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_UpdatesRecordWithResponse()
    {
        // Arrange
        var record = new IdempotencyRecord
        {
            Key = "test-key",
            Status = IdempotencyStatus.Processing
        };
        var json = JsonSerializer.Serialize(record, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _service.CompleteAsync("test-key", 201, "{\"id\":123}");

        // Assert
        result.Should().BeTrue();
        _cacheMock.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b).Contains("\"status\":1")), // Completed = 1
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FailAsync_UpdatesRecordStatus()
    {
        // Arrange
        var record = new IdempotencyRecord
        {
            Key = "test-key",
            Status = IdempotencyStatus.Processing
        };
        var json = JsonSerializer.Serialize(record, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _service.FailAsync("test-key", "Something went wrong");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_ReturnsRecord()
    {
        // Arrange
        var record = new IdempotencyRecord
        {
            Key = "test-key",
            HttpMethod = "POST",
            Path = "/api/orders",
            Status = IdempotencyStatus.Completed
        };
        var json = JsonSerializer.Serialize(record, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _service.GetAsync("test-key");

        // Assert
        result.Should().NotBeNull();
        result!.Key.Should().Be("test-key");
        result.Status.Should().Be(IdempotencyStatus.Completed);
    }

    [Fact]
    public async Task GetAsync_WhenKeyNotFound_ReturnsNull()
    {
        // Arrange
        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _service.GetAsync("nonexistent-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesFromCache()
    {
        // Act
        var result = await _service.DeleteAsync("test-key");

        // Assert
        result.Should().BeTrue();
        _cacheMock.Verify(x => x.RemoveAsync(
            It.Is<string>(k => k.Contains("test-key")),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsStats()
    {
        // Arrange
        _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes("5"));

        // Act
        var result = await _service.GetStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.DuplicateRequestsBlocked.Should().Be(5);
    }
}

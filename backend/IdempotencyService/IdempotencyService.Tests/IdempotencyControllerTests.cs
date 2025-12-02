using FluentAssertions;
using IdempotencyService.Api.Controllers;
using IdempotencyService.Core.Interfaces;
using IdempotencyService.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace IdempotencyService.Tests;

public class IdempotencyControllerTests
{
    private readonly Mock<IIdempotencyService> _serviceMock;
    private readonly Mock<ILogger<IdempotencyController>> _loggerMock;
    private readonly IdempotencyController _controller;

    public IdempotencyControllerTests()
    {
        _serviceMock = new Mock<IIdempotencyService>();
        _loggerMock = new Mock<ILogger<IdempotencyController>>();
        _controller = new IdempotencyController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetRecord_WhenExists_ReturnsOk()
    {
        // Arrange
        var record = new IdempotencyRecord
        {
            Key = "test-key",
            Status = IdempotencyStatus.Completed
        };
        _serviceMock.Setup(x => x.GetAsync("test-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        var result = await _controller.GetRecord("test-key", CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedRecord = okResult.Value.Should().BeOfType<IdempotencyRecord>().Subject;
        returnedRecord.Key.Should().Be("test-key");
    }

    [Fact]
    public async Task GetRecord_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(x => x.GetAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyRecord?)null);

        // Act
        var result = await _controller.GetRecord("nonexistent", CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CheckKey_WithValidKey_ReturnsResult()
    {
        // Arrange
        var checkResult = IdempotencyCheckResult.NotFound();
        _serviceMock.Setup(x => x.CheckAsync("test-key", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkResult);

        var request = new CheckKeyRequest { Key = "test-key" };

        // Act
        var result = await _controller.CheckKey(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResult = okResult.Value.Should().BeOfType<IdempotencyCheckResult>().Subject;
        returnedResult.Exists.Should().BeFalse();
    }

    [Fact]
    public async Task CheckKey_WithEmptyKey_ReturnsBadRequest()
    {
        // Arrange
        var request = new CheckKeyRequest { Key = "" };

        // Act
        var result = await _controller.CheckKey(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteRecord_WhenExists_ReturnsNoContent()
    {
        // Arrange
        var record = new IdempotencyRecord { Key = "test-key" };
        _serviceMock.Setup(x => x.GetAsync("test-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _serviceMock.Setup(x => x.DeleteAsync("test-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteRecord("test-key", CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteRecord_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(x => x.GetAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyRecord?)null);

        // Act
        var result = await _controller.DeleteRecord("nonexistent", CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetStats_ReturnsStats()
    {
        // Arrange
        var stats = new IdempotencyStats
        {
            DuplicateRequestsBlocked = 10,
            TotalRecords = 100
        };
        _serviceMock.Setup(x => x.GetStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStats(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<IdempotencyStats>().Subject;
        returnedStats.DuplicateRequestsBlocked.Should().Be(10);
    }

    [Fact]
    public async Task CreateRecord_WithValidData_ReturnsCreated()
    {
        // Arrange
        _serviceMock.Setup(x => x.GetAsync("new-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyRecord?)null);
        _serviceMock.Setup(x => x.StartProcessingAsync(It.IsAny<IdempotencyRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _serviceMock.Setup(x => x.CompleteAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreateRecordRequest
        {
            Key = "new-key",
            ResponseStatusCode = 200,
            ResponseBody = "{}"
        };

        // Act
        var result = await _controller.CreateRecord(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateRecord_WithEmptyKey_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateRecordRequest { Key = "" };

        // Act
        var result = await _controller.CreateRecord(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateRecord_WhenKeyExists_ReturnsConflict()
    {
        // Arrange
        var existingRecord = new IdempotencyRecord { Key = "existing-key" };
        _serviceMock.Setup(x => x.GetAsync("existing-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRecord);

        var request = new CreateRecordRequest { Key = "existing-key" };

        // Act
        var result = await _controller.CreateRecord(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Cleanup_ReturnsCleanupResult()
    {
        // Arrange
        _serviceMock.Setup(x => x.CleanupExpiredAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _controller.Cleanup(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var cleanupResult = okResult.Value.Should().BeOfType<CleanupResult>().Subject;
        cleanupResult.RemovedCount.Should().Be(5);
    }
}

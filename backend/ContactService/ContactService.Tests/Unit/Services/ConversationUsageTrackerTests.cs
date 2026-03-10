using CarDealer.Contracts.Enums;
using ContactService.Domain.Entities;
using ContactService.Domain.Interfaces;
using ContactService.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace ContactService.Tests.Unit.Services;

/// <summary>
/// Tests for ConversationUsageTracker — CONTRA #5 fix.
/// Tests the DB-fallback path (no Redis) to validate core logic.
/// </summary>
public class ConversationUsageTrackerTests
{
    private readonly Mock<IContactRequestRepository> _mockRepo;
    private readonly Mock<ILogger<ConversationUsageTracker>> _mockLogger;
    private readonly ConversationUsageTracker _tracker;

    public ConversationUsageTrackerTests()
    {
        _mockRepo = new Mock<IContactRequestRepository>();
        _mockLogger = new Mock<ILogger<ConversationUsageTracker>>();

        // No Redis → falls back to DB counting
        _tracker = new ConversationUsageTracker(
            _mockRepo.Object,
            _mockLogger.Object,
            redis: null);
    }

    [Fact]
    public async Task IncrementAndCheck_ElitePlan_NormalUsage_ReturnsNormal()
    {
        // Arrange: dealer with 100 conversations this month
        var dealerId = Guid.NewGuid();
        _mockRepo.Setup(r => r.CountBySellerIdInPeriodAsync(
            dealerId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        // Act
        var result = await _tracker.IncrementAndCheckAsync(dealerId, "elite");

        // Assert
        result.Status.Should().Be(ConversationUsageStatus.Normal);
        result.MaxAllowed.Should().Be(2000);
        result.OverageCount.Should().Be(0);
    }

    [Fact]
    public async Task IncrementAndCheck_ElitePlan_At1600_ReturnsWarning()
    {
        // Arrange: dealer at exactly 1,600 (80% threshold)
        var dealerId = Guid.NewGuid();
        _mockRepo.Setup(r => r.CountBySellerIdInPeriodAsync(
            dealerId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1600);

        // Act
        var result = await _tracker.IncrementAndCheckAsync(dealerId, "elite");

        // Assert
        result.Status.Should().Be(ConversationUsageStatus.WarningThreshold);
        result.MaxAllowed.Should().Be(2000);
        result.OverageCount.Should().Be(0);
    }

    [Fact]
    public async Task IncrementAndCheck_ElitePlan_At2000_ReturnsLimitReached()
    {
        // Arrange: dealer at exactly 2,000 (hard limit)
        var dealerId = Guid.NewGuid();
        _mockRepo.Setup(r => r.CountBySellerIdInPeriodAsync(
            dealerId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2000);

        // Act
        var result = await _tracker.IncrementAndCheckAsync(dealerId, "elite");

        // Assert
        result.Status.Should().Be(ConversationUsageStatus.LimitReached);
        result.MaxAllowed.Should().Be(2000);
    }

    [Fact]
    public async Task IncrementAndCheck_ElitePlan_Over2000_ReturnsOverageCount()
    {
        // Arrange: dealer at 2,150 conversations (150 over limit)
        var dealerId = Guid.NewGuid();
        _mockRepo.Setup(r => r.CountBySellerIdInPeriodAsync(
            dealerId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2150);

        // Act
        var result = await _tracker.IncrementAndCheckAsync(dealerId, "elite");

        // Assert
        result.Status.Should().Be(ConversationUsageStatus.LimitReached);
        result.OverageCount.Should().Be(150);
    }

    [Fact]
    public async Task IncrementAndCheck_LibrePlan_ReturnsNoAccess()
    {
        // Arrange: Libre plan (no ChatAgent access)
        var dealerId = Guid.NewGuid();

        // Act
        var result = await _tracker.IncrementAndCheckAsync(dealerId, "libre");

        // Assert
        result.Status.Should().Be(ConversationUsageStatus.NoAccess);
        result.MaxAllowed.Should().Be(0);
    }

    [Fact]
    public async Task IsInBasicMode_ElitePlan_Below2000_ReturnsFalse()
    {
        var dealerId = Guid.NewGuid();
        _mockRepo.Setup(r => r.CountBySellerIdInPeriodAsync(
            dealerId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1999);

        var isBasicMode = await _tracker.IsInBasicModeAsync(dealerId, "elite");
        isBasicMode.Should().BeFalse();
    }

    [Fact]
    public async Task IsInBasicMode_ElitePlan_At2000_ReturnsTrue()
    {
        var dealerId = Guid.NewGuid();
        _mockRepo.Setup(r => r.CountBySellerIdInPeriodAsync(
            dealerId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2000);

        var isBasicMode = await _tracker.IsInBasicModeAsync(dealerId, "elite");
        isBasicMode.Should().BeTrue();
    }

    [Fact]
    public async Task IsInBasicMode_LibrePlan_AlwaysTrue()
    {
        var dealerId = Guid.NewGuid();
        var isBasicMode = await _tracker.IsInBasicModeAsync(dealerId, "libre");
        isBasicMode.Should().BeTrue("Libre plan has no ChatAgent access");
    }

    [Fact]
    public async Task GetOverageCount_ElitePlan_At2100_Returns100()
    {
        var dealerId = Guid.NewGuid();
        _mockRepo.Setup(r => r.CountBySellerIdInPeriodAsync(
            dealerId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2100);

        var overage = await _tracker.GetOverageCountAsync(dealerId, "elite");
        overage.Should().Be(100);
    }

    [Fact]
    public async Task GetOverageCount_ElitePlan_At1500_ReturnsZero()
    {
        var dealerId = Guid.NewGuid();
        _mockRepo.Setup(r => r.CountBySellerIdInPeriodAsync(
            dealerId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1500);

        var overage = await _tracker.GetOverageCountAsync(dealerId, "elite");
        overage.Should().Be(0);
    }

    [Fact]
    public async Task GetCurrentMonthCount_NoRedis_FallsBackToDatabase()
    {
        var dealerId = Guid.NewGuid();
        _mockRepo.Setup(r => r.CountBySellerIdInPeriodAsync(
            dealerId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(750);

        var count = await _tracker.GetCurrentMonthCountAsync(dealerId);
        count.Should().Be(750);
    }
}

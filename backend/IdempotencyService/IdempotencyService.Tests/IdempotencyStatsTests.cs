using FluentAssertions;
using IdempotencyService.Core.Interfaces;

namespace IdempotencyService.Tests;

/// <summary>
/// Unit tests for IdempotencyStats
/// </summary>
public class IdempotencyStatsTests
{
    [Fact]
    public void DefaultStats_HasZeroValues()
    {
        // Act
        var stats = new IdempotencyStats();

        // Assert
        stats.TotalRecords.Should().Be(0);
        stats.ProcessingRecords.Should().Be(0);
        stats.CompletedRecords.Should().Be(0);
        stats.FailedRecords.Should().Be(0);
        stats.DuplicateRequestsBlocked.Should().Be(0);
    }

    [Fact]
    public void LastUpdated_IsSetToUtcNow()
    {
        // Act
        var before = DateTime.UtcNow;
        var stats = new IdempotencyStats();
        var after = DateTime.UtcNow;

        // Assert
        stats.LastUpdated.Should().BeOnOrAfter(before);
        stats.LastUpdated.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void CanSetAllProperties()
    {
        // Arrange
        var lastUpdated = DateTime.UtcNow.AddMinutes(-5);

        // Act
        var stats = new IdempotencyStats
        {
            TotalRecords = 1000,
            ProcessingRecords = 50,
            CompletedRecords = 900,
            FailedRecords = 50,
            DuplicateRequestsBlocked = 150,
            LastUpdated = lastUpdated
        };

        // Assert
        stats.TotalRecords.Should().Be(1000);
        stats.ProcessingRecords.Should().Be(50);
        stats.CompletedRecords.Should().Be(900);
        stats.FailedRecords.Should().Be(50);
        stats.DuplicateRequestsBlocked.Should().Be(150);
        stats.LastUpdated.Should().Be(lastUpdated);
    }

    [Fact]
    public void RecordCounts_CanBeSummed()
    {
        // Arrange
        var stats = new IdempotencyStats
        {
            ProcessingRecords = 10,
            CompletedRecords = 80,
            FailedRecords = 10
        };

        // Act
        var total = stats.ProcessingRecords + stats.CompletedRecords + stats.FailedRecords;

        // Assert
        total.Should().Be(100);
    }

    [Fact]
    public void DuplicateRequestsBlocked_CanBeVeryLarge()
    {
        // Act
        var stats = new IdempotencyStats
        {
            DuplicateRequestsBlocked = long.MaxValue
        };

        // Assert
        stats.DuplicateRequestsBlocked.Should().Be(long.MaxValue);
    }
}

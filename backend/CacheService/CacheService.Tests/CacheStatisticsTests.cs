using Xunit;
using FluentAssertions;
using CacheService.Domain;

namespace CacheService.Tests;

public class CacheStatisticsTests
{
    [Fact]
    public void GetHitRatio_WithNoHitsOrMisses_ReturnsZero()
    {
        // Arrange
        var stats = new CacheStatistics();

        // Act
        var result = stats.GetHitRatio();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetHitRatio_WithHitsAndMisses_ReturnsCorrectRatio()
    {
        // Arrange
        var stats = new CacheStatistics
        {
            TotalHits = 75,
            TotalMisses = 25
        };

        // Act
        var result = stats.GetHitRatio();

        // Assert
        result.Should().Be(0.75);
    }

    [Fact]
    public void GetHitPercentage_ReturnsCorrectPercentage()
    {
        // Arrange
        var stats = new CacheStatistics
        {
            TotalHits = 80,
            TotalMisses = 20
        };

        // Act
        var result = stats.GetHitPercentage();

        // Assert
        result.Should().Be(80);
    }

    [Fact]
    public void RecordHit_IncrementsHitCountAndTracksKey()
    {
        // Arrange
        var stats = new CacheStatistics();
        var key = "test-key";

        // Act
        stats.RecordHit(key);
        stats.RecordHit(key);

        // Assert
        stats.TotalHits.Should().Be(2);
        stats.HitsByKey[key].Should().Be(2);
    }

    [Fact]
    public void RecordMiss_IncrementsMissCountAndTracksKey()
    {
        // Arrange
        var stats = new CacheStatistics();
        var key = "test-key";

        // Act
        stats.RecordMiss(key);

        // Assert
        stats.TotalMisses.Should().Be(1);
        stats.MissesByKey[key].Should().Be(1);
    }

    [Fact]
    public void RecordSet_IncrementsSetCount()
    {
        // Arrange
        var stats = new CacheStatistics();

        // Act
        stats.RecordSet();
        stats.RecordSet();

        // Assert
        stats.TotalSets.Should().Be(2);
    }

    [Fact]
    public void RecordDelete_IncrementsDeleteCount()
    {
        // Arrange
        var stats = new CacheStatistics();

        // Act
        stats.RecordDelete();

        // Assert
        stats.TotalDeletes.Should().Be(1);
    }

    [Fact]
    public void Reset_ClearsAllStatistics()
    {
        // Arrange
        var stats = new CacheStatistics
        {
            TotalHits = 100,
            TotalMisses = 50,
            TotalSets = 75,
            TotalDeletes = 25
        };
        stats.HitsByKey["key1"] = 50;
        stats.MissesByKey["key2"] = 30;

        // Act
        stats.Reset();

        // Assert
        stats.TotalHits.Should().Be(0);
        stats.TotalMisses.Should().Be(0);
        stats.TotalSets.Should().Be(0);
        stats.TotalDeletes.Should().Be(0);
        stats.HitsByKey.Should().BeEmpty();
        stats.MissesByKey.Should().BeEmpty();
        stats.LastResetAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}

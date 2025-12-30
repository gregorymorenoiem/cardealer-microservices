using Xunit;
using FluentAssertions;
using CacheService.Domain;

namespace CacheService.Tests;

public class CacheEntryTests
{
    [Fact]
    public void IsExpired_WithNoExpiration_ReturnsFalse()
    {
        // Arrange
        var entry = new CacheEntry
        {
            Key = "test-key",
            Value = "test-value",
            ExpiresAt = null
        };

        // Act
        var result = entry.IsExpired();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithFutureExpiration_ReturnsFalse()
    {
        // Arrange
        var entry = new CacheEntry
        {
            Key = "test-key",
            Value = "test-value",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var result = entry.IsExpired();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithPastExpiration_ReturnsTrue()
    {
        // Arrange
        var entry = new CacheEntry
        {
            Key = "test-key",
            Value = "test-value",
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        // Act
        var result = entry.IsExpired();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetRemainingTtl_WithNoExpiration_ReturnsNull()
    {
        // Arrange
        var entry = new CacheEntry
        {
            Key = "test-key",
            Value = "test-value",
            ExpiresAt = null
        };

        // Act
        var result = entry.GetRemainingTtl();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetRemainingTtl_WithFutureExpiration_ReturnsPositiveTimeSpan()
    {
        // Arrange
        var entry = new CacheEntry
        {
            Key = "test-key",
            Value = "test-value",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        // Act
        var result = entry.GetRemainingTtl();

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().BeGreaterThan(TimeSpan.Zero);
        result.Value.TotalMinutes.Should().BeApproximately(30, 0.1);
    }

    [Fact]
    public void RecordAccess_IncrementsAccessCountAndUpdatesLastAccessedAt()
    {
        // Arrange
        var entry = new CacheEntry
        {
            Key = "test-key",
            Value = "test-value",
            AccessCount = 0,
            LastAccessedAt = null
        };

        // Act
        entry.RecordAccess();

        // Assert
        entry.AccessCount.Should().Be(1);
        entry.LastAccessedAt.Should().NotBeNull();
        entry.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}

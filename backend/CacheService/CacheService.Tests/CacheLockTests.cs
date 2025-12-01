using Xunit;
using FluentAssertions;
using CacheService.Domain;

namespace CacheService.Tests;

public class CacheLockTests
{
    [Fact]
    public void IsExpired_WithFutureExpiration_ReturnsFalse()
    {
        // Arrange
        var cacheLock = new CacheLock
        {
            Key = "resource-lock",
            OwnerId = "owner-1",
            ExpiresAt = DateTime.UtcNow.AddSeconds(30)
        };

        // Act
        var result = cacheLock.IsExpired();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithPastExpiration_ReturnsTrue()
    {
        // Arrange
        var cacheLock = new CacheLock
        {
            Key = "resource-lock",
            OwnerId = "owner-1",
            ExpiresAt = DateTime.UtcNow.AddSeconds(-1)
        };

        // Act
        var result = cacheLock.IsExpired();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithNonExpiredLock_ReturnsTrue()
    {
        // Arrange
        var cacheLock = new CacheLock
        {
            Key = "resource-lock",
            OwnerId = "owner-1",
            ExpiresAt = DateTime.UtcNow.AddSeconds(30)
        };

        // Act
        var result = cacheLock.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetRemainingTime_WithFutureExpiration_ReturnsPositiveTimeSpan()
    {
        // Arrange
        var cacheLock = new CacheLock
        {
            Key = "resource-lock",
            OwnerId = "owner-1",
            ExpiresAt = DateTime.UtcNow.AddSeconds(30)
        };

        // Act
        var result = cacheLock.GetRemainingTime();

        // Assert
        result.Should().BeGreaterThan(TimeSpan.Zero);
        result.TotalSeconds.Should().BeApproximately(30, 1);
    }

    [Fact]
    public void Renew_UpdatesExpirationAndIncrementsRenewCount()
    {
        // Arrange
        var ttl = TimeSpan.FromSeconds(30);
        var cacheLock = new CacheLock
        {
            Key = "resource-lock",
            OwnerId = "owner-1",
            AcquiredAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddSeconds(10),
            Ttl = ttl,
            RenewCount = 0
        };

        // Act
        cacheLock.Renew();

        // Assert
        cacheLock.RenewCount.Should().Be(1);
        cacheLock.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(ttl), TimeSpan.FromSeconds(1));
    }
}

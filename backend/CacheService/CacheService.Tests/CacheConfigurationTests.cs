using Xunit;
using FluentAssertions;
using CacheService.Domain;

namespace CacheService.Tests;

public class CacheConfigurationTests
{
    [Fact]
    public void IsValidTtl_WithPositiveTtlWithinLimit_ReturnsTrue()
    {
        // Arrange
        var config = new CacheConfiguration
        {
            MaxTtlSeconds = 86400
        };

        // Act
        var result = config.IsValidTtl(3600);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidTtl_WithTtlExceedingMax_ReturnsFalse()
    {
        // Arrange
        var config = new CacheConfiguration
        {
            MaxTtlSeconds = 3600
        };

        // Act
        var result = config.IsValidTtl(7200);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidTtl_WithZeroTtl_ReturnsFalse()
    {
        // Arrange
        var config = new CacheConfiguration();

        // Act
        var result = config.IsValidTtl(0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetDefaultTtl_ReturnsConfiguredDefault()
    {
        // Arrange
        var config = new CacheConfiguration
        {
            DefaultTtlSeconds = 3600
        };

        // Act
        var result = config.GetDefaultTtl();

        // Assert
        result.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void GetMaxTtl_ReturnsConfiguredMax()
    {
        // Arrange
        var config = new CacheConfiguration
        {
            MaxTtlSeconds = 86400
        };

        // Act
        var result = config.GetMaxTtl();

        // Assert
        result.Should().Be(TimeSpan.FromDays(1));
    }
}

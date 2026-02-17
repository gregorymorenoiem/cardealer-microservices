using BackgroundRemovalService.Domain.Entities;
using BackgroundRemovalService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace BackgroundRemovalService.Tests;

public class ProviderConfigurationTests
{
    [Fact]
    public void ProviderConfiguration_ShouldBeAvailable_WhenEnabledAndNoLimitsReached()
    {
        // Arrange
        var config = new ProviderConfiguration
        {
            Id = Guid.NewGuid(),
            Provider = BackgroundRemovalProvider.RemoveBg,
            IsEnabled = true,
            RequestsUsedToday = 0,
            RateLimitPerDay = 1000,
            IsCircuitBreakerOpen = false
        };

        // Act
        var isAvailable = config.IsAvailable();

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public void ProviderConfiguration_ShouldNotBeAvailable_WhenDisabled()
    {
        // Arrange
        var config = new ProviderConfiguration
        {
            Id = Guid.NewGuid(),
            Provider = BackgroundRemovalProvider.RemoveBg,
            IsEnabled = false,
            RateLimitPerDay = 1000
        };

        // Act
        var isAvailable = config.IsAvailable();

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public void ProviderConfiguration_ShouldNotBeAvailable_WhenDailyLimitReached()
    {
        // Arrange
        var config = new ProviderConfiguration
        {
            Id = Guid.NewGuid(),
            Provider = BackgroundRemovalProvider.RemoveBg,
            IsEnabled = true,
            RequestsUsedToday = 1000,
            RateLimitPerDay = 1000
        };

        // Act
        var isAvailable = config.IsAvailable();

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public void ProviderConfiguration_ShouldNotBeAvailable_WhenCircuitBreakerOpen()
    {
        // Arrange
        var config = new ProviderConfiguration
        {
            Id = Guid.NewGuid(),
            Provider = BackgroundRemovalProvider.RemoveBg,
            IsEnabled = true,
            RateLimitPerDay = 1000,
            IsCircuitBreakerOpen = true,
            CircuitBreakerResetAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act
        var isAvailable = config.IsAvailable();

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public void ProviderConfiguration_ShouldBeAvailable_WhenCircuitBreakerExpired()
    {
        // Arrange
        var config = new ProviderConfiguration
        {
            Id = Guid.NewGuid(),
            Provider = BackgroundRemovalProvider.RemoveBg,
            IsEnabled = true,
            RateLimitPerDay = 1000,
            IsCircuitBreakerOpen = true,
            CircuitBreakerResetAt = DateTime.UtcNow.AddMinutes(-1) // Expired
        };

        // Act
        var isAvailable = config.IsAvailable();

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public void ProviderConfiguration_RecordSuccess_ShouldUpdateStats()
    {
        // Arrange
        var config = new ProviderConfiguration
        {
            Id = Guid.NewGuid(),
            Provider = BackgroundRemovalProvider.RemoveBg,
            ConsecutiveFailures = 3,
            TotalRequestsProcessed = 10,
            RequestsUsedToday = 5,
            AverageResponseTimeMs = 100
        };
        var responseTimeMs = 200L;

        // Act
        config.RecordSuccess(responseTimeMs);

        // Assert
        config.ConsecutiveFailures.Should().Be(0);
        config.TotalRequestsProcessed.Should().Be(11);
        config.RequestsUsedToday.Should().Be(6);
        config.IsCircuitBreakerOpen.Should().BeFalse();
    }

    [Fact]
    public void ProviderConfiguration_RecordFailure_ShouldIncrementCounters()
    {
        // Arrange
        var config = new ProviderConfiguration
        {
            Id = Guid.NewGuid(),
            Provider = BackgroundRemovalProvider.RemoveBg,
            ConsecutiveFailures = 2,
            TotalRequestsProcessed = 5
        };

        // Act
        config.RecordFailure();

        // Assert
        config.ConsecutiveFailures.Should().Be(3);
        config.TotalRequestsProcessed.Should().Be(6);
    }

    [Fact]
    public void ProviderConfiguration_RecordFailure_ShouldOpenCircuitBreakerAfter5Failures()
    {
        // Arrange
        var config = new ProviderConfiguration
        {
            Id = Guid.NewGuid(),
            Provider = BackgroundRemovalProvider.RemoveBg,
            ConsecutiveFailures = 4,
            TotalRequestsProcessed = 10
        };

        // Act
        config.RecordFailure();

        // Assert
        config.ConsecutiveFailures.Should().Be(5);
        config.IsCircuitBreakerOpen.Should().BeTrue();
        config.CircuitBreakerResetAt.Should().NotBeNull();
        config.CircuitBreakerResetAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ProviderConfiguration_ResetDailyCounter_ShouldResetWhenNewDay()
    {
        // Arrange
        var config = new ProviderConfiguration
        {
            Id = Guid.NewGuid(),
            Provider = BackgroundRemovalProvider.RemoveBg,
            RequestsUsedToday = 100,
            LastDailyReset = DateTime.UtcNow.Date.AddDays(-1)
        };

        // Act
        config.ResetDailyCounter();

        // Assert
        config.RequestsUsedToday.Should().Be(0);
        config.LastDailyReset.Should().Be(DateTime.UtcNow.Date);
    }

    [Theory]
    [InlineData(BackgroundRemovalProvider.RemoveBg)]
    [InlineData(BackgroundRemovalProvider.Photoroom)]
    [InlineData(BackgroundRemovalProvider.Slazzer)]
    [InlineData(BackgroundRemovalProvider.ClippingMagic)]
    [InlineData(BackgroundRemovalProvider.Local)]
    public void BackgroundRemovalProvider_Enum_ShouldHaveExpectedValues(BackgroundRemovalProvider provider)
    {
        // Assert
        Enum.IsDefined(typeof(BackgroundRemovalProvider), provider).Should().BeTrue();
    }
}

using ConfigurationService.Application.Interfaces;
using ConfigurationService.Domain.Entities;
using ConfigurationService.Infrastructure.Data;
using ConfigurationService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationService.Tests;

public class FeatureFlagManagerTests : IDisposable
{
    private readonly ConfigurationDbContext _context;
    private readonly IFeatureFlagManager _manager;

    public FeatureFlagManagerTests()
    {
        var options = new DbContextOptionsBuilder<ConfigurationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ConfigurationDbContext(options);
        _manager = new FeatureFlagManager(_context);
    }

    [Fact]
    public async Task CreateFeatureFlag_ShouldCreateSuccessfully()
    {
        // Arrange
        var flag = new FeatureFlag
        {
            Name = "New Feature",
            Key = "new_feature",
            IsEnabled = true,
            Environment = "Dev",
            CreatedBy = "TestUser"
        };

        // Act
        var result = await _manager.CreateFeatureFlagAsync(flag);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.RolloutPercentage.Should().Be(100);
    }

    [Fact]
    public async Task IsFeatureEnabled_WhenEnabled_ShouldReturnTrue()
    {
        // Arrange
        var flag = new FeatureFlag
        {
            Name = "Enabled Feature",
            Key = "enabled_feature",
            IsEnabled = true,
            Environment = "Dev",
            CreatedBy = "TestUser"
        };
        await _manager.CreateFeatureFlagAsync(flag);

        // Act
        var result = await _manager.IsFeatureEnabledAsync("enabled_feature", "Dev");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsFeatureEnabled_WhenDisabled_ShouldReturnFalse()
    {
        // Arrange
        var flag = new FeatureFlag
        {
            Name = "Disabled Feature",
            Key = "disabled_feature",
            IsEnabled = false,
            Environment = "Dev",
            CreatedBy = "TestUser"
        };
        await _manager.CreateFeatureFlagAsync(flag);

        // Act
        var result = await _manager.IsFeatureEnabledAsync("disabled_feature", "Dev");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsFeatureEnabled_WithRollout50Percent_ShouldReturnConsistentResult()
    {
        // Arrange
        var flag = new FeatureFlag
        {
            Name = "Rollout Feature",
            Key = "rollout_feature",
            IsEnabled = true,
            RolloutPercentage = 50,
            Environment = "Dev",
            CreatedBy = "TestUser"
        };
        await _manager.CreateFeatureFlagAsync(flag);

        // Act - Test same user multiple times
        var result1 = await _manager.IsFeatureEnabledAsync("rollout_feature", "Dev", userId: "user123");
        var result2 = await _manager.IsFeatureEnabledAsync("rollout_feature", "Dev", userId: "user123");

        // Assert - Same user should get same result
        result1.Should().Be(result2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

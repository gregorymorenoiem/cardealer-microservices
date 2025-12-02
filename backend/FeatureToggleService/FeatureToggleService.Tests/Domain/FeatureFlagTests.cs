using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using FluentAssertions;
using Xunit;
using Environment = FeatureToggleService.Domain.Enums.Environment;

namespace FeatureToggleService.Tests.Domain;

public class FeatureFlagTests
{
    [Fact]
    public void Create_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var flag = new FeatureFlag
        {
            Key = "test-feature",
            Name = "Test Feature",
            Description = "A test feature flag"
        };

        // Assert
        flag.Id.Should().NotBeEmpty();
        flag.Key.Should().Be("test-feature");
        flag.Status.Should().Be(FlagStatus.Draft);
        flag.IsEnabled.Should().BeFalse();
        flag.RolloutPercentage.Should().Be(0);
        flag.Environment.Should().Be(Environment.All);
        flag.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Enable_ShouldSetIsEnabledToTrue()
    {
        // Arrange
        var flag = new FeatureFlag { Key = "test", Name = "Test" };

        // Act
        flag.Enable("admin");

        // Assert
        flag.IsEnabled.Should().BeTrue();
        flag.Status.Should().Be(FlagStatus.Active);
        flag.ModifiedBy.Should().Be("admin");
    }

    [Fact]
    public void Disable_ShouldSetIsEnabledToFalse()
    {
        // Arrange
        var flag = new FeatureFlag { Key = "test", Name = "Test" };
        flag.Enable("admin");

        // Act
        flag.Disable("admin");

        // Assert
        flag.IsEnabled.Should().BeFalse();
        flag.Status.Should().Be(FlagStatus.Inactive);
    }

    [Fact]
    public void Archive_ShouldSetStatusToArchived()
    {
        // Arrange
        var flag = new FeatureFlag { Key = "test", Name = "Test" };

        // Act
        flag.Archive("admin");

        // Assert
        flag.Status.Should().Be(FlagStatus.Archived);
        flag.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void TriggerKillSwitch_ShouldDisableAndSetFlag()
    {
        // Arrange
        var flag = new FeatureFlag { Key = "test", Name = "Test" };
        flag.Enable("admin");

        // Act
        flag.TriggerKillSwitch("admin", "Emergency");

        // Assert
        flag.IsEnabled.Should().BeFalse();
        flag.KillSwitchTriggered.Should().BeTrue();
        flag.Status.Should().Be(FlagStatus.Inactive);
    }

    [Fact]
    public void SetRolloutPercentage_ShouldUpdatePercentage()
    {
        // Arrange
        var flag = new FeatureFlag { Key = "test", Name = "Test" };

        // Act
        flag.SetRolloutPercentage(50, "admin");

        // Assert
        flag.RolloutPercentage.Should().Be(50);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void SetRolloutPercentage_WithInvalidPercentage_ShouldThrow(int percentage)
    {
        // Arrange
        var flag = new FeatureFlag { Key = "test", Name = "Test" };

        // Act & Assert
        var action = () => flag.SetRolloutPercentage(percentage, "admin");
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddTargetUsers_ShouldAddUsersToList()
    {
        // Arrange
        var flag = new FeatureFlag { Key = "test", Name = "Test" };

        // Act
        flag.AddTargetUsers(new List<string> { "user1", "user2" }, "admin");

        // Assert
        flag.TargetUserIds.Should().Contain("user1");
        flag.TargetUserIds.Should().Contain("user2");
        flag.TargetUserIds.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveTargetUsers_ShouldRemoveUsersFromList()
    {
        // Arrange
        var flag = new FeatureFlag { Key = "test", Name = "Test" };
        flag.AddTargetUsers(new List<string> { "user1", "user2", "user3" }, "admin");

        // Act
        flag.RemoveTargetUsers(new List<string> { "user2" }, "admin");

        // Assert
        flag.TargetUserIds.Should().NotContain("user2");
        flag.TargetUserIds.Should().HaveCount(2);
    }

    [Fact]
    public void IsExpired_ShouldReturnTrueWhenExpiresAtIsPast()
    {
        // Arrange
        var flag = new FeatureFlag 
        { 
            Key = "test", 
            Name = "Test",
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        flag.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ShouldReturnFalseWhenExpiresAtIsFuture()
    {
        // Arrange
        var flag = new FeatureFlag 
        { 
            Key = "test", 
            Name = "Test",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        // Act & Assert
        flag.IsExpired.Should().BeFalse();
    }
}

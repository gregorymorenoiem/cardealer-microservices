using Xunit;
using FluentAssertions;
using UserBehaviorService.Domain.Entities;

namespace UserBehaviorService.Tests;

public class UserBehaviorProfileTests
{
    [Fact]
    public void UserBehaviorProfile_ShouldBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var profile = new UserBehaviorProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        profile.Should().NotBeNull();
        profile.UserSegment.Should().Be("Unknown");
        profile.EngagementScore.Should().Be(0);
        profile.PurchaseIntentScore.Should().Be(0);
        profile.PreferredMakes.Should().BeEmpty();
        profile.TotalSearches.Should().Be(0);
    }

    [Fact]
    public void IsHighIntentBuyer_ShouldReturnTrue_WhenScoreIs70OrAbove()
    {
        // Arrange
        var profile = new UserBehaviorProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PurchaseIntentScore = 70
        };

        // Act
        var result = profile.IsHighIntentBuyer();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsHighIntentBuyer_ShouldReturnFalse_WhenScoreIsBelow70()
    {
        // Arrange
        var profile = new UserBehaviorProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PurchaseIntentScore = 69
        };

        // Act
        var result = profile.IsHighIntentBuyer();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsActiveRecently_ShouldReturnTrue_WhenLastActivityWithin7Days()
    {
        // Arrange
        var profile = new UserBehaviorProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            LastActivityAt = DateTime.UtcNow.AddDays(-5)
        };

        // Act
        var result = profile.IsActiveRecently();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsActiveRecently_ShouldReturnFalse_WhenLastActivityOver7Days()
    {
        // Arrange
        var profile = new UserBehaviorProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            LastActivityAt = DateTime.UtcNow.AddDays(-8)
        };

        // Act
        var result = profile.IsActiveRecently();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasStrongPreferences_ShouldReturnTrue_WhenHasMultipleMakes()
    {
        // Arrange
        var profile = new UserBehaviorProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PreferredMakes = new List<string> { "Toyota", "Honda" }
        };

        // Act
        var result = profile.HasStrongPreferences();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasStrongPreferences_ShouldReturnTrue_WhenHasMultipleModels()
    {
        // Arrange
        var profile = new UserBehaviorProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PreferredModels = new List<string> { "Corolla", "Civic" }
        };

        // Act
        var result = profile.HasStrongPreferences();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasStrongPreferences_ShouldReturnFalse_WhenHasOnlyOneMake()
    {
        // Arrange
        var profile = new UserBehaviorProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PreferredMakes = new List<string> { "Toyota" }
        };

        // Act
        var result = profile.HasStrongPreferences();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UserAction_ShouldBeCreated_WithRequiredFields()
    {
        // Arrange & Act
        var action = new UserAction
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ActionType = "Search",
            ActionDetails = "Toyota Corolla 2020",
            Timestamp = DateTime.UtcNow,
            SessionId = "session123",
            DeviceType = "Desktop"
        };

        // Assert
        action.Should().NotBeNull();
        action.ActionType.Should().Be("Search");
        action.DeviceType.Should().Be("Desktop");
    }

    [Fact]
    public void UserAction_CanHaveOptionalFields()
    {
        // Arrange & Act
        var vehicleId = Guid.NewGuid();
        var action = new UserAction
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ActionType = "VehicleView",
            ActionDetails = "Viewed vehicle details",
            RelatedVehicleId = vehicleId,
            SearchQuery = "Toyota Corolla",
            Timestamp = DateTime.UtcNow,
            SessionId = "session456",
            DeviceType = "Mobile"
        };

        // Assert
        action.RelatedVehicleId.Should().Be(vehicleId);
        action.SearchQuery.Should().Be("Toyota Corolla");
    }
}

using Xunit;
using FluentAssertions;
using FeatureStoreService.Domain.Entities;

namespace FeatureStoreService.Tests;

public class FeatureStoreTests
{
    [Fact]
    public void UserFeature_ShouldBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var feature = new UserFeature
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FeatureName = "preferred_make",
            FeatureValue = "Toyota",
            ComputedAt = DateTime.UtcNow
        };

        // Assert
        feature.Should().NotBeNull();
        feature.FeatureType.Should().Be("Numeric");
        feature.Version.Should().Be(1);
        feature.Source.Should().Be("System");
    }

    [Fact]
    public void VehicleFeature_ShouldBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var feature = new VehicleFeature
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            FeatureName = "view_count",
            FeatureValue = "150",
            ComputedAt = DateTime.UtcNow
        };

        // Assert
        feature.Should().NotBeNull();
        feature.FeatureType.Should().Be("Numeric");
        feature.Version.Should().Be(1);
        feature.Source.Should().Be("System");
    }

    [Fact]
    public void FeatureDefinition_ShouldBeCreated_WithRequiredFields()
    {
        // Arrange & Act
        var definition = new FeatureDefinition
        {
            Id = Guid.NewGuid(),
            FeatureName = "user_engagement_score",
            Category = "Behavioral",
            Description = "User engagement score based on actions",
            FeatureType = "Numeric",
            ComputationLogic = "SUM(actions) / days_active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        definition.Should().NotBeNull();
        definition.IsActive.Should().BeTrue();
        definition.RefreshIntervalHours.Should().Be(24);
    }

    [Fact]
    public void FeatureBatch_ShouldBeCreated_WithStatus()
    {
        // Arrange & Act
        var batch = new FeatureBatch
        {
            Id = Guid.NewGuid(),
            BatchName = "daily_user_features",
            FeatureNames = new List<string> { "engagement_score", "purchase_intent" },
            TargetEntity = "User",
            StartedAt = DateTime.UtcNow
        };

        // Assert
        batch.Should().NotBeNull();
        batch.Status.Should().Be("Running");
        batch.TotalEntitiesProcessed.Should().Be(0);
    }

    [Fact]
    public void UserFeature_CanHaveExpiration()
    {
        // Arrange & Act
        var feature = new UserFeature
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FeatureName = "temp_feature",
            FeatureValue = "value",
            ComputedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Assert
        feature.ExpiresAt.Should().NotBeNull();
        feature.ExpiresAt.Value.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void VehicleFeature_CanHaveExpiration()
    {
        // Arrange & Act
        var feature = new VehicleFeature
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            FeatureName = "temp_feature",
            FeatureValue = "value",
            ComputedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(6)
        };

        // Assert
        feature.ExpiresAt.Should().NotBeNull();
        feature.ExpiresAt.Value.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void FeatureDefinition_CanBeInactive()
    {
        // Arrange & Act
        var definition = new FeatureDefinition
        {
            Id = Guid.NewGuid(),
            FeatureName = "deprecated_feature",
            Category = "Legacy",
            Description = "Old feature no longer used",
            FeatureType = "Numeric",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        definition.IsActive.Should().BeFalse();
    }

    [Fact]
    public void FeatureBatch_CanBeCompleted()
    {
        // Arrange
        var batch = new FeatureBatch
        {
            Id = Guid.NewGuid(),
            BatchName = "completed_batch",
            FeatureNames = new List<string> { "feature1" },
            TargetEntity = "User",
            StartedAt = DateTime.UtcNow.AddHours(-2),
            Status = "Running"
        };

        // Act
        batch.Status = "Completed";
        batch.CompletedAt = DateTime.UtcNow;
        batch.TotalEntitiesProcessed = 1000;

        // Assert
        batch.Status.Should().Be("Completed");
        batch.CompletedAt.Should().NotBeNull();
        batch.TotalEntitiesProcessed.Should().Be(1000);
    }
}

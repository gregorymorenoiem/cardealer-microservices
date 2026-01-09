using System;
using System.Collections.Generic;
using FluentAssertions;
using RecommendationService.Domain.Entities;
using Xunit;

namespace RecommendationService.Tests;

public class RecommendationServiceTests
{
    [Fact]
    public void Recommendation_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var type = RecommendationType.ForYou;
        var score = 0.85;
        var reason = "Based on your preferences";

        // Act
        var recommendation = new Recommendation(userId, vehicleId, type, score, reason);

        // Assert
        recommendation.Should().NotBeNull();
        recommendation.Id.Should().NotBeEmpty();
        recommendation.UserId.Should().Be(userId);
        recommendation.VehicleId.Should().Be(vehicleId);
        recommendation.Type.Should().Be(type);
        recommendation.Score.Should().Be(score);
        recommendation.Reason.Should().Be(reason);
        recommendation.IsRelevant.Should().BeTrue();
        recommendation.ViewedAt.Should().BeNull();
        recommendation.ClickedAt.Should().BeNull();
    }

    [Fact]
    public void Recommendation_MarkViewed_ShouldSetViewedAt()
    {
        // Arrange
        var recommendation = new Recommendation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            RecommendationType.Similar,
            0.9,
            "Similar vehicle"
        );

        // Act
        recommendation.MarkViewed();

        // Assert
        recommendation.ViewedAt.Should().NotBeNull();
        recommendation.ViewedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Recommendation_MarkClicked_ShouldSetClickedAt()
    {
        // Arrange
        var recommendation = new Recommendation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            RecommendationType.AlsoViewed,
            0.75,
            "Users also viewed"
        );

        // Act
        recommendation.MarkClicked();

        // Assert
        recommendation.ClickedAt.Should().NotBeNull();
        recommendation.ClickedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Recommendation_MarkNotRelevant_ShouldSetIsRelevantToFalse()
    {
        // Arrange
        var recommendation = new Recommendation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            RecommendationType.Popular,
            0.6,
            "Popular vehicle"
        );

        // Act
        recommendation.MarkNotRelevant();

        // Assert
        recommendation.IsRelevant.Should().BeFalse();
    }

    [Fact]
    public void UserPreference_ShouldBeCreated_WithDefaultValues()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var preference = new UserPreference(userId);

        // Assert
        preference.Should().NotBeNull();
        preference.Id.Should().NotBeEmpty();
        preference.UserId.Should().Be(userId);
        preference.PreferredMakes.Should().BeEmpty();
        preference.PreferredModels.Should().BeEmpty();
        preference.Confidence.Should().Be(0.0);
        preference.TotalVehiclesViewed.Should().Be(0);
        preference.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        preference.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void UserPreference_CalculateConfidence_ShouldIncreaseWithInteractions()
    {
        // Arrange
        var preference = new UserPreference(Guid.NewGuid())
        {
            TotalVehiclesViewed = 10,
            TotalSearches = 5,
            TotalFavorites = 3,
            TotalContacts = 2
        };

        // Act
        preference.CalculateConfidence();

        // Assert
        // Total: 10 + (5*2) + (3*3) + (2*5) = 10 + 10 + 9 + 10 = 39
        // Confidence: 39/100 = 0.39
        preference.Confidence.Should().BeApproximately(0.39, 0.01);
    }

    [Fact]
    public void UserPreference_CalculateConfidence_ShouldCapAt1()
    {
        // Arrange
        var preference = new UserPreference(Guid.NewGuid())
        {
            TotalVehiclesViewed = 50,
            TotalSearches = 30,
            TotalFavorites = 20,
            TotalContacts = 15
        };

        // Act
        preference.CalculateConfidence();

        // Assert
        preference.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void UserPreference_UpdateTimestamp_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var preference = new UserPreference(Guid.NewGuid());
        var originalUpdatedAt = preference.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(100); // Wait a bit
        preference.UpdateTimestamp();

        // Assert
        preference.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void VehicleInteraction_ShouldBeCreated_WithCorrectType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var type = InteractionType.View;
        var source = "search";

        // Act
        var interaction = new VehicleInteraction(userId, vehicleId, type, source);

        // Assert
        interaction.Should().NotBeNull();
        interaction.Id.Should().NotBeEmpty();
        interaction.UserId.Should().Be(userId);
        interaction.VehicleId.Should().Be(vehicleId);
        interaction.Type.Should().Be(type);
        interaction.Source.Should().Be(source);
        interaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        interaction.DurationSeconds.Should().Be(0);
    }

    [Fact]
    public void RecommendationType_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<RecommendationType>().Should().Contain(new[]
        {
            RecommendationType.ForYou,
            RecommendationType.Similar,
            RecommendationType.AlsoViewed,
            RecommendationType.Popular,
            RecommendationType.Trending,
            RecommendationType.RecentlyViewed
        });
    }

    [Fact]
    public void InteractionType_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<InteractionType>().Should().Contain(new[]
        {
            InteractionType.View,
            InteractionType.Favorite,
            InteractionType.Contact,
            InteractionType.Share,
            InteractionType.Compare
        });
    }

    [Fact]
    public void Recommendation_Metadata_ShouldBeInitialized()
    {
        // Arrange & Act
        var recommendation = new Recommendation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            RecommendationType.Trending,
            0.8,
            "Trending vehicle"
        );

        // Assert
        recommendation.Metadata.Should().NotBeNull();
        recommendation.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Recommendation_Metadata_ShouldStoreCustomData()
    {
        // Arrange
        var recommendation = new Recommendation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            RecommendationType.Similar,
            0.95,
            "Similar specs"
        );

        // Act
        recommendation.Metadata["similarityScore"] = 0.95;
        recommendation.Metadata["matchedFeatures"] = new[] { "make", "model", "year" };

        // Assert
        recommendation.Metadata.Should().ContainKey("similarityScore");
        recommendation.Metadata.Should().ContainKey("matchedFeatures");
        recommendation.Metadata["similarityScore"].Should().Be(0.95);
    }

    [Fact]
    public void UserPreference_AddPreferredMake_ShouldWork()
    {
        // Arrange
        var preference = new UserPreference(Guid.NewGuid());

        // Act
        preference.PreferredMakes.Add("Toyota");
        preference.PreferredMakes.Add("Honda");

        // Assert
        preference.PreferredMakes.Should().HaveCount(2);
        preference.PreferredMakes.Should().Contain(new[] { "Toyota", "Honda" });
    }

    [Fact]
    public void UserPreference_SetPriceRange_ShouldWork()
    {
        // Arrange
        var preference = new UserPreference(Guid.NewGuid());

        // Act
        preference.MinPrice = 10000;
        preference.MaxPrice = 30000;

        // Assert
        preference.MinPrice.Should().Be(10000);
        preference.MaxPrice.Should().Be(30000);
    }
}

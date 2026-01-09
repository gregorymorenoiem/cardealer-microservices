using Xunit;

using FluentAssertions;
using ReviewService.Domain.Entities;

namespace ReviewService.Tests.Domain.Entities;

/// <summary>
/// Tests para SellerBadge entity
/// </summary>
public class SellerBadgeTests
{
    [Fact]
    public void SellerBadge_ShouldBeCreatedSuccessfully_WithValidData()
    {
        // Arrange & Act
        var badge = new SellerBadge
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BadgeType = BadgeType.TopRated,
            GrantedAt = DateTime.UtcNow,
            IsActive = true,
            Notes = "Achieved 4.5+ rating with 20+ reviews",
            QualifyingStats = "{\"averageRating\": 4.6, \"totalReviews\": 25}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        badge.Id.Should().NotBe(Guid.Empty);
        badge.SellerId.Should().NotBe(Guid.Empty);
        badge.BadgeType.Should().Be(BadgeType.TopRated);
        badge.IsActive.Should().BeTrue();
        badge.RevokedAt.Should().BeNull();
        badge.Notes.Should().Be("Achieved 4.5+ rating with 20+ reviews");
        badge.QualifyingStats.Should().Contain("averageRating");
    }

    [Theory]
    [InlineData(BadgeType.TopRated)]
    [InlineData(BadgeType.TrustedDealer)]
    [InlineData(BadgeType.FiveStarSeller)]
    [InlineData(BadgeType.CustomerChoice)]
    [InlineData(BadgeType.QuickResponder)]
    [InlineData(BadgeType.VerifiedProfessional)]
    public void SellerBadge_ShouldSupportAllBadgeTypes(BadgeType badgeType)
    {
        // Arrange & Act
        var badge = new SellerBadge
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BadgeType = badgeType,
            GrantedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Assert
        badge.BadgeType.Should().Be(badgeType);
    }

    [Fact]
    public void SellerBadge_ShouldAllowRevocation()
    {
        // Arrange
        var badge = new SellerBadge
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BadgeType = BadgeType.TrustedDealer,
            GrantedAt = DateTime.UtcNow.AddDays(-30),
            IsActive = true
        };

        // Act
        badge.IsActive = false;
        badge.RevokedAt = DateTime.UtcNow;

        // Assert
        badge.IsActive.Should().BeFalse();
        badge.RevokedAt.Should().NotBeNull();
        badge.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SellerBadge_ShouldStoreQualifyingStatsAsJson()
    {
        // Arrange & Act
        var stats = "{\"averageRating\": 4.8, \"totalReviews\": 45, \"responseRate\": 95}";
        var badge = new SellerBadge
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BadgeType = BadgeType.ResponsiveSeller,
            GrantedAt = DateTime.UtcNow,
            IsActive = true,
            QualifyingStats = stats
        };

        // Assert
        badge.QualifyingStats.Should().Be(stats);
        badge.QualifyingStats.Should().Contain("averageRating");
        badge.QualifyingStats.Should().Contain("totalReviews");
        badge.QualifyingStats.Should().Contain("responseRate");
    }
}
using Xunit;

using FluentAssertions;
using Moq;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Interfaces;
using ReviewService.Domain.Services;

namespace ReviewService.Tests.Domain.Services;

/// <summary>
/// Tests para BadgeCalculationService
/// </summary>
public class BadgeCalculationServiceTests
{
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly Mock<ISellerBadgeRepository> _sellerBadgeRepositoryMock;
    private readonly BadgeCalculationService _service;

    public BadgeCalculationServiceTests()
    {
        _reviewRepositoryMock = new Mock<IReviewRepository>();
        _sellerBadgeRepositoryMock = new Mock<ISellerBadgeRepository>();
        _service = new BadgeCalculationService(_reviewRepositoryMock.Object, _sellerBadgeRepositoryMock.Object);
    }

    [Fact]
    public async Task IsEligibleForBadgeAsync_ShouldReturnTrue_ForTopRatedEligibleSeller()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var stats = new SellerReviewStats
        {
            AverageRating = 4.6m,
            TotalReviews = 25,
            ResponseRate = 80m
        };

        _reviewRepositoryMock.Setup(x => x.GetSellerStatsAsync(sellerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _service.IsEligibleForBadgeAsync(sellerId, BadgeType.TopRated);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEligibleForBadgeAsync_ShouldReturnFalse_ForTopRatedIneligibleSeller()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var stats = new SellerReviewStats
        {
            AverageRating = 4.2m, // Below 4.5 threshold
            TotalReviews = 25,
            ResponseRate = 80m
        };

        _reviewRepositoryMock.Setup(x => x.GetSellerStatsAsync(sellerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _service.IsEligibleForBadgeAsync(sellerId, BadgeType.TopRated);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEligibleForBadgeAsync_ShouldReturnTrue_ForTrustedDealerEligibleSeller()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var stats = new SellerReviewStats
        {
            AverageRating = 4.5m,
            TotalReviews = 60,
            ResponseRate = 92m
        };

        _reviewRepositoryMock.Setup(x => x.GetSellerStatsAsync(sellerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _service.IsEligibleForBadgeAsync(sellerId, BadgeType.TrustedDealer);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEligibleForBadgeAsync_ShouldReturnTrue_ForFiveStarSellerEligibleSeller()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var stats = new SellerReviewStats
        {
            AverageRating = 4.8m,
            TotalReviews = 15,
            FiveStarCount = 12, // 80% five-star reviews
            ResponseRate = 85m
        };

        _reviewRepositoryMock.Setup(x => x.GetSellerStatsAsync(sellerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _service.IsEligibleForBadgeAsync(sellerId, BadgeType.FiveStarSeller);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEligibleForBadgeAsync_ShouldReturnTrue_ForResponsiveSellerEligibleSeller()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var stats = new SellerReviewStats
        {
            AverageRating = 4.0m,
            TotalReviews = 20,
            ResponseRate = 97m // Above 95% threshold
        };

        _reviewRepositoryMock.Setup(x => x.GetSellerStatsAsync(sellerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _service.IsEligibleForBadgeAsync(sellerId, BadgeType.ResponsiveSeller);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CalculateAndGrantBadgesAsync_ShouldGrantEligibleBadges_AndSkipExisting()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var stats = new SellerReviewStats
        {
            AverageRating = 4.7m,
            TotalReviews = 30,
            FiveStarCount = 25,
            ResponseRate = 96m,
            TotalHelpfulVotes = 60
        };

        _reviewRepositoryMock.Setup(x => x.GetSellerStatsAsync(sellerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Setup existing badge (TopRated already exists)
        var existingBadge = new SellerBadge { BadgeType = BadgeType.TopRated, IsActive = true };
        _sellerBadgeRepositoryMock.Setup(x => x.GetActiveBadgeAsync(sellerId, BadgeType.TopRated, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBadge);

        // No existing badges for other types
        _sellerBadgeRepositoryMock.Setup(x => x.GetActiveBadgeAsync(sellerId, It.IsNotIn(BadgeType.TopRated), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SellerBadge?)null);

        // Act
        var grantedBadges = await _service.CalculateAndGrantBadgesAsync(sellerId);

        // Assert
        grantedBadges.Should().NotContain(BadgeType.TopRated); // Already exists
        grantedBadges.Should().Contain(BadgeType.FiveStarSeller); // Should be granted
        grantedBadges.Should().Contain(BadgeType.ResponsiveSeller); // Should be granted

        // Verify GrantBadgeAsync was called for new badges only
        _sellerBadgeRepositoryMock.Verify(x => x.GrantBadgeAsync(sellerId, BadgeType.TopRated, It.IsAny<CancellationToken>()), Times.Never);
        _sellerBadgeRepositoryMock.Verify(x => x.GrantBadgeAsync(sellerId, BadgeType.FiveStarSeller, It.IsAny<CancellationToken>()), Times.Once);
        _sellerBadgeRepositoryMock.Verify(x => x.GrantBadgeAsync(sellerId, BadgeType.ResponsiveSeller, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsEligibleForBadgeAsync_ShouldReturnFalse_ForUnsupportedBadgeType()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var unsupportedBadgeType = (BadgeType)999; // Invalid enum value

        // Act
        var result = await _service.IsEligibleForBadgeAsync(sellerId, unsupportedBadgeType);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEligibleForBadgeAsync_ShouldReturnTrue_ForNewcomerTrustedEligibleSeller()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var stats = new SellerReviewStats
        {
            AverageRating = 4.6m,
            TotalReviews = 8,
            FirstReviewDate = DateTime.UtcNow.AddDays(-60), // Within 90 days
            ResponseRate = 90m
        };

        _reviewRepositoryMock.Setup(x => x.GetSellerStatsAsync(sellerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _service.IsEligibleForBadgeAsync(sellerId, BadgeType.NewcommerTrusted);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEligibleForBadgeAsync_ShouldReturnTrue_ForExperiencedSellerEligibleSeller()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var stats = new SellerReviewStats
        {
            AverageRating = 4.2m,
            TotalReviews = 35,
            FirstReviewDate = DateTime.UtcNow.AddDays(-400), // More than 365 days
            ResponseRate = 85m
        };

        _reviewRepositoryMock.Setup(x => x.GetSellerStatsAsync(sellerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _service.IsEligibleForBadgeAsync(sellerId, BadgeType.ExperiencedSeller);

        // Assert
        result.Should().BeTrue();
    }
}
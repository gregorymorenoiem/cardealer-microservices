using Xunit;
using FluentAssertions;
using ReviewService.Domain.Entities;

namespace ReviewService.Tests;

/// <summary>
/// Tests para ReviewService - Sprint 14 Sistema de Reviews Básico
/// </summary>
public class ReviewServiceTests
{
    #region Domain Entity Tests

    [Fact]
    public void Review_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        // Act
        var review = new Review
        {
            Id = Guid.NewGuid(),
            BuyerId = buyerId,
            SellerId = sellerId,
            VehicleId = vehicleId,
            OrderId = orderId,
            Rating = 5,
            Title = "Excelente servicio",
            Content = "El vendedor fue muy profesional y el vehículo está en perfectas condiciones.",
            BuyerName = "Juan Pérez",
            IsVerifiedPurchase = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        review.Should().NotBeNull();
        review.BuyerId.Should().Be(buyerId);
        review.SellerId.Should().Be(sellerId);
        review.VehicleId.Should().Be(vehicleId);
        review.OrderId.Should().Be(orderId);
        review.Rating.Should().Be(5);
        review.Title.Should().Be("Excelente servicio");
        review.Content.Should().Contain("profesional");
        review.BuyerName.Should().Be("Juan Pérez");
        review.IsVerifiedPurchase.Should().BeTrue();
        review.IsApproved.Should().BeTrue(); // Default value
        review.HelpfulVotes.Should().Be(0); // Default value
    }

    [Fact]
    public void ReviewSummary_ShouldCalculateMetrics_Correctly()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var summary = new ReviewSummary
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var reviews = new List&lt;Review&gt;
        {
            CreateMockReview(sellerId, 5, true, true),
            CreateMockReview(sellerId, 4, true, false),
            CreateMockReview(sellerId, 5, true, true),
            CreateMockReview(sellerId, 3, true, false),
            CreateMockReview(sellerId, 2, false, false) // Not approved - should be ignored
        };

        // Act
        summary.RecalculateMetrics(reviews);

        // Assert
        summary.TotalReviews.Should().Be(4); // Only approved reviews
        summary.AverageRating.Should().Be(4.25m); // (5+4+5+3)/4
        summary.FiveStarReviews.Should().Be(2);
        summary.FourStarReviews.Should().Be(1);
        summary.ThreeStarReviews.Should().Be(1);
        summary.TwoStarReviews.Should().Be(0);
        summary.OneStarReviews.Should().Be(0);
        summary.PositivePercentage.Should().Be(75m); // (2+1)/4 * 100
        summary.VerifiedPurchaseReviews.Should().Be(2);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Review_Rating_ShouldBeValid(int rating)
    {
        // Arrange & Act
        var review = new Review
        {
            Id = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            Rating = rating,
            Title = "Test Review",
            Content = "Test content for review",
            BuyerName = "Test User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        review.Rating.Should().BeInRange(1, 5);
    }

    [Fact]
    public void ReviewResponse_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();

        // Act
        var response = new ReviewResponse
        {
            Id = Guid.NewGuid(),
            ReviewId = reviewId,
            SellerId = sellerId,
            Content = "Gracias por tu review. Nos alegra saber que tuviste una buena experiencia.",
            SellerName = "AutoDealer Pro",
            IsApproved = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        response.Should().NotBeNull();
        response.ReviewId.Should().Be(reviewId);
        response.SellerId.Should().Be(sellerId);
        response.Content.Should().Contain("Gracias");
        response.SellerName.Should().Be("AutoDealer Pro");
        response.IsApproved.Should().BeTrue();
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public void ReviewSummary_WithNoReviews_ShouldHaveZeroMetrics()
    {
        // Arrange
        var summary = new ReviewSummary
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var emptyReviews = new List&lt;Review&gt;();

        // Act
        summary.RecalculateMetrics(emptyReviews);

        // Assert
        summary.TotalReviews.Should().Be(0);
        summary.AverageRating.Should().Be(0);
        summary.PositivePercentage.Should().Be(0);
        summary.VerifiedPurchaseReviews.Should().Be(0);
        summary.LastReviewDate.Should().BeNull();
    }

    [Fact]
    public void ReviewSummary_GetRatingDistribution_ShouldReturnCorrectDictionary()
    {
        // Arrange
        var summary = new ReviewSummary
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            FiveStarReviews = 10,
            FourStarReviews = 5,
            ThreeStarReviews = 2,
            TwoStarReviews = 1,
            OneStarReviews = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var distribution = summary.GetRatingDistribution();

        // Assert
        distribution.Should().ContainKey(5).WhoseValue.Should().Be(10);
        distribution.Should().ContainKey(4).WhoseValue.Should().Be(5);
        distribution.Should().ContainKey(3).WhoseValue.Should().Be(2);
        distribution.Should().ContainKey(2).WhoseValue.Should().Be(1);
        distribution.Should().ContainKey(1).WhoseValue.Should().Be(0);
    }

    [Fact]
    public void Review_VerifiedPurchase_ShouldBeTrueWhenOrderIdExists()
    {
        // Arrange & Act
        var reviewWithOrder = new Review
        {
            Id = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(), // Has OrderId
            Rating = 5,
            Title = "Great purchase",
            Content = "Everything was perfect",
            BuyerName = "Verified Buyer",
            IsVerifiedPurchase = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var reviewWithoutOrder = new Review
        {
            Id = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            OrderId = null, // No OrderId
            Rating = 4,
            Title = "Good service",
            Content = "Nice experience",
            BuyerName = "Regular User",
            IsVerifiedPurchase = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        reviewWithOrder.OrderId.Should().NotBeNull();
        reviewWithOrder.IsVerifiedPurchase.Should().BeTrue();
        
        reviewWithoutOrder.OrderId.Should().BeNull();
        reviewWithoutOrder.IsVerifiedPurchase.Should().BeFalse();
    }

    [Fact]
    public void ReviewSummary_PositivePercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var summary = new ReviewSummary
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 8 total: 3x5★ + 2x4★ + 1x3★ + 1x2★ + 1x1★ = 62.5% positive (5/8)
        var reviews = new List&lt;Review&gt;
        {
            CreateMockReview(sellerId, 5, true),
            CreateMockReview(sellerId, 5, true),
            CreateMockReview(sellerId, 5, true),
            CreateMockReview(sellerId, 4, true),
            CreateMockReview(sellerId, 4, true),
            CreateMockReview(sellerId, 3, true),
            CreateMockReview(sellerId, 2, true),
            CreateMockReview(sellerId, 1, true)
        };

        // Act
        summary.RecalculateMetrics(reviews);

        // Assert
        summary.TotalReviews.Should().Be(8);
        summary.PositivePercentage.Should().Be(62.5m); // (3+2)/8 * 100
    }

    [Fact]
    public void Review_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var review = new Review();

        // Assert
        review.IsApproved.Should().BeTrue(); // Default
        review.IsVerifiedPurchase.Should().BeFalse(); // Default
        review.HelpfulVotes.Should().Be(0); // Default
        review.TotalVotes.Should().Be(0); // Default
        review.Title.Should().Be(string.Empty); // Default
        review.Content.Should().Be(string.Empty); // Default
        review.BuyerName.Should().Be(string.Empty); // Default
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Crear review mock para tests
    /// </summary>
    private Review CreateMockReview(Guid sellerId, int rating, bool isApproved, bool isVerified = false)
    {
        return new Review
        {
            Id = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            SellerId = sellerId,
            VehicleId = Guid.NewGuid(),
            OrderId = isVerified ? Guid.NewGuid() : null,
            Rating = rating,
            Title = $"Review {rating} stars",
            Content = $"This is a {rating} star review content.",
            BuyerName = "Test Buyer",
            IsApproved = isApproved,
            IsVerifiedPurchase = isVerified,
            CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)),
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
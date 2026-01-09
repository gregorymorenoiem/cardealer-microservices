using FluentAssertions;
using ReviewService.Domain.Entities;
using Xunit;

namespace ReviewService.Tests;

/// <summary>
/// Tests para las funcionalidades del Sprint 15 - Reviews Avanzado
/// Incluye: Votos de utilidad, Badges de vendedor, Solicitudes automáticas, Anti-fraude
/// </summary>
public class Sprint15AdvancedReviewsTests
{
    #region ReviewHelpfulVote Tests

    [Fact]
    public void ReviewHelpfulVote_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var vote = new ReviewHelpfulVote
        {
            Id = Guid.NewGuid(),
            ReviewId = reviewId,
            UserId = userId,
            IsHelpful = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        vote.Should().NotBeNull();
        vote.ReviewId.Should().Be(reviewId);
        vote.UserId.Should().Be(userId);
        vote.IsHelpful.Should().BeTrue();
    }

    [Fact]
    public void ReviewHelpfulVote_WhenNotHelpful_ShouldHaveIsHelpfulFalse()
    {
        // Arrange & Act
        var vote = new ReviewHelpfulVote
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsHelpful = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        vote.IsHelpful.Should().BeFalse();
    }

    [Fact]
    public void ReviewHelpfulVote_ShouldTrackUserAgent()
    {
        // Arrange & Act
        var vote = new ReviewHelpfulVote
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsHelpful = true,
            UserAgent = "Mozilla/5.0 Chrome/120.0",
            UserIpAddress = "192.168.1.1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        vote.UserAgent.Should().Be("Mozilla/5.0 Chrome/120.0");
        vote.UserIpAddress.Should().Be("192.168.1.1");
    }

    #endregion

    #region SellerBadge Tests

    [Fact]
    public void SellerBadge_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var sellerId = Guid.NewGuid();

        // Act
        var badge = new SellerBadge
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            BadgeType = BadgeType.TopRated,
            Title = "Top Rated",
            Description = "Vendedor con calificación 4.8+",
            Icon = "⭐",
            Color = "#FFD700",
            IsActive = true,
            EarnedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        badge.Should().NotBeNull();
        badge.SellerId.Should().Be(sellerId);
        badge.BadgeType.Should().Be(BadgeType.TopRated);
        badge.Title.Should().Be("Top Rated");
        badge.Icon.Should().Be("⭐");
        badge.Color.Should().Be("#FFD700");
        badge.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(BadgeType.TopRated, "Top Rated")]
    [InlineData(BadgeType.TrustedDealer, "Trusted Dealer")]
    [InlineData(BadgeType.FiveStarSeller, "Five Star Seller")]
    [InlineData(BadgeType.VerifiedProfessional, "Verified Professional")]
    [InlineData(BadgeType.CustomerChoice, "Customer Choice")]
    [InlineData(BadgeType.RisingStar, "Rising Star")]
    public void BadgeType_ShouldHaveExpectedValues(BadgeType badgeType, string expectedName)
    {
        // Arrange & Act
        var badge = new SellerBadge
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BadgeType = badgeType,
            Title = expectedName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        badge.BadgeType.Should().Be(badgeType);
        badge.Title.Should().Be(expectedName);
    }

    [Fact]
    public void SellerBadge_WhenRevoked_ShouldHaveRevokedAt()
    {
        // Arrange
        var badge = new SellerBadge
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BadgeType = BadgeType.RisingStar,
            Title = "Estrella en Ascenso",
            IsActive = true,
            EarnedAt = DateTime.UtcNow.AddDays(-30),
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-30)
        };

        // Act - Revocar badge
        badge.IsActive = false;
        badge.RevokedAt = DateTime.UtcNow;
        badge.UpdatedAt = DateTime.UtcNow;

        // Assert
        badge.IsActive.Should().BeFalse();
        badge.RevokedAt.Should().NotBeNull();
        badge.RevokedAt.Should().BeAfter(badge.EarnedAt);
    }

    [Fact]
    public void SellerBadge_WithExpiry_ShouldTrackExpiration()
    {
        // Arrange
        var badge = new SellerBadge
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BadgeType = BadgeType.RisingStar,
            Title = "Estrella en Ascenso",
            IsActive = true,
            EarnedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(90),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        badge.ExpiresAt.Should().NotBeNull();
        badge.ExpiresAt.Should().BeAfter(badge.EarnedAt);
        
        // El badge aún está vigente
        var isValid = badge.IsActive && (badge.ExpiresAt == null || badge.ExpiresAt > DateTime.UtcNow);
        isValid.Should().BeTrue();
    }

    #endregion

    #region ReviewRequest Tests

    [Fact]
    public void ReviewRequest_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        // Act
        var request = new ReviewRequest
        {
            Id = Guid.NewGuid(),
            BuyerId = buyerId,
            SellerId = sellerId,
            VehicleId = vehicleId,
            OrderId = orderId,
            BuyerEmail = "buyer@example.com",
            BuyerName = "Juan Pérez",
            PurchaseDate = DateTime.UtcNow.AddDays(-7),
            RequestSentAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Status = ReviewRequestStatus.Sent,
            Token = "abc123xyz",
            RemindersSent = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        request.Should().NotBeNull();
        request.BuyerId.Should().Be(buyerId);
        request.SellerId.Should().Be(sellerId);
        request.VehicleId.Should().Be(vehicleId);
        request.OrderId.Should().Be(orderId);
        request.BuyerEmail.Should().Be("buyer@example.com");
        request.Status.Should().Be(ReviewRequestStatus.Sent);
        request.Token.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(ReviewRequestStatus.Sent)]
    [InlineData(ReviewRequestStatus.Viewed)]
    [InlineData(ReviewRequestStatus.Completed)]
    [InlineData(ReviewRequestStatus.Expired)]
    [InlineData(ReviewRequestStatus.Cancelled)]
    public void ReviewRequestStatus_ShouldHaveExpectedValues(ReviewRequestStatus status)
    {
        // Arrange & Act
        var request = new ReviewRequest
        {
            Id = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Status = status,
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        request.Status.Should().Be(status);
    }

    [Fact]
    public void ReviewRequest_WhenCompleted_ShouldHaveReviewId()
    {
        // Arrange
        var request = new ReviewRequest
        {
            Id = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Status = ReviewRequestStatus.Sent,
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act - Completar solicitud
        var reviewId = Guid.NewGuid();
        request.Status = ReviewRequestStatus.Completed;
        request.ReviewId = reviewId;
        request.ReviewCreatedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        // Assert
        request.Status.Should().Be(ReviewRequestStatus.Completed);
        request.ReviewId.Should().Be(reviewId);
        request.ReviewCreatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ReviewRequest_ShouldTrackReminders()
    {
        // Arrange
        var request = new ReviewRequest
        {
            Id = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Status = ReviewRequestStatus.Sent,
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            RemindersSent = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act - Enviar recordatorios
        request.RemindersSent = 1;
        request.LastReminderAt = DateTime.UtcNow;

        // Assert
        request.RemindersSent.Should().Be(1);
        request.LastReminderAt.Should().NotBeNull();
    }

    #endregion

    #region FraudDetectionLog Tests

    [Fact]
    public void FraudDetectionLog_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var reviewId = Guid.NewGuid();

        // Act
        var log = new FraudDetectionLog
        {
            Id = Guid.NewGuid(),
            ReviewId = reviewId,
            CheckType = FraudCheckType.DuplicateIp,
            Result = FraudCheckResult.Pass,
            ConfidenceScore = 95,
            Details = "IP address not found in blacklist",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        log.Should().NotBeNull();
        log.ReviewId.Should().Be(reviewId);
        log.CheckType.Should().Be(FraudCheckType.DuplicateIp);
        log.Result.Should().Be(FraudCheckResult.Pass);
        log.ConfidenceScore.Should().Be(95);
    }

    [Theory]
    [InlineData(FraudCheckType.DuplicateIp)]
    [InlineData(FraudCheckType.DuplicateDevice)]
    [InlineData(FraudCheckType.ContentAnalysis)]
    [InlineData(FraudCheckType.SpeedCheck)]
    [InlineData(FraudCheckType.NewUserCheck)]
    [InlineData(FraudCheckType.RatingPattern)]
    [InlineData(FraudCheckType.TextSimilarity)]
    [InlineData(FraudCheckType.RelationshipCheck)]
    [InlineData(FraudCheckType.GeolocationCheck)]
    [InlineData(FraudCheckType.PurchaseVerification)]
    public void FraudCheckType_ShouldHaveExpectedValues(FraudCheckType checkType)
    {
        // Arrange & Act
        var log = new FraudDetectionLog
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            CheckType = checkType,
            Result = FraudCheckResult.Pass,
            ConfidenceScore = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        log.CheckType.Should().Be(checkType);
    }

    [Theory]
    [InlineData(FraudCheckResult.Pass)]
    [InlineData(FraudCheckResult.Warning)]
    [InlineData(FraudCheckResult.Fail)]
    [InlineData(FraudCheckResult.Suspicious)]
    [InlineData(FraudCheckResult.Error)]
    public void FraudCheckResult_ShouldHaveExpectedValues(FraudCheckResult result)
    {
        // Arrange & Act
        var log = new FraudDetectionLog
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            CheckType = FraudCheckType.ContentAnalysis,
            Result = result,
            ConfidenceScore = 80,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        log.Result.Should().Be(result);
    }

    [Fact]
    public void FraudDetectionLog_ShouldStoreMetadata()
    {
        // Arrange & Act
        var log = new FraudDetectionLog
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            CheckType = FraudCheckType.ContentAnalysis,
            Result = FraudCheckResult.Warning,
            ConfidenceScore = 60,
            Details = "Possible spam patterns detected",
            Metadata = "{\"spamWords\": [\"compre ya\", \"oferta\"], \"frequency\": 3}",
            AlgorithmVersion = "2.0",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        log.Metadata.Should().NotBeNullOrEmpty();
        log.Metadata.Should().Contain("spamWords");
        log.AlgorithmVersion.Should().Be("2.0");
    }

    [Fact]
    public void FraudDetectionLog_LowConfidenceScore_ShouldIndicateSuspicious()
    {
        // Arrange
        var log = new FraudDetectionLog
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            CheckType = FraudCheckType.SpeedCheck,
            Result = FraudCheckResult.Suspicious,
            ConfidenceScore = 30,
            Details = "Multiple reviews from same IP in short time",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        log.ConfidenceScore.Should().BeLessThan(50);
        log.Result.Should().Be(FraudCheckResult.Suspicious);
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public void ReviewWithVotes_ShouldCalculateHelpfulPercentage()
    {
        // Arrange - Simular una review con votos
        var reviewId = Guid.NewGuid();
        var votes = new List<ReviewHelpfulVote>();

        // Agregar votos positivos y negativos
        for (int i = 0; i < 8; i++)
        {
            votes.Add(new ReviewHelpfulVote
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                UserId = Guid.NewGuid(),
                IsHelpful = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        for (int i = 0; i < 2; i++)
        {
            votes.Add(new ReviewHelpfulVote
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                UserId = Guid.NewGuid(),
                IsHelpful = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Act
        var totalVotes = votes.Count;
        var helpfulVotes = votes.Count(v => v.IsHelpful);
        var helpfulPercentage = (decimal)helpfulVotes / totalVotes * 100;

        // Assert
        totalVotes.Should().Be(10);
        helpfulVotes.Should().Be(8);
        helpfulPercentage.Should().Be(80);
    }

    [Fact]
    public void SellerWithMultipleBadges_ShouldHaveCorrectBadgeCount()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var badges = new List<SellerBadge>
        {
            new SellerBadge
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                BadgeType = BadgeType.TopRated,
                Title = "Top Rated",
                IsActive = true,
                EarnedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new SellerBadge
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                BadgeType = BadgeType.TrustedDealer,
                Title = "Trusted Dealer",
                IsActive = true,
                EarnedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new SellerBadge
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                BadgeType = BadgeType.RisingStar,
                Title = "Rising Star",
                IsActive = false,  // Revoked
                RevokedAt = DateTime.UtcNow,
                EarnedAt = DateTime.UtcNow.AddDays(-30),
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        // Act
        var activeBadges = badges.Where(b => b.IsActive).ToList();
        var revokedBadges = badges.Where(b => !b.IsActive).ToList();

        // Assert
        activeBadges.Should().HaveCount(2);
        revokedBadges.Should().HaveCount(1);
        activeBadges.Should().Contain(b => b.BadgeType == BadgeType.TopRated);
        activeBadges.Should().Contain(b => b.BadgeType == BadgeType.TrustedDealer);
        revokedBadges.Should().Contain(b => b.BadgeType == BadgeType.RisingStar);
    }

    [Fact]
    public void FraudDetection_MultipleChecks_ShouldCalculateOverallRisk()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var checks = new List<FraudDetectionLog>
        {
            new FraudDetectionLog
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                CheckType = FraudCheckType.DuplicateIp,
                Result = FraudCheckResult.Pass,
                ConfidenceScore = 100,
                CreatedAt = DateTime.UtcNow
            },
            new FraudDetectionLog
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                CheckType = FraudCheckType.ContentAnalysis,
                Result = FraudCheckResult.Warning,
                ConfidenceScore = 70,
                CreatedAt = DateTime.UtcNow
            },
            new FraudDetectionLog
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                CheckType = FraudCheckType.SpeedCheck,
                Result = FraudCheckResult.Pass,
                ConfidenceScore = 90,
                CreatedAt = DateTime.UtcNow
            }
        };

        // Act
        var averageConfidence = (int)checks.Average(c => c.ConfidenceScore);
        var hasWarnings = checks.Any(c => c.Result == FraudCheckResult.Warning);
        var hasFails = checks.Any(c => c.Result == FraudCheckResult.Fail);

        // Assert
        averageConfidence.Should().BeGreaterThan(80);
        hasWarnings.Should().BeTrue();
        hasFails.Should().BeFalse();
    }

    #endregion
}

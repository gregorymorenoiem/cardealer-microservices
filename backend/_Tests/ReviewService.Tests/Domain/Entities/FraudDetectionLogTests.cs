using Xunit;

using FluentAssertions;
using ReviewService.Domain.Entities;

namespace ReviewService.Tests.Domain.Entities;

/// <summary>
/// Tests para FraudDetectionLog entity
/// </summary>
public class FraudDetectionLogTests
{
    [Fact]
    public void FraudDetectionLog_ShouldBeCreatedSuccessfully_WithValidData()
    {
        // Arrange & Act
        var log = new FraudDetectionLog
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            CheckType = FraudCheckType.DuplicateContent,
            Result = FraudCheckResult.Passed,
            ConfidenceScore = 85,
            Details = "Content analysis passed similarity threshold",
            CheckedAt = DateTime.UtcNow,
            Metadata = "{\"similarityScore\": 0.15, \"threshold\": 0.8}"
        };

        // Assert
        log.Id.Should().NotBe(Guid.Empty);
        log.ReviewId.Should().NotBe(Guid.Empty);
        log.CheckType.Should().Be(FraudCheckType.DuplicateContent);
        log.Result.Should().Be(FraudCheckResult.Passed);
        log.ConfidenceScore.Should().Be(85);
        log.Details.Should().Be("Content analysis passed similarity threshold");
        log.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        log.Metadata.Should().Contain("similarityScore");
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
    public void FraudDetectionLog_ShouldSupportAllCheckTypes(FraudCheckType checkType)
    {
        // Arrange & Act
        var log = new FraudDetectionLog
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            CheckType = checkType,
            Result = FraudCheckResult.Passed,
            ConfidenceScore = 90,
            Details = $"Check for {checkType} completed",
            CheckedAt = DateTime.UtcNow
        };

        // Assert
        log.CheckType.Should().Be(checkType);
    }

    [Theory]
    [InlineData(FraudCheckResult.Pass)]
    [InlineData(FraudCheckResult.Warning)]
    [InlineData(FraudCheckResult.Fail)]
    [InlineData(FraudCheckResult.Suspicious)]
    public void FraudDetectionLog_ShouldSupportAllResultTypes(FraudCheckResult result)
    {
        // Arrange & Act
        var log = new FraudDetectionLog
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            CheckType = FraudCheckType.ContentAnalysis,
            Result = result,
            ConfidenceScore = 75,
            Details = $"Check result: {result}",
            CheckedAt = DateTime.UtcNow
        };

        // Assert
        log.Result.Should().Be(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void FraudDetectionLog_ShouldAcceptValidConfidenceScores(int confidenceScore)
    {
        // Arrange & Act
        var log = new FraudDetectionLog
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            CheckType = FraudCheckType.SpeedCheck,
            Result = FraudCheckResult.Passed,
            ConfidenceScore = confidenceScore,
            Details = "Speed check completed",
            CheckedAt = DateTime.UtcNow
        };

        // Assert
        log.ConfidenceScore.Should().Be(confidenceScore);
    }

    [Fact]
    public void FraudDetectionLog_ShouldAllowNullMetadata()
    {
        // Arrange & Act
        var log = new FraudDetectionLog
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            CheckType = FraudCheckType.UserBehavior,
            Result = FraudCheckResult.Passed,
            ConfidenceScore = 95,
            Details = "Basic user behavior check",
            CheckedAt = DateTime.UtcNow,
            Metadata = null
        };

        // Assert
        log.Metadata.Should().BeNull();
    }

    [Fact]
    public void FraudDetectionLog_ShouldStoreComplexMetadata()
    {
        // Arrange & Act
        var metadata = "{\"ipAddress\": \"192.168.1.1\", \"userAgent\": \"Mozilla/5.0\", \"sessionId\": \"abc123\", \"previousChecks\": [\"ContentAnalysis\", \"SpeedCheck\"]}";
        var log = new FraudDetectionLog
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            CheckType = FraudCheckType.DeviceFingerprint,
            Result = FraudCheckResult.Suspicious,
            ConfidenceScore = 65,
            Details = "Device fingerprint analysis indicates potential issues",
            CheckedAt = DateTime.UtcNow,
            Metadata = metadata
        };

        // Assert
        log.Metadata.Should().Be(metadata);
        log.Metadata.Should().Contain("ipAddress");
        log.Metadata.Should().Contain("userAgent");
        log.Metadata.Should().Contain("previousChecks");
    }
}
using FeatureToggleService.Application.Interfaces;
using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using FeatureToggleService.Domain.Interfaces;
using FeatureToggleService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Environment = FeatureToggleService.Domain.Enums.Environment;

namespace FeatureToggleService.Tests.Services;

public class FeatureFlagEvaluatorTests
{
    private readonly Mock<IFeatureFlagRepository> _repositoryMock;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<FeatureFlagEvaluator>> _loggerMock;
    private readonly IFeatureFlagEvaluator _evaluator;

    public FeatureFlagEvaluatorTests()
    {
        _repositoryMock = new Mock<IFeatureFlagRepository>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<FeatureFlagEvaluator>>();
        _evaluator = new FeatureFlagEvaluator(_repositoryMock.Object, _cache, _loggerMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_WithEnabledFlag_ShouldReturnTrue()
    {
        // Arrange
        var flag = new FeatureFlag 
        { 
            Key = "test-feature", 
            Name = "Test",
            Status = FlagStatus.Active,
            IsEnabled = true,
            RolloutPercentage = 100
        };
        _repositoryMock.Setup(r => r.GetByKeyAsync("test-feature", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);

        var context = new EvaluationContext { UserId = "user1" };

        // Act
        var result = await _evaluator.EvaluateAsync("test-feature", context);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WithDisabledFlag_ShouldReturnFalse()
    {
        // Arrange
        var flag = new FeatureFlag 
        { 
            Key = "test-feature", 
            Name = "Test",
            Status = FlagStatus.Inactive,
            IsEnabled = false
        };
        _repositoryMock.Setup(r => r.GetByKeyAsync("test-feature", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);

        var context = new EvaluationContext { UserId = "user1" };

        // Act
        var result = await _evaluator.EvaluateAsync("test-feature", context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_WithNonExistentFlag_ShouldReturnFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByKeyAsync("non-existent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeatureFlag?)null);

        var context = new EvaluationContext { UserId = "user1" };

        // Act
        var result = await _evaluator.EvaluateAsync("non-existent", context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_WithExpiredFlag_ShouldReturnFalse()
    {
        // Arrange
        var flag = new FeatureFlag 
        { 
            Key = "test-feature", 
            Name = "Test",
            Status = FlagStatus.Active,
            IsEnabled = true,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };
        _repositoryMock.Setup(r => r.GetByKeyAsync("test-feature", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);

        var context = new EvaluationContext { UserId = "user1" };

        // Act
        var result = await _evaluator.EvaluateAsync("test-feature", context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_WithKillSwitchTriggered_ShouldReturnFalse()
    {
        // Arrange
        var flag = new FeatureFlag 
        { 
            Key = "test-feature", 
            Name = "Test",
            Status = FlagStatus.Active,
            IsEnabled = true,
            KillSwitchTriggered = true
        };
        _repositoryMock.Setup(r => r.GetByKeyAsync("test-feature", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);

        var context = new EvaluationContext { UserId = "user1" };

        // Act
        var result = await _evaluator.EvaluateAsync("test-feature", context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_WithTargetedUser_ShouldReturnTrue()
    {
        // Arrange
        var flag = new FeatureFlag 
        { 
            Key = "test-feature", 
            Name = "Test",
            Status = FlagStatus.Active,
            IsEnabled = true,
            TargetUserIds = new List<string> { "user1", "user2" }
        };
        _repositoryMock.Setup(r => r.GetByKeyAsync("test-feature", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);

        var context = new EvaluationContext { UserId = "user1" };

        // Act
        var result = await _evaluator.EvaluateAsync("test-feature", context);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WithEnvironmentMismatch_ShouldReturnFalse()
    {
        // Arrange
        var flag = new FeatureFlag 
        { 
            Key = "test-feature", 
            Name = "Test",
            Status = FlagStatus.Active,
            IsEnabled = true,
            Environment = Environment.Production
        };
        _repositoryMock.Setup(r => r.GetByKeyAsync("test-feature", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);

        var context = new EvaluationContext { UserId = "user1", Environment = "Development" };

        // Act
        var result = await _evaluator.EvaluateAsync("test-feature", context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateMultipleAsync_ShouldReturnDictionaryOfResults()
    {
        // Arrange
        var flag1 = new FeatureFlag 
        { 
            Key = "feature-1", 
            Name = "Feature 1",
            Status = FlagStatus.Active,
            IsEnabled = true,
            RolloutPercentage = 100
        };
        var flag2 = new FeatureFlag 
        { 
            Key = "feature-2", 
            Name = "Feature 2",
            Status = FlagStatus.Inactive,
            IsEnabled = false
        };

        _repositoryMock.Setup(r => r.GetByKeyAsync("feature-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag1);
        _repositoryMock.Setup(r => r.GetByKeyAsync("feature-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag2);

        var context = new EvaluationContext { UserId = "user1" };
        var flagKeys = new List<string> { "feature-1", "feature-2" };

        // Act
        var results = await _evaluator.EvaluateMultipleAsync(flagKeys, context);

        // Assert
        results.Should().HaveCount(2);
        results["feature-1"].Should().BeTrue();
        results["feature-2"].Should().BeFalse();
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(0, false)]
    public async Task EvaluateAsync_WithRolloutPercentage_ShouldRespectPercentage(int percentage, bool expectedResult)
    {
        // Arrange
        var flag = new FeatureFlag 
        { 
            Key = "test-feature", 
            Name = "Test",
            Status = FlagStatus.Active,
            IsEnabled = true,
            RolloutPercentage = percentage
        };
        _repositoryMock.Setup(r => r.GetByKeyAsync("test-feature", It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);

        var context = new EvaluationContext { UserId = "user1" };

        // Act
        var result = await _evaluator.EvaluateAsync("test-feature", context);

        // Assert
        result.Should().Be(expectedResult);
    }
}

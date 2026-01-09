using FluentAssertions;
using LeadScoringService.Domain.Entities;
using LeadScoringService.Infrastructure.Services;
using Moq;
using LeadScoringService.Domain.Interfaces;
using Xunit;

namespace LeadScoringService.Tests;

public class LeadScoringServiceTests
{
    private readonly Mock<ILeadRepository> _mockLeadRepository;
    private readonly LeadScoringEngine _scoringEngine;

    public LeadScoringServiceTests()
    {
        _mockLeadRepository = new Mock<ILeadRepository>();
        _scoringEngine = new LeadScoringEngine(_mockLeadRepository.Object);
    }

    [Fact]
    public void Lead_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            UserFullName = "Test User",
            VehicleId = Guid.NewGuid(),
            VehicleTitle = "Toyota Camry 2023",
            VehiclePrice = 25000,
            DealerId = Guid.NewGuid(),
            DealerName = "AutoDealer RD",
            Status = LeadStatus.New,
            Temperature = LeadTemperature.Cold,
            FirstInteractionAt = DateTime.UtcNow,
            LastInteractionAt = DateTime.UtcNow
        };

        // Assert
        lead.Should().NotBeNull();
        lead.Id.Should().NotBeEmpty();
        lead.UserEmail.Should().Be("test@example.com");
        lead.Status.Should().Be(LeadStatus.New);
        lead.Temperature.Should().Be(LeadTemperature.Cold);
    }

    [Fact]
    public void EngagementScore_ShouldBe10_WithMultipleViews()
    {
        // Arrange
        var lead = CreateTestLead();
        lead.ViewCount = 12; // 12 views = 10 points

        // Act
        var score = _scoringEngine.CalculateEngagementScore(lead);

        // Assert
        score.Should().Be(10);
    }

    [Fact]
    public void EngagementScore_ShouldIncrease_WithFavorites()
    {
        // Arrange
        var lead = CreateTestLead();
        lead.ViewCount = 5; // 6 points
        lead.FavoriteCount = 1; // +10 points

        // Act
        var score = _scoringEngine.CalculateEngagementScore(lead);

        // Assert
        score.Should().Be(16); // 6 + 10
    }

    [Fact]
    public void EngagementScore_ShouldNotExceed40()
    {
        // Arrange
        var lead = CreateTestLead();
        lead.ViewCount = 20; // 10 points
        lead.FavoriteCount = 5; // 10 points
        lead.ComparisonCount = 5; // 8 points
        lead.ShareCount = 5; // 6 points
        lead.TotalTimeSpentSeconds = 600; // 10 minutes = 6 points
        // Total would be 40 points

        // Act
        var score = _scoringEngine.CalculateEngagementScore(lead);

        // Assert
        score.Should().BeLessOrEqualTo(40);
        score.Should().Be(40);
    }

    [Fact]
    public void RecencyScore_ShouldBe30_WithRecentInteraction()
    {
        // Arrange
        var lead = CreateTestLead();
        lead.LastInteractionAt = DateTime.UtcNow.AddMinutes(-30); // 30 minutes ago

        // Act
        var score = _scoringEngine.CalculateRecencyScore(lead);

        // Assert
        score.Should().Be(30); // Less than 1 hour = 30 points
    }

    [Fact]
    public void RecencyScore_ShouldDecrease_WithOldInteraction()
    {
        // Arrange
        var lead = CreateTestLead();
        lead.LastInteractionAt = DateTime.UtcNow.AddDays(-10); // 10 days ago

        // Act
        var score = _scoringEngine.CalculateRecencyScore(lead);

        // Assert
        score.Should().Be(5); // 7-14 days = 5 points
    }

    [Fact]
    public void IntentScore_ShouldBe15_WithTestDrive()
    {
        // Arrange
        var lead = CreateTestLead();
        lead.HasScheduledTestDrive = true;

        // Act
        var score = _scoringEngine.CalculateIntentScore(lead);

        // Assert
        score.Should().Be(15);
    }

    [Fact]
    public void IntentScore_ShouldBe27_WithTestDriveAndFinancing()
    {
        // Arrange
        var lead = CreateTestLead();
        lead.HasScheduledTestDrive = true; // 15 points
        lead.HasRequestedFinancing = true; // 12 points

        // Act
        var score = _scoringEngine.CalculateIntentScore(lead);

        // Assert
        score.Should().Be(27);
    }

    [Fact]
    public void IntentScore_ShouldNotExceed30()
    {
        // Arrange
        var lead = CreateTestLead();
        lead.HasScheduledTestDrive = true; // 15 points
        lead.HasRequestedFinancing = true; // 12 points
        lead.ContactCount = 10; // 10 points
        // Total would be 37, but max is 30

        // Act
        var score = _scoringEngine.CalculateIntentScore(lead);

        // Assert
        score.Should().BeLessOrEqualTo(30);
    }

    [Fact]
    public void Temperature_ShouldBeHot_WithHighScore()
    {
        // Arrange
        var score = 85;

        // Act
        var temperature = _scoringEngine.DetermineTemperature(score);

        // Assert
        temperature.Should().Be(LeadTemperature.Hot);
    }

    [Fact]
    public void Temperature_ShouldBeWarm_WithMediumScore()
    {
        // Arrange
        var score = 55;

        // Act
        var temperature = _scoringEngine.DetermineTemperature(score);

        // Assert
        temperature.Should().Be(LeadTemperature.Warm);
    }

    [Fact]
    public void Temperature_ShouldBeCold_WithLowScore()
    {
        // Arrange
        var score = 25;

        // Act
        var temperature = _scoringEngine.DetermineTemperature(score);

        // Assert
        temperature.Should().Be(LeadTemperature.Cold);
    }

    [Fact]
    public void ConversionProbability_ShouldIncrease_WithHighIntent()
    {
        // Arrange
        var lead = CreateTestLead();
        lead.Score = 80;
        lead.EngagementScore = 35;
        lead.RecencyScore = 25;
        lead.IntentScore = 20;
        lead.HasScheduledTestDrive = true;
        lead.HasRequestedFinancing = true;
        lead.ContactCount = 5;

        // Act
        var probability = _scoringEngine.CalculateConversionProbability(lead);

        // Assert
        probability.Should().BeGreaterThan(50);
        probability.Should().BeLessOrEqualTo(100);
    }

    [Fact]
    public async Task CalculateLeadScore_ShouldReturnValidScore()
    {
        // Arrange
        var lead = CreateTestLead();
        lead.ViewCount = 10;
        lead.FavoriteCount = 1;
        lead.ContactCount = 2;
        lead.LastInteractionAt = DateTime.UtcNow.AddHours(-2);

        // Act
        var totalScore = await _scoringEngine.CalculateLeadScoreAsync(lead);

        // Assert
        totalScore.Should().BeGreaterOrEqualTo(0);
        totalScore.Should().BeLessOrEqualTo(100);
        lead.EngagementScore.Should().BeGreaterThan(0);
        lead.RecencyScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LeadAction_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var action = new LeadAction
        {
            Id = Guid.NewGuid(),
            LeadId = Guid.NewGuid(),
            ActionType = LeadActionType.ViewListing,
            Description = "Viewed listing",
            ScoreImpact = 2,
            OccurredAt = DateTime.UtcNow
        };

        // Assert
        action.Should().NotBeNull();
        action.ActionType.Should().Be(LeadActionType.ViewListing);
        action.ScoreImpact.Should().Be(2);
    }

    [Fact]
    public void LeadStatus_ShouldTransition_FromNewToContacted()
    {
        // Arrange
        var lead = CreateTestLead();
        lead.Status = LeadStatus.New;

        // Act
        lead.Status = LeadStatus.Contacted;
        lead.LastContactedAt = DateTime.UtcNow;

        // Assert
        lead.Status.Should().Be(LeadStatus.Contacted);
        lead.LastContactedAt.Should().NotBeNull();
    }

    // Helper method
    private Lead CreateTestLead()
    {
        return new Lead
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            UserFullName = "Test User",
            VehicleId = Guid.NewGuid(),
            VehicleTitle = "Toyota Camry 2023",
            VehiclePrice = 25000,
            DealerId = Guid.NewGuid(),
            DealerName = "AutoDealer RD",
            Score = 0,
            Temperature = LeadTemperature.Cold,
            Status = LeadStatus.New,
            Source = LeadSource.OrganicSearch,
            FirstInteractionAt = DateTime.UtcNow,
            LastInteractionAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ChatbotService.Domain.Entities;
using ChatbotService.Infrastructure.Services;

namespace ChatbotService.Tests;

public class ConversationTests
{
    [Fact]
    public void Conversation_ShouldBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var conversation = new Conversation
        {
            UserId = Guid.NewGuid(),
            UserName = "Test User",
            VehicleId = Guid.NewGuid(),
            DealerId = Guid.NewGuid()
        };

        // Assert
        conversation.Status.Should().Be(ConversationStatus.Active);
        conversation.LeadScore.Should().Be(0);
        conversation.LeadTemperature.Should().Be(LeadTemperature.Unknown);
    }
}

public class LeadScoringEngineTests
{
    private readonly LeadScoringEngine _engine;

    public LeadScoringEngineTests()
    {
        var mockLogger = new Mock<ILogger<LeadScoringEngine>>();
        _engine = new LeadScoringEngine(mockLogger.Object);
    }

    [Fact]
    public async Task CalculateLeadScore_ShouldReturnHotLead_ForHighEngagement()
    {
        // Arrange
        var conversation = new Conversation
        {
            UserId = Guid.NewGuid(),
            UserName = "Test User",
            VehicleId = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Messages = new List<Message>()
        };

        // Add messages with strong buying signals
        conversation.Messages.Add(new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = "I need to buy this car TODAY, my budget is ready"
        });

        conversation.Messages.Add(new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = "I want to schedule a test drive ASAP and my trade-in is ready"
        });

        // Act
        var score = await _engine.CalculateLeadScoreAsync(conversation);

        // Assert
        score.Should().BeGreaterThanOrEqualTo(85); // HOT lead
    }

    [Fact]
    public async Task CalculateLeadScore_ShouldReturnWarmLead_ForModerateEngagement()
    {
        // Arrange
        var conversation = new Conversation
        {
            UserId = Guid.NewGuid(),
            UserName = "Test User",
            VehicleId = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Messages = new List<Message>()
        };

        conversation.Messages.Add(new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = "I'm interested in this vehicle, what are the specs?"
        });

        // Act
        var score = await _engine.CalculateLeadScoreAsync(conversation);

        // Assert
        score.Should().BeInRange(50, 84); // WARM lead
    }

    [Fact]
    public async Task CalculateLeadScore_ShouldReturnColdLead_ForLowEngagement()
    {
        // Arrange
        var conversation = new Conversation
        {
            UserId = Guid.NewGuid(),
            UserName = "Test User",
            VehicleId = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Messages = new List<Message>()
        };

        conversation.Messages.Add(new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = "Just browsing"
        });

        // Act
        var score = await _engine.CalculateLeadScoreAsync(conversation);

        // Assert
        score.Should().BeLessThan(50); // COLD lead
    }

    [Fact]
    public void DetermineLeadTemperature_Hot_WhenScoreAbove85()
    {
        // Arrange
        int score = 90;

        // Act
        var temperature = _engine.DetermineLeadTemperature(score);

        // Assert
        temperature.Should().Be(LeadTemperature.Hot);
    }

    [Fact]
    public void DetermineLeadTemperature_Warm_WhenScoreBetween50And69()
    {
        // Arrange
        int score = 60;

        // Act
        var temperature = _engine.DetermineLeadTemperature(score);

        // Assert
        temperature.Should().Be(LeadTemperature.Warm);
    }

    [Fact]
    public void DetermineLeadTemperature_Cold_WhenScoreBelow50()
    {
        // Arrange
        int score = 30;

        // Act
        var temperature = _engine.DetermineLeadTemperature(score);

        // Assert
        temperature.Should().Be(LeadTemperature.Cold);
    }

    [Fact]
    public void ShouldTriggerHandoff_True_ForHotLead()
    {
        // Arrange
        var conversation = new Conversation
        {
            LeadScore = 90,
            LeadTemperature = LeadTemperature.Hot
        };

        // Act
        var shouldHandoff = _engine.ShouldTriggerHandoff(conversation);

        // Assert
        shouldHandoff.Should().BeTrue();
    }

    [Fact]
    public void ShouldTriggerHandoff_False_ForColdLead()
    {
        // Arrange
        var conversation = new Conversation
        {
            LeadScore = 30,
            LeadTemperature = LeadTemperature.Cold
        };

        // Act
        var shouldHandoff = _engine.ShouldTriggerHandoff(conversation);

        // Assert
        shouldHandoff.Should().BeFalse();
    }
}

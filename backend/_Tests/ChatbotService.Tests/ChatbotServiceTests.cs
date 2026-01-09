using Xunit;
using FluentAssertions;
using ChatbotService.Domain.Entities;

namespace ChatbotService.Tests;

public class ChatConversationTests
{
    [Fact]
    public void ChatConversation_ShouldBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var conversation = new ChatConversation
        {
            Id = Guid.NewGuid(),
            SessionId = "test-session-123"
        };

        // Assert
        conversation.Id.Should().NotBeEmpty();
        conversation.Status.Should().Be(ConversationStatus.Active);
        conversation.MessageCount.Should().Be(0);
        conversation.TotalTokensUsed.Should().Be(0);
        conversation.EstimatedCost.Should().Be(0);
        conversation.LeadQualification.Should().Be(LeadQualification.Unknown);
        conversation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ChatConversation_AddMessage_ShouldIncrementMessageCount()
    {
        // Arrange
        var conversation = new ChatConversation { Id = Guid.NewGuid() };
        var message = ChatMessage.CreateUserMessage(conversation.Id, "Hello");

        // Act
        conversation.AddMessage(message);

        // Assert
        conversation.MessageCount.Should().Be(1);
        conversation.Messages.Should().HaveCount(1);
        conversation.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ChatConversation_EndConversation_ShouldUpdateStatus()
    {
        // Arrange
        var conversation = new ChatConversation { Id = Guid.NewGuid() };

        // Act
        conversation.EndConversation("User closed chat");

        // Assert
        conversation.Status.Should().Be(ConversationStatus.Ended);
        conversation.EndReason.Should().Be("User closed chat");
        conversation.EndedAt.Should().NotBeNull();
        conversation.EndedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ChatConversation_UpdateLeadScore_ShouldSetQualification()
    {
        // Arrange
        var conversation = new ChatConversation { Id = Guid.NewGuid() };

        // Act
        conversation.UpdateLeadScore(0.85, LeadQualification.Hot);

        // Assert
        conversation.LeadScore.Should().Be(0.85);
        conversation.LeadQualification.Should().Be(LeadQualification.Hot);
    }

    [Fact]
    public void ChatConversation_AddTokenUsage_ShouldAccumulateTokensAndCost()
    {
        // Arrange
        var conversation = new ChatConversation { Id = Guid.NewGuid() };

        // Act
        conversation.AddTokenUsage(100, 0.001m);
        conversation.AddTokenUsage(150, 0.0015m);

        // Assert
        conversation.TotalTokensUsed.Should().Be(250);
        conversation.EstimatedCost.Should().Be(0.0025m);
    }
}

public class ChatMessageTests
{
    [Fact]
    public void ChatMessage_CreateUserMessage_ShouldHaveCorrectRole()
    {
        // Arrange
        var conversationId = Guid.NewGuid();

        // Act
        var message = ChatMessage.CreateUserMessage(conversationId, "Test message");

        // Assert
        message.Id.Should().NotBeEmpty();
        message.ConversationId.Should().Be(conversationId);
        message.Role.Should().Be(MessageRole.User);
        message.Content.Should().Be("Test message");
        message.Type.Should().Be(MessageType.Text);
        message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ChatMessage_CreateAssistantMessage_ShouldHaveTokensAndResponseTime()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var responseTime = TimeSpan.FromMilliseconds(500);

        // Act
        var message = ChatMessage.CreateAssistantMessage(conversationId, "AI response", 50, responseTime);

        // Assert
        message.Role.Should().Be(MessageRole.Assistant);
        message.TokenCount.Should().Be(50);
        message.ResponseTime.Should().Be(responseTime);
    }

    [Fact]
    public void ChatMessage_CreateSystemMessage_ShouldHaveSystemRole()
    {
        // Arrange
        var conversationId = Guid.NewGuid();

        // Act
        var message = ChatMessage.CreateSystemMessage(conversationId, "System notification");

        // Assert
        message.Role.Should().Be(MessageRole.System);
        message.Type.Should().Be(MessageType.System);
    }
}

public class ChatbotConfigurationTests
{
    [Fact]
    public void ChatbotConfiguration_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var config = new ChatbotConfiguration();

        // Assert
        config.Name.Should().Be("OKLA Assistant");
        config.Model.Should().Be("gpt-4o-mini");
        config.Temperature.Should().Be(0.7);
        config.MaxTokens.Should().Be(500);
        config.MaxConversationMessages.Should().Be(20);
        config.IsActive.Should().BeTrue();
        config.SystemPrompt.Should().NotBeNullOrEmpty();
        config.SystemPrompt.Should().Contain("OKLA");
    }
}

public class EnumTests
{
    [Fact]
    public void ConversationStatus_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<ConversationStatus>().Should().HaveCount(4);
        ConversationStatus.Active.Should().BeDefined();
        ConversationStatus.Paused.Should().BeDefined();
        ConversationStatus.Ended.Should().BeDefined();
        ConversationStatus.TransferredToAgent.Should().BeDefined();
    }

    [Fact]
    public void LeadQualification_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<LeadQualification>().Should().HaveCount(4);
        LeadQualification.Unknown.Should().BeDefined();
        LeadQualification.Cold.Should().BeDefined();
        LeadQualification.Warm.Should().BeDefined();
        LeadQualification.Hot.Should().BeDefined();
    }

    [Fact]
    public void MessageRole_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<MessageRole>().Should().HaveCount(3);
        MessageRole.User.Should().BeDefined();
        MessageRole.Assistant.Should().BeDefined();
        MessageRole.System.Should().BeDefined();
    }

    [Fact]
    public void MessageType_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<MessageType>().Should().HaveCount(5);
        MessageType.Text.Should().BeDefined();
        MessageType.Image.Should().BeDefined();
        MessageType.System.Should().BeDefined();
        MessageType.Action.Should().BeDefined();
        MessageType.QuickReply.Should().BeDefined();
    }
}

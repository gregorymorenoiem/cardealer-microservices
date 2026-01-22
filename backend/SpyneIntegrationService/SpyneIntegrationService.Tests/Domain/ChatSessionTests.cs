using FluentAssertions;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using Xunit;

namespace SpyneIntegrationService.Tests.Domain;

/// <summary>
/// Tests for Chat entities
/// 
/// ⚠️ FASE 4: Estos tests validan la funcionalidad del Chat AI que 
/// NO se consume en el frontend en esta versión.
/// </summary>
public class ChatSessionTests
{
    [Fact]
    public void ChatSession_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var session = new ChatSession
        {
            VehicleId = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Language = "es",
            Status = ChatSessionStatus.Active
        };

        // Assert
        session.Id.Should().NotBeEmpty();
        session.Status.Should().Be(ChatSessionStatus.Active);
        session.Language.Should().Be("es");
        session.IsQualifiedLead.Should().BeFalse();
    }

    [Fact]
    public void ChatSession_AddMessage_ShouldIncreaseMessageCount()
    {
        // Arrange
        var session = new ChatSession
        {
            VehicleId = Guid.NewGuid(),
            Status = ChatSessionStatus.Active
        };

        // Act
        session.Messages.Add(new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = ChatMessageRole.User,
            Content = "Hola, me interesa este vehículo"
        });

        session.Messages.Add(new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = ChatMessageRole.Assistant,
            Content = "¡Hola! Claro, te cuento más sobre este vehículo..."
        });

        // Assert
        session.Messages.Should().HaveCount(2);
    }

    [Fact]
    public void ChatSession_MarkAsQualifiedLead_ShouldUpdateFlag()
    {
        // Arrange
        var session = new ChatSession
        {
            VehicleId = Guid.NewGuid(),
            Status = ChatSessionStatus.Active,
            IsQualifiedLead = false
        };

        // Act
        session.IsQualifiedLead = true;
        session.LeadInfo = new ChatLeadInfo
        {
            ChatSessionId = session.Id,
            Name = "Juan Pérez",
            Email = "juan@email.com",
            Phone = "809-555-1234",
            InterestType = LeadInterestType.ReadyToBuy,
            LeadScore = 85
        };

        // Assert
        session.IsQualifiedLead.Should().BeTrue();
        session.LeadInfo.Should().NotBeNull();
        session.LeadInfo!.LeadScore.Should().Be(85);
    }

    [Fact]
    public void ChatSession_Close_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var session = new ChatSession
        {
            VehicleId = Guid.NewGuid(),
            Status = ChatSessionStatus.Active
        };

        // Act
        session.Status = ChatSessionStatus.Closed;
        session.ClosedAt = DateTime.UtcNow;
        session.ClosureReason = "User ended conversation";
        session.UserRating = 5;

        // Assert
        session.Status.Should().Be(ChatSessionStatus.Closed);
        session.ClosedAt.Should().NotBeNull();
        session.UserRating.Should().Be(5);
    }

    [Theory]
    [InlineData(ChatMessageRole.User)]
    [InlineData(ChatMessageRole.Assistant)]
    [InlineData(ChatMessageRole.System)]
    public void ChatMessage_ShouldSupportAllRoles(ChatMessageRole role)
    {
        // Arrange & Act
        var message = new ChatMessage
        {
            ChatSessionId = Guid.NewGuid(),
            Role = role,
            Content = "Test message"
        };

        // Assert
        message.Role.Should().Be(role);
    }

    [Theory]
    [InlineData(LeadInterestType.JustLooking)]
    [InlineData(LeadInterestType.ComparingOptions)]
    [InlineData(LeadInterestType.ReadyToBuy)]
    [InlineData(LeadInterestType.NeedsFinancing)]
    [InlineData(LeadInterestType.TradeInInterested)]
    public void ChatLeadInfo_ShouldSupportAllInterestTypes(LeadInterestType interestType)
    {
        // Arrange & Act
        var leadInfo = new ChatLeadInfo
        {
            ChatSessionId = Guid.NewGuid(),
            InterestType = interestType
        };

        // Assert
        leadInfo.InterestType.Should().Be(interestType);
    }
}

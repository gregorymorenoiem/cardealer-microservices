using Xunit;
using FluentAssertions;
using Moq;
using MediatR;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Application.Features.Commands;
using ChatbotService.Application.Features.Queries;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Tests;

public class CreateConversationCommandTests
{
    [Fact]
    public async Task Handle_ShouldCreateNewConversation_WhenNoExistingActive()
    {
        // Arrange
        var mockRepo = new Mock<IChatConversationRepository>();
        mockRepo.Setup(r => r.GetActiveConversationAsync(It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<Guid?>(), default))
            .ReturnsAsync((ChatConversation?)null);
        mockRepo.Setup(r => r.CreateAsync(It.IsAny<ChatConversation>(), default))
            .ReturnsAsync((ChatConversation c, CancellationToken _) => c);

        var handler = new CreateConversationHandler(mockRepo.Object);
        var command = new CreateConversationCommand(
            UserId: Guid.NewGuid(),
            SessionId: "test-session",
            VehicleId: null,
            UserEmail: "test@example.com",
            UserName: "Test User",
            UserPhone: null,
            VehicleContext: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Active");
        result.UserEmail.Should().Be("test@example.com");
        mockRepo.Verify(r => r.CreateAsync(It.IsAny<ChatConversation>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnExisting_WhenActiveConversationExists()
    {
        // Arrange
        var existingConversation = new ChatConversation
        {
            Id = Guid.NewGuid(),
            SessionId = "test-session",
            Status = ConversationStatus.Active
        };

        var mockRepo = new Mock<IChatConversationRepository>();
        mockRepo.Setup(r => r.GetActiveConversationAsync(It.IsAny<Guid?>(), "test-session", It.IsAny<Guid?>(), default))
            .ReturnsAsync(existingConversation);

        var handler = new CreateConversationHandler(mockRepo.Object);
        var command = new CreateConversationCommand(null, "test-session", null, null, null, null, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(existingConversation.Id);
        mockRepo.Verify(r => r.CreateAsync(It.IsAny<ChatConversation>(), default), Times.Never);
    }
}

public class EndConversationCommandTests
{
    [Fact]
    public async Task Handle_ShouldEndConversation_WhenExists()
    {
        // Arrange
        var conversation = new ChatConversation
        {
            Id = Guid.NewGuid(),
            Status = ConversationStatus.Active
        };

        var mockRepo = new Mock<IChatConversationRepository>();
        mockRepo.Setup(r => r.GetByIdWithMessagesAsync(conversation.Id, default))
            .ReturnsAsync(conversation);
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<ChatConversation>(), default))
            .ReturnsAsync((ChatConversation c, CancellationToken _) => c);

        var handler = new EndConversationHandler(mockRepo.Object);
        var command = new EndConversationCommand(conversation.Id, "User closed");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Ended");
        mockRepo.Verify(r => r.UpdateAsync(It.IsAny<ChatConversation>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenConversationNotFound()
    {
        // Arrange
        var mockRepo = new Mock<IChatConversationRepository>();
        mockRepo.Setup(r => r.GetByIdWithMessagesAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((ChatConversation?)null);

        var handler = new EndConversationHandler(mockRepo.Object);
        var command = new EndConversationCommand(Guid.NewGuid(), "Reason");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }
}

public class GetConversationQueryTests
{
    [Fact]
    public async Task Handle_ShouldReturnConversation_WhenExists()
    {
        // Arrange
        var conversation = new ChatConversation
        {
            Id = Guid.NewGuid(),
            SessionId = "test",
            Status = ConversationStatus.Active
        };

        var mockRepo = new Mock<IChatConversationRepository>();
        mockRepo.Setup(r => r.GetByIdWithMessagesAsync(conversation.Id, default))
            .ReturnsAsync(conversation);

        var handler = new GetConversationHandler(mockRepo.Object);
        var query = new GetConversationQuery(conversation.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(conversation.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var mockRepo = new Mock<IChatConversationRepository>();
        mockRepo.Setup(r => r.GetByIdWithMessagesAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((ChatConversation?)null);

        var handler = new GetConversationHandler(mockRepo.Object);
        var query = new GetConversationQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

public class GetUserConversationsQueryTests
{
    [Fact]
    public async Task Handle_ShouldReturnUserConversations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversations = new List<ChatConversation>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Status = ConversationStatus.Active },
            new() { Id = Guid.NewGuid(), UserId = userId, Status = ConversationStatus.Ended }
        };

        var mockRepo = new Mock<IChatConversationRepository>();
        mockRepo.Setup(r => r.GetByUserIdAsync(userId, 0, 20, default))
            .ReturnsAsync(conversations);

        var handler = new GetUserConversationsHandler(mockRepo.Object);
        var query = new GetUserConversationsQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }
}

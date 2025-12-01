using MessageBusService.Application.Commands;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MessageBusService.Tests;

public class PublishMessageCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsTrue()
    {
        // Arrange
        var mockPublisher = new Mock<IMessagePublisher>();
        var mockLogger = new Mock<ILogger<PublishMessageCommandHandler>>();

        mockPublisher.Setup(p => p.PublishAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<MessagePriority>(),
            It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(true);

        var handler = new PublishMessageCommandHandler(mockPublisher.Object, mockLogger.Object);
        var command = new PublishMessageCommand
        {
            Topic = "test.topic",
            Payload = "test payload",
            Priority = MessagePriority.Normal
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        mockPublisher.Verify(p => p.PublishAsync(
            "test.topic",
            "test payload",
            MessagePriority.Normal,
            null), Times.Once);
    }

    [Fact]
    public async Task Handle_PublishFails_ReturnsFalse()
    {
        // Arrange
        var mockPublisher = new Mock<IMessagePublisher>();
        var mockLogger = new Mock<ILogger<PublishMessageCommandHandler>>();

        mockPublisher.Setup(p => p.PublishAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<MessagePriority>(),
            It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(false);

        var handler = new PublishMessageCommandHandler(mockPublisher.Object, mockLogger.Object);
        var command = new PublishMessageCommand
        {
            Topic = "test.topic",
            Payload = "test payload"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}

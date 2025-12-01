using MessageBusService.Application.Commands;
using MessageBusService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MessageBusService.Tests;

public class RetryDeadLetterCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsTrue()
    {
        // Arrange
        var mockDeadLetterManager = new Mock<IDeadLetterManager>();
        var mockLogger = new Mock<ILogger<RetryDeadLetterCommandHandler>>();

        var messageId = Guid.NewGuid();
        mockDeadLetterManager.Setup(d => d.RetryAsync(messageId))
            .ReturnsAsync(true);

        var handler = new RetryDeadLetterCommandHandler(mockDeadLetterManager.Object, mockLogger.Object);
        var command = new RetryDeadLetterCommand
        {
            DeadLetterMessageId = messageId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        mockDeadLetterManager.Verify(d => d.RetryAsync(messageId), Times.Once);
    }

    [Fact]
    public async Task Handle_RetryFails_ReturnsFalse()
    {
        // Arrange
        var mockDeadLetterManager = new Mock<IDeadLetterManager>();
        var mockLogger = new Mock<ILogger<RetryDeadLetterCommandHandler>>();

        var messageId = Guid.NewGuid();
        mockDeadLetterManager.Setup(d => d.RetryAsync(messageId))
            .ReturnsAsync(false);

        var handler = new RetryDeadLetterCommandHandler(mockDeadLetterManager.Object, mockLogger.Object);
        var command = new RetryDeadLetterCommand
        {
            DeadLetterMessageId = messageId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}

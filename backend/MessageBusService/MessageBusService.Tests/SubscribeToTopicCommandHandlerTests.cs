using MessageBusService.Application.Commands;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MessageBusService.Tests;

public class SubscribeToTopicCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSubscriptionId()
    {
        // Arrange
        var mockSubscriber = new Mock<IMessageSubscriber>();
        var mockLogger = new Mock<ILogger<SubscribeToTopicCommandHandler>>();

        var subscriptionId = Guid.NewGuid();
        var subscription = new Subscription
        {
            Id = subscriptionId,
            Topic = "test.topic",
            ConsumerName = "test-consumer",
            IsActive = true
        };

        mockSubscriber.Setup(s => s.SubscribeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(subscription);

        var handler = new SubscribeToTopicCommandHandler(mockSubscriber.Object, mockLogger.Object);
        var command = new SubscribeToTopicCommand
        {
            Topic = "test.topic",
            ConsumerName = "test-consumer"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(subscriptionId, result);
        mockSubscriber.Verify(s => s.SubscribeAsync("test.topic", "test-consumer"), Times.Once);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ThrowsException()
    {
        // Arrange
        var mockSubscriber = new Mock<IMessageSubscriber>();
        var mockLogger = new Mock<ILogger<SubscribeToTopicCommandHandler>>();

        mockSubscriber.Setup(s => s.SubscribeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Subscription failed"));

        var handler = new SubscribeToTopicCommandHandler(mockSubscriber.Object, mockLogger.Object);
        var command = new SubscribeToTopicCommand
        {
            Topic = "test.topic",
            ConsumerName = "test-consumer"
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }
}

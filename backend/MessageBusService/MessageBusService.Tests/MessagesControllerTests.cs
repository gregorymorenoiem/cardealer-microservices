using MessageBusService.Api.Controllers;
using MessageBusService.Application.Commands;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;
using MessageBusService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MessageBusService.Tests;

public class MessagesControllerTests
{
    [Fact]
    public async Task PublishMessage_ValidCommand_ReturnsOk()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var mockPublisher = new Mock<IMessagePublisher>();
        var mockLogger = new Mock<ILogger<MessagesController>>();

        mockMediator.Setup(m => m.Send(It.IsAny<PublishMessageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new MessagesController(mockMediator.Object, mockPublisher.Object, mockLogger.Object);
        var command = new PublishMessageCommand
        {
            Topic = "test.topic",
            Payload = "test payload",
            Priority = MessagePriority.Normal
        };

        // Act
        var result = await controller.PublishMessage(command);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMessageStatus_ExistingMessage_ReturnsOk()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var mockPublisher = new Mock<IMessagePublisher>();
        var mockLogger = new Mock<ILogger<MessagesController>>();

        var messageId = Guid.NewGuid();
        var message = new Message
        {
            Id = messageId,
            Topic = "test.topic",
            Payload = "test payload",
            Status = MessageStatus.Completed
        };

        mockPublisher.Setup(p => p.GetMessageStatusAsync(messageId))
            .ReturnsAsync(message);

        var controller = new MessagesController(mockMediator.Object, mockPublisher.Object, mockLogger.Object);

        // Act
        var result = await controller.GetMessageStatus(messageId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(message, okResult.Value);
    }
}

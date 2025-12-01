using MessageBusService.Api.Controllers;
using MessageBusService.Application.Commands;
using MessageBusService.Application.Queries;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MessageBusService.Tests;

public class DeadLetterControllerTests
{
    [Fact]
    public async Task GetDeadLetters_ReturnsListOfMessages()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var mockDeadLetterManager = new Mock<IDeadLetterManager>();
        var mockLogger = new Mock<ILogger<DeadLetterController>>();

        var deadLetters = new List<DeadLetterMessage>
        {
            new DeadLetterMessage { Id = Guid.NewGuid(), Topic = "test.topic", Payload = "test" }
        };

        mockMediator.Setup(m => m.Send(It.IsAny<GetDeadLettersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deadLetters);

        var controller = new DeadLetterController(mockMediator.Object, mockDeadLetterManager.Object, mockLogger.Object);

        // Act
        var result = await controller.GetDeadLetters(1, 50);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedList = Assert.IsType<List<DeadLetterMessage>>(okResult.Value);
        Assert.Single(returnedList);
    }

    [Fact]
    public async Task Retry_ValidMessage_ReturnsOk()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var mockDeadLetterManager = new Mock<IDeadLetterManager>();
        var mockLogger = new Mock<ILogger<DeadLetterController>>();

        var messageId = Guid.NewGuid();
        mockMediator.Setup(m => m.Send(It.IsAny<RetryDeadLetterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new DeadLetterController(mockMediator.Object, mockDeadLetterManager.Object, mockLogger.Object);

        // Act
        var result = await controller.Retry(messageId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}

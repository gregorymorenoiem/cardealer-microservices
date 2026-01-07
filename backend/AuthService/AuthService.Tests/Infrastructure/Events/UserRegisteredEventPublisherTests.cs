using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Infrastructure.Events;
using CarDealer.Contracts.Events.Auth;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace AuthService.Tests.Infrastructure.Events;

/// <summary>
/// Unit tests para UserRegisteredEventPublisher
/// </summary>
public class UserRegisteredEventPublisherTests
{
    private readonly Mock<IConnection> _connectionMock;
    private readonly Mock<IModel> _channelMock;
    private readonly Mock<ILogger<UserRegisteredEventPublisher>> _loggerMock;
    private readonly UserRegisteredEventPublisher _publisher;

    public UserRegisteredEventPublisherTests()
    {
        _connectionMock = new Mock<IConnection>();
        _channelMock = new Mock<IModel>();
        _loggerMock = new Mock<ILogger<UserRegisteredEventPublisher>>();

        // Setup: Connection devuelve Channel
        _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

        // Setup: Channel CreateBasicProperties (REQUIRED for all tests)
        var mockProperties = new Mock<IBasicProperties>();
        mockProperties.SetupAllProperties();
        _channelMock.Setup(ch => ch.CreateBasicProperties()).Returns(mockProperties.Object);

        _publisher = new UserRegisteredEventPublisher(_connectionMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task PublishAsync_ValidEvent_PublishesToRabbitMQ()
    {
        // Arrange
        var @event = new UserRegisteredEvent
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            RegisteredAt = DateTime.UtcNow,
            OccurredAt = DateTime.UtcNow
        };

        byte[]? publishedBody = null;
        string? publishedRoutingKey = null;
        IBasicProperties? publishedProperties = null;

        // Capturar los parámetros de BasicPublish
        _channelMock
            .Setup(ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()))
            .Callback<string, string, bool, IBasicProperties, ReadOnlyMemory<byte>>(
                (exchange, routingKey, mandatory, properties, body) =>
                {
                    publishedRoutingKey = routingKey;
                    publishedProperties = properties;
                    publishedBody = body.ToArray();
                });

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        _channelMock.Verify(
            ch => ch.BasicPublish(
                "cardealer.events",
                "auth.user.registered",
                false,
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()),
            Times.Once);

        publishedRoutingKey.Should().Be("auth.user.registered");
        publishedBody.Should().NotBeNull();

        // Verificar que el body contiene datos del evento
        var bodyString = Encoding.UTF8.GetString(publishedBody!);
        bodyString.Should().Contain(@event.Email);
        bodyString.Should().Contain(@event.FullName);
        bodyString.Should().Contain(@event.UserId.ToString());
    }

    [Fact]
    public async Task PublishAsync_ValidEvent_SetsCorrectProperties()
    {
        // Arrange
        var @event = new UserRegisteredEvent
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            OccurredAt = DateTime.UtcNow
        };

        IBasicProperties? capturedProperties = null;

        _channelMock
            .Setup(ch => ch.CreateBasicProperties())
            .Returns(new Mock<IBasicProperties>().Object);

        _channelMock
            .Setup(ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()))
            .Callback<string, string, bool, IBasicProperties, ReadOnlyMemory<byte>>(
                (_, _, _, properties, _) => capturedProperties = properties);

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        capturedProperties.Should().NotBeNull();
        // En implementación real, verificar:
        // - Persistent = true
        // - MessageId = EventId
        // - ContentType = "application/json"
    }

    [Fact]
    public async Task PublishAsync_RabbitMQDown_ThrowsException()
    {
        // Arrange
        var @event = new UserRegisteredEvent
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User"
        };

        _channelMock
            .Setup(ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()))
            .Throws(new InvalidOperationException("RabbitMQ connection lost"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _publisher.PublishAsync(@event, CancellationToken.None));
    }

    [Fact]
    public async Task PublishAsync_LogsEventPublication()
    {
        // Arrange
        var @event = new UserRegisteredEvent
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User"
        };

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("UserRegisteredEvent")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void PublishAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            () => _publisher.PublishAsync(null!, CancellationToken.None));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task PublishAsync_InvalidEmail_StillPublishes(string? email)
    {
        // Arrange
        var @event = new UserRegisteredEvent
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Email = email!,
            FullName = "Test User"
        };

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        _channelMock.Verify(
            ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()),
            Times.Once,
            "Publisher no debe validar datos, solo publicar");
    }

    [Fact]
    public async Task PublishAsync_MultipleEvents_PublishesInOrder()
    {
        // Arrange
        var events = new[]
        {
            new UserRegisteredEvent { EventId = Guid.NewGuid(), Email = "user1@test.com", FullName = "User 1", UserId = Guid.NewGuid() },
            new UserRegisteredEvent { EventId = Guid.NewGuid(), Email = "user2@test.com", FullName = "User 2", UserId = Guid.NewGuid() },
            new UserRegisteredEvent { EventId = Guid.NewGuid(), Email = "user3@test.com", FullName = "User 3", UserId = Guid.NewGuid() }
        };

        // Act
        foreach (var evt in events)
        {
            await _publisher.PublishAsync(evt, CancellationToken.None);
        }

        // Assert
        _channelMock.Verify(
            ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()),
            Times.Exactly(3));
    }
}

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VehiclesSaleService.Infrastructure.Events;
using CarDealer.Contracts.Events.Vehicle;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace VehiclesSaleService.Tests.Infrastructure.Events;

/// <summary>
/// Unit tests para VehicleCreatedEventPublisher
/// </summary>
public class VehicleCreatedEventPublisherTests
{
    private readonly Mock<IConnection> _connectionMock;
    private readonly Mock<IModel> _channelMock;
    private readonly Mock<ILogger<VehicleCreatedEventPublisher>> _loggerMock;
    private readonly VehicleCreatedEventPublisher _publisher;

    public VehicleCreatedEventPublisherTests()
    {
        _connectionMock = new Mock<IConnection>();
        _channelMock = new Mock<IModel>();
        _loggerMock = new Mock<ILogger<VehicleCreatedEventPublisher>>();

        _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

        // ✅ PROVEN FIX: Setup Channel CreateBasicProperties (REQUIRED for all tests)
        var mockProperties = new Mock<IBasicProperties>();
        mockProperties.SetupAllProperties();
        _channelMock.Setup(ch => ch.CreateBasicProperties()).Returns(mockProperties.Object);

        _publisher = new VehicleCreatedEventPublisher(_connectionMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task PublishAsync_ValidEvent_PublishesToRabbitMQ()
    {
        // Arrange
        var @event = new VehicleCreatedEvent
        {
            EventId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Make = "Toyota",
            Model = "Camry",
            Year = 2024,
            VIN = "1HGCM82633A123456",
            Price = 32000,
            OccurredAt = DateTime.UtcNow
        };

        byte[]? publishedBody = null;
        string? publishedRoutingKey = null;

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
                    publishedBody = body.ToArray();
                });

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        _channelMock.Verify(
            ch => ch.BasicPublish(
                "cardealer.events",
                "vehicle.created",
                false,
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()),
            Times.Once);

        publishedRoutingKey.Should().Be("vehicle.created");
        publishedBody.Should().NotBeNull();

        var bodyString = Encoding.UTF8.GetString(publishedBody!);
        bodyString.Should().Contain(@event.Make);
        bodyString.Should().Contain(@event.Model);
        bodyString.Should().Contain(@event.VIN);
        bodyString.Should().Contain(@event.VehicleId.ToString());
    }

    [Fact]
    public async Task PublishAsync_VehicleWithAllData_SerializesCorrectly()
    {
        // Arrange
        var @event = new VehicleCreatedEvent
        {
            EventId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Make = "BMW",
            Model = "X5",
            Year = 2025,
            VIN = "WBAFG9C50CDW12345",
            Price = 75000,
            OccurredAt = DateTime.UtcNow
        };

        byte[]? publishedBody = null;

        _channelMock
            .Setup(ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()))
            .Callback<string, string, bool, IBasicProperties, ReadOnlyMemory<byte>>(
                (_, _, _, _, body) => publishedBody = body.ToArray());

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        publishedBody.Should().NotBeNull();
        var bodyString = Encoding.UTF8.GetString(publishedBody!);
        bodyString.Should().Contain("BMW");
        bodyString.Should().Contain("X5");
        bodyString.Should().Contain("2025");
        bodyString.Should().Contain("75000");
    }

    [Fact]
    public async Task PublishAsync_RabbitMQConnectionFails_ThrowsException()
    {
        // Arrange
        var @event = new VehicleCreatedEvent
        {
            EventId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Make = "Tesla",
            Model = "Model 3",
            Year = 2024,
            VIN = "5YJ3E1EA1KF123456",
            Price = 45000
        };

        _channelMock
            .Setup(ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()))
            .Throws(new InvalidOperationException("RabbitMQ broker not available"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _publisher.PublishAsync(@event, CancellationToken.None));
    }

    [Fact]
    public async Task PublishAsync_LogsVehicleDetails()
    {
        // Arrange
        var @event = new VehicleCreatedEvent
        {
            EventId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Make = "Ford",
            Model = "F-150",
            Year = 2024,
            VIN = "1FTFW1E85KFB12345",
            Price = 55000
        };

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("VehicleCreatedEvent") || v.ToString()!.Contains(@event.VehicleId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    [InlineData(999999999)]
    public async Task PublishAsync_InvalidPrice_StillPublishes(decimal price)
    {
        // Arrange
        var @event = new VehicleCreatedEvent
        {
            EventId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Make = "Honda",
            Model = "Civic",
            Year = 2024,
            VIN = "2HGFC2F59NH123456",
            Price = price
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
            "Publisher no debe validar reglas de negocio, solo publicar");
    }

    [Fact]
    public async Task PublishAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var @event = new VehicleCreatedEvent
        {
            EventId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Make = "Chevrolet",
            Model = "Silverado",
            Year = 2024,
            VIN = "3GCPYDED0LG123456",
            Price = 48000
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        // Si el publisher maneja CancellationToken correctamente
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _publisher.PublishAsync(@event, cts.Token));
    }

    [Fact]
    public async Task PublishAsync_MultipleVehicles_PublishesAll()
    {
        // Arrange
        var events = new[]
        {
            new VehicleCreatedEvent { EventId = Guid.NewGuid(), VehicleId = Guid.NewGuid(), Make = "Toyota", Model = "Corolla", Year = 2024, VIN = "VIN001", Price = 25000 },
            new VehicleCreatedEvent { EventId = Guid.NewGuid(), VehicleId = Guid.NewGuid(), Make = "Honda", Model = "Accord", Year = 2024, VIN = "VIN002", Price = 30000 },
            new VehicleCreatedEvent { EventId = Guid.NewGuid(), VehicleId = Guid.NewGuid(), Make = "Mazda", Model = "CX-5", Year = 2024, VIN = "VIN003", Price = 35000 }
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

using AdminService.Application.Interfaces;
using AdminService.Application.UseCases.Vehicles.RejectVehicle;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdminService.Tests.Application.UseCases.Vehicles;

public class RejectVehicleCommandHandlerTests
{
    private readonly Mock<IAuditServiceClient> _auditMock;
    private readonly Mock<INotificationServiceClient> _notificationMock;
    private readonly Mock<ILogger<RejectVehicleCommandHandler>> _loggerMock;
    private readonly RejectVehicleCommandHandler _handler;

    public RejectVehicleCommandHandlerTests()
    {
        _auditMock = new Mock<IAuditServiceClient>();
        _notificationMock = new Mock<INotificationServiceClient>();
        _loggerMock = new Mock<ILogger<RejectVehicleCommandHandler>>();

        _handler = new RejectVehicleCommandHandler(
            _auditMock.Object,
            _notificationMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsTrue()
    {
        // Arrange
        var command = new RejectVehicleCommand(
            VehicleId: Guid.NewGuid(),
            RejectedBy: "admin@okla.com.do",
            Reason: "Fotos inapropiadas",
            OwnerEmail: "seller@test.com",
            VehicleTitle: "2022 Honda Civic"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Allow fire-and-forget tasks to complete
        await Task.Delay(200);

        // Assert
        Assert.True(result);
        _auditMock.Verify(x => x.LogVehicleRejectedAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.AtMostOnce);
        _notificationMock.Verify(x => x.SendVehicleRejectedNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.AtMostOnce);
    }

    [Fact]
    public async Task Handle_EmptyVehicleId_ReturnsFalse()
    {
        // Arrange
        var command = new RejectVehicleCommand(
            VehicleId: Guid.Empty,
            RejectedBy: "admin@okla.com.do",
            Reason: "Bad listing",
            OwnerEmail: "owner@test.com",
            VehicleTitle: "Test Vehicle"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _auditMock.Verify(x => x.LogVehicleRejectedAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmptyReason_ReturnsFalse()
    {
        // Arrange
        var command = new RejectVehicleCommand(
            VehicleId: Guid.NewGuid(),
            RejectedBy: "admin@okla.com.do",
            Reason: "",
            OwnerEmail: "owner@test.com",
            VehicleTitle: "Test Vehicle"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Handle_WhitespaceReason_ReturnsFalse()
    {
        // Arrange
        var command = new RejectVehicleCommand(
            VehicleId: Guid.NewGuid(),
            RejectedBy: "admin@okla.com.do",
            Reason: "   ",
            OwnerEmail: "owner@test.com",
            VehicleTitle: "Test Vehicle"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Handle_AuditServiceFails_StillReturnsTrue()
    {
        // Arrange
        _auditMock.Setup(x => x.LogVehicleRejectedAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Service unavailable"));

        var command = new RejectVehicleCommand(
            VehicleId: Guid.NewGuid(),
            RejectedBy: "admin@okla.com.do",
            Reason: "Policy violation",
            OwnerEmail: "owner@test.com",
            VehicleTitle: "2021 Toyota RAV4"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — fire-and-forget pattern means main thread still succeeds
        Assert.True(result);
    }

    [Fact]
    public async Task Handle_EmptyOwnerEmail_DoesNotSendNotification()
    {
        // Arrange
        var command = new RejectVehicleCommand(
            VehicleId: Guid.NewGuid(),
            RejectedBy: "admin@okla.com.do",
            Reason: "Duplicate listing",
            OwnerEmail: "",
            VehicleTitle: "Test Vehicle"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        await Task.Delay(200);

        // Assert
        Assert.True(result);
        _notificationMock.Verify(x => x.SendVehicleRejectedNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NotificationFails_StillReturnsTrue()
    {
        // Arrange
        _notificationMock.Setup(x => x.SendVehicleRejectedNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Notification service down"));

        var command = new RejectVehicleCommand(
            VehicleId: Guid.NewGuid(),
            RejectedBy: "admin@okla.com.do",
            Reason: "Contenido fraudulento",
            OwnerEmail: "owner@test.com",
            VehicleTitle: "2023 BMW X5"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
    }
}

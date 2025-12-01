using AdminService.Application.Interfaces;
using AdminService.Application.UseCases.Vehicles.ApproveVehicle;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdminService.Tests.Application.UseCases.Vehicles;

public class ApproveVehicleCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsTrue()
    {
        // Arrange
        var auditMock = new Mock<IAuditServiceClient>();
        var notificationMock = new Mock<INotificationServiceClient>();
        var loggerMock = new Mock<ILogger<ApproveVehicleCommandHandler>>();
        
        var handler = new ApproveVehicleCommandHandler(
            auditMock.Object,
            notificationMock.Object,
            loggerMock.Object
        );

        var command = new ApproveVehicleCommand(
            VehicleId: Guid.NewGuid(),
            ApprovedBy: "admin@test.com",
            Reason: "Quality approved",
            OwnerEmail: "owner@test.com",
            VehicleTitle: "2020 Toyota Camry"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        auditMock.Verify(x => x.LogVehicleApprovedAsync(
            It.IsAny<Guid>(), 
            It.IsAny<string>(), 
            It.IsAny<string>()), Times.Once);
        notificationMock.Verify(x => x.SendVehicleApprovedNotificationAsync(
            It.IsAny<string>(), 
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AuditServiceFails_StillReturnsTrue()
    {
        // Arrange
        var auditMock = new Mock<IAuditServiceClient>();
        auditMock.Setup(x => x.LogVehicleApprovedAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Service unavailable"));

        var notificationMock = new Mock<INotificationServiceClient>();
        var loggerMock = new Mock<ILogger<ApproveVehicleCommandHandler>>();
        
        var handler = new ApproveVehicleCommandHandler(
            auditMock.Object,
            notificationMock.Object,
            loggerMock.Object
        );

        var command = new ApproveVehicleCommand(
            Guid.NewGuid(),
            "admin@test.com",
            "Approved",
            "owner@test.com",
            "Test Vehicle"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Should still return true even if audit fails
        Assert.True(result);
    }
}

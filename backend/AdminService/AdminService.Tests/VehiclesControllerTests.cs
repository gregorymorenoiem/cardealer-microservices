using AdminService.Api.Controllers;
using AdminService.Application.UseCases.Vehicles.ApproveVehicle;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdminService.Tests.Controllers;

public class VehiclesControllerTests
{
    [Fact]
    public async Task ApproveVehicle_ValidRequest_ReturnsOk()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<VehiclesController>>();
        
        mediatorMock.Setup(x => x.Send(It.IsAny<ApproveVehicleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new VehiclesController(mediatorMock.Object, loggerMock.Object);
        var vehicleId = Guid.NewGuid();
        var request = new ApproveVehicleRequest(
            ApprovedBy: "admin@test.com",
            Reason: "Quality approved",
            OwnerEmail: "owner@test.com",
            VehicleTitle: "2020 Toyota Camry"
        );

        // Act
        var result = await controller.ApproveVehicle(vehicleId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult);
        mediatorMock.Verify(x => x.Send(It.IsAny<ApproveVehicleCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveVehicle_ExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<VehiclesController>>();
        
        mediatorMock.Setup(x => x.Send(It.IsAny<ApproveVehicleCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var controller = new VehiclesController(mediatorMock.Object, loggerMock.Object);
        var vehicleId = Guid.NewGuid();
        var request = new ApproveVehicleRequest(
            "admin@test.com",
            "Approved",
            "owner@test.com",
            "Test Vehicle"
        );

        // Act
        var result = await controller.ApproveVehicle(vehicleId, request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}

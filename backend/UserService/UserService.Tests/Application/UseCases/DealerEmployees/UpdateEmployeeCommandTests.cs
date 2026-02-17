using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.UseCases.DealerEmployees;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Tests.Application.UseCases.DealerEmployees;

public class UpdateEmployeeCommandTests
{
    private readonly Mock<IDealerEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<ILogger<UpdateEmployeeCommandHandler>> _loggerMock;
    private readonly UpdateEmployeeCommandHandler _handler;

    public UpdateEmployeeCommandTests()
    {
        _employeeRepositoryMock = new Mock<IDealerEmployeeRepository>();
        _loggerMock = new Mock<ILogger<UpdateEmployeeCommandHandler>>();
        _handler = new UpdateEmployeeCommandHandler(
            _employeeRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateRole_WhenValidRoleProvided()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var employee = new DealerEmployee
        {
            Id = employeeId,
            DealerId = dealerId,
            UserId = Guid.NewGuid(),
            DealerRole = DealerRole.Salesperson,
            Status = EmployeeStatus.Active
        };

        _employeeRepositoryMock.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync(employee);

        var command = new UpdateEmployeeCommand(dealerId, employeeId, Role: "Manager");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _employeeRepositoryMock.Verify(r => r.UpdateAsync(It.Is<DealerEmployee>(
            e => e.DealerRole == DealerRole.Manager)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateStatus_WhenValidStatusProvided()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var employee = new DealerEmployee
        {
            Id = employeeId,
            DealerId = dealerId,
            UserId = Guid.NewGuid(),
            DealerRole = DealerRole.Salesperson,
            Status = EmployeeStatus.Pending
        };

        _employeeRepositoryMock.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync(employee);

        var command = new UpdateEmployeeCommand(dealerId, employeeId, Status: "Active");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _employeeRepositoryMock.Verify(r => r.UpdateAsync(It.Is<DealerEmployee>(
            e => e.Status == EmployeeStatus.Active)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenEmployeeNotFound()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        _employeeRepositoryMock.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync((DealerEmployee?)null);

        var command = new UpdateEmployeeCommand(dealerId, employeeId, Role: "Manager");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenInvalidRole()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var employee = new DealerEmployee
        {
            Id = employeeId,
            DealerId = dealerId,
            DealerRole = DealerRole.Salesperson
        };

        _employeeRepositoryMock.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync(employee);

        var command = new UpdateEmployeeCommand(dealerId, employeeId, Role: "InvalidRole");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenInvalidStatus()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var employee = new DealerEmployee
        {
            Id = employeeId,
            DealerId = dealerId,
            Status = EmployeeStatus.Pending
        };

        _employeeRepositoryMock.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync(employee);

        var command = new UpdateEmployeeCommand(dealerId, employeeId, Status: "InvalidStatus");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}

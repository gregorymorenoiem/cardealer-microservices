using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.UseCases.DealerEmployees;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Tests.Application.UseCases.DealerEmployees;

public class RemoveEmployeeCommandTests
{
    private readonly Mock<IDealerEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<ILogger<RemoveEmployeeCommandHandler>> _loggerMock;
    private readonly RemoveEmployeeCommandHandler _handler;

    public RemoveEmployeeCommandTests()
    {
        _employeeRepositoryMock = new Mock<IDealerEmployeeRepository>();
        _loggerMock = new Mock<ILogger<RemoveEmployeeCommandHandler>>();
        _handler = new RemoveEmployeeCommandHandler(
            _employeeRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteEmployee_WhenEmployeeExists()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var employee = new DealerEmployee
        {
            Id = employeeId,
            DealerId = dealerId,
            UserId = Guid.NewGuid(),
            DealerRole = DealerRole.Manager
        };

        _employeeRepositoryMock.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        var command = new RemoveEmployeeCommand(dealerId, employeeId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _employeeRepositoryMock.Verify(r => r.DeleteAsync(employeeId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenEmployeeNotFound()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        _employeeRepositoryMock.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync((DealerEmployee?)null);

        var command = new RemoveEmployeeCommand(dealerId, employeeId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenEmployeeBelongsToDifferentDealer()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var differentDealerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var employee = new DealerEmployee
        {
            Id = employeeId,
            DealerId = differentDealerId,
            UserId = Guid.NewGuid()
        };

        _employeeRepositoryMock.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        var command = new RemoveEmployeeCommand(dealerId, employeeId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

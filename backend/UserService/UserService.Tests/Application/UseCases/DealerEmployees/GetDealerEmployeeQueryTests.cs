using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.UseCases.DealerEmployees;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Tests.Application.UseCases.DealerEmployees;

public class GetDealerEmployeeQueryTests
{
    private readonly Mock<IDealerEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<GetDealerEmployeeQueryHandler>> _loggerMock;
    private readonly GetDealerEmployeeQueryHandler _handler;

    public GetDealerEmployeeQueryTests()
    {
        _employeeRepositoryMock = new Mock<IDealerEmployeeRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<GetDealerEmployeeQueryHandler>>();
        _handler = new GetDealerEmployeeQueryHandler(
            _employeeRepositoryMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmployee_WhenEmployeeExists()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var employee = new DealerEmployee
        {
            Id = employeeId,
            UserId = userId,
            DealerId = dealerId,
            DealerRole = DealerRole.Manager,
            Status = EmployeeStatus.Active,
            InvitationDate = DateTime.UtcNow.AddDays(-30)
        };

        var user = new User
        {
            Id = userId,
            Email = "employee@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _employeeRepositoryMock.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var query = new GetDealerEmployeeQuery(dealerId, employeeId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(employeeId);
        result.UserFullName.Should().Be("John Doe");
        result.Role.Should().Be("Manager");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenEmployeeNotFound()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        _employeeRepositoryMock.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync((DealerEmployee?)null);

        var query = new GetDealerEmployeeQuery(dealerId, employeeId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

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
            UserId = Guid.NewGuid(),
            DealerId = differentDealerId, // Different dealer
            DealerRole = DealerRole.Manager,
            Status = EmployeeStatus.Active
        };

        _employeeRepositoryMock.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        var query = new GetDealerEmployeeQuery(dealerId, employeeId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

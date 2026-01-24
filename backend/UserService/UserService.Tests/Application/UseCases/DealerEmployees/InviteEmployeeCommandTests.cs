using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.UseCases.DealerEmployees;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Tests.Application.UseCases.DealerEmployees;

public class InviteEmployeeCommandTests
{
    private readonly Mock<IDealerEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IDealerRepository> _dealerRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<InviteEmployeeCommandHandler>> _loggerMock;
    private readonly InviteEmployeeCommandHandler _handler;

    public InviteEmployeeCommandTests()
    {
        _employeeRepositoryMock = new Mock<IDealerEmployeeRepository>();
        _dealerRepositoryMock = new Mock<IDealerRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<InviteEmployeeCommandHandler>>();
        _handler = new InviteEmployeeCommandHandler(
            _employeeRepositoryMock.Object,
            _dealerRepositoryMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateEmployee_WhenValidRequest()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var dealer = new Dealer { Id = dealerId, BusinessName = "Test Dealer" };
        var user = new User { Id = userId, Email = "user@test.com", FirstName = "John", LastName = "Doe" };

        _dealerRepositoryMock.Setup(r => r.GetByIdAsync(dealerId)).ReturnsAsync(dealer);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _employeeRepositoryMock.Setup(r => r.GetByUserIdAndDealerIdAsync(userId, dealerId))
            .ReturnsAsync((DealerEmployee?)null);
        _employeeRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DealerEmployee>()))
            .ReturnsAsync((DealerEmployee e) => e);

        var command = new InviteEmployeeCommand(dealerId, userId, "Manager");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DealerId.Should().Be(dealerId);
        result.UserId.Should().Be(userId);
        result.Role.Should().Be("Manager");
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenDealerNotFound()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        _dealerRepositoryMock.Setup(r => r.GetByIdAsync(dealerId)).ReturnsAsync((Dealer?)null);

        var command = new InviteEmployeeCommand(dealerId, Guid.NewGuid(), "Manager");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var dealer = new Dealer { Id = dealerId };
        _dealerRepositoryMock.Setup(r => r.GetByIdAsync(dealerId)).ReturnsAsync(dealer);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var command = new InviteEmployeeCommand(dealerId, userId, "Manager");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenUserAlreadyEmployee()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var dealer = new Dealer { Id = dealerId };
        var user = new User { Id = userId };
        var existingEmployee = new DealerEmployee { Id = Guid.NewGuid(), UserId = userId, DealerId = dealerId };

        _dealerRepositoryMock.Setup(r => r.GetByIdAsync(dealerId)).ReturnsAsync(dealer);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _employeeRepositoryMock.Setup(r => r.GetByUserIdAndDealerIdAsync(userId, dealerId))
            .ReturnsAsync(existingEmployee);

        var command = new InviteEmployeeCommand(dealerId, userId, "Manager");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenInvalidRole()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var dealer = new Dealer { Id = dealerId };
        var user = new User { Id = userId };

        _dealerRepositoryMock.Setup(r => r.GetByIdAsync(dealerId)).ReturnsAsync(dealer);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _employeeRepositoryMock.Setup(r => r.GetByUserIdAndDealerIdAsync(userId, dealerId))
            .ReturnsAsync((DealerEmployee?)null);

        var command = new InviteEmployeeCommand(dealerId, userId, "InvalidRole");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}

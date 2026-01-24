using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.UseCases.DealerEmployees;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Tests.Application.UseCases.DealerEmployees;

public class GetDealerEmployeesQueryTests
{
    private readonly Mock<IDealerEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<GetDealerEmployeesQueryHandler>> _loggerMock;
    private readonly GetDealerEmployeesQueryHandler _handler;

    public GetDealerEmployeesQueryTests()
    {
        _employeeRepositoryMock = new Mock<IDealerEmployeeRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<GetDealerEmployeesQueryHandler>>();
        _handler = new GetDealerEmployeesQueryHandler(
            _employeeRepositoryMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmployeeList_WhenEmployeesExist()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var employees = new List<DealerEmployee>
        {
            new DealerEmployee
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DealerId = dealerId,
                DealerRole = DealerRole.Manager,
                Status = EmployeeStatus.Active,
                InvitationDate = DateTime.UtcNow.AddDays(-30),
                ActivationDate = DateTime.UtcNow.AddDays(-29)
            }
        };

        var user = new User
        {
            Id = userId,
            Email = "employee@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _employeeRepositoryMock.Setup(r => r.GetByDealerIdAsync(dealerId))
            .ReturnsAsync(employees);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var query = new GetDealerEmployeesQuery(dealerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].UserFullName.Should().Be("John Doe");
        result[0].UserEmail.Should().Be("employee@test.com");
        result[0].Role.Should().Be("Manager");
        result[0].Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoEmployees()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        _employeeRepositoryMock.Setup(r => r.GetByDealerIdAsync(dealerId))
            .ReturnsAsync(new List<DealerEmployee>());

        var query = new GetDealerEmployeesQuery(dealerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

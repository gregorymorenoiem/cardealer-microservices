using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.UseCases.DealerOnboarding;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Tests.Application.UseCases.DealerOnboarding;

public class RegisterDealerCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDealerRepository> _dealerRepositoryMock;
    private readonly Mock<IDealerOnboardingRepository> _onboardingRepositoryMock;
    private readonly Mock<ILogger<RegisterDealerCommandHandler>> _loggerMock;
    private readonly RegisterDealerCommandHandler _handler;

    public RegisterDealerCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _dealerRepositoryMock = new Mock<IDealerRepository>();
        _onboardingRepositoryMock = new Mock<IDealerOnboardingRepository>();
        _loggerMock = new Mock<ILogger<RegisterDealerCommandHandler>>();
        _handler = new RegisterDealerCommandHandler(
            _userRepositoryMock.Object,
            _dealerRepositoryMock.Object,
            _onboardingRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateDealerAndOnboarding_WhenValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "user@test.com", FirstName = "John", LastName = "Doe" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _dealerRepositoryMock.Setup(r => r.GetByOwnerIdAsync(userId)).ReturnsAsync((Dealer?)null);
        _dealerRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Dealer>()))
            .ReturnsAsync((Dealer d) => d);
        _onboardingRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<DealerOnboardingProcess>()))
            .ReturnsAsync((DealerOnboardingProcess o) => o);

        var command = new RegisterDealerCommand(userId, "Test Business", "business@test.com", "809-555-1234", "Starter");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DealerId.Should().NotBeEmpty();
        result.Status.Should().Be("InProgress");
        result.CurrentStep.Should().Be("BasicInfo");
        result.Progress.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var command = new RegisterDealerCommand(userId, "Test Business", "business@test.com", null, "Starter");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenUserAlreadyHasDealer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };
        var existingDealer = new Dealer { Id = Guid.NewGuid(), OwnerUserId = userId };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _dealerRepositoryMock.Setup(r => r.GetByOwnerIdAsync(userId)).ReturnsAsync(existingDealer);

        var command = new RegisterDealerCommand(userId, "Test Business", "business@test.com", null, "Starter");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}

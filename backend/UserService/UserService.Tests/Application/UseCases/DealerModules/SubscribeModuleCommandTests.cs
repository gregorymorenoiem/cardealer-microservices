using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.UseCases.DealerModules;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Tests.Application.UseCases.DealerModules;

public class SubscribeModuleCommandTests
{
    private readonly Mock<IDealerRepository> _dealerRepositoryMock;
    private readonly Mock<IModuleRepository> _moduleRepositoryMock;
    private readonly Mock<IDealerModuleRepository> _dealerModuleRepositoryMock;
    private readonly Mock<ILogger<SubscribeModuleCommandHandler>> _loggerMock;
    private readonly SubscribeModuleCommandHandler _handler;

    public SubscribeModuleCommandTests()
    {
        _dealerRepositoryMock = new Mock<IDealerRepository>();
        _moduleRepositoryMock = new Mock<IModuleRepository>();
        _dealerModuleRepositoryMock = new Mock<IDealerModuleRepository>();
        _loggerMock = new Mock<ILogger<SubscribeModuleCommandHandler>>();
        _handler = new SubscribeModuleCommandHandler(
            _dealerRepositoryMock.Object,
            _moduleRepositoryMock.Object,
            _dealerModuleRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateSubscription_WhenAllValidationsPass()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();

        var dealer = new Dealer { Id = dealerId, BusinessName = "Test Dealer" };
        var module = new Module { Id = moduleId, Name = "CRM Module", IsActive = true, Price = 49.99m };

        _dealerRepositoryMock.Setup(r => r.GetByIdAsync(dealerId)).ReturnsAsync(dealer);
        _moduleRepositoryMock.Setup(r => r.GetByIdAsync(moduleId)).ReturnsAsync(module);
        _dealerModuleRepositoryMock.Setup(r => r.IsSubscribedAsync(dealerId, moduleId)).ReturnsAsync(false);
        _dealerModuleRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DealerModule>()))
            .ReturnsAsync((DealerModule dm) => dm);

        var command = new SubscribeModuleCommand(dealerId, moduleId, 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _dealerModuleRepositoryMock.Verify(r => r.AddAsync(It.Is<DealerModule>(
            dm => dm.DealerId == dealerId && dm.ModuleId == moduleId && dm.IsActive)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenDealerNotFound()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();

        _dealerRepositoryMock.Setup(r => r.GetByIdAsync(dealerId)).ReturnsAsync((Dealer?)null);

        var command = new SubscribeModuleCommand(dealerId, moduleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenModuleNotFound()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();

        var dealer = new Dealer { Id = dealerId };
        _dealerRepositoryMock.Setup(r => r.GetByIdAsync(dealerId)).ReturnsAsync(dealer);
        _moduleRepositoryMock.Setup(r => r.GetByIdAsync(moduleId)).ReturnsAsync((Module?)null);

        var command = new SubscribeModuleCommand(dealerId, moduleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenModuleNotActive()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();

        var dealer = new Dealer { Id = dealerId };
        var module = new Module { Id = moduleId, Name = "Inactive Module", IsActive = false };

        _dealerRepositoryMock.Setup(r => r.GetByIdAsync(dealerId)).ReturnsAsync(dealer);
        _moduleRepositoryMock.Setup(r => r.GetByIdAsync(moduleId)).ReturnsAsync(module);

        var command = new SubscribeModuleCommand(dealerId, moduleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAlreadySubscribed()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();

        var dealer = new Dealer { Id = dealerId };
        var module = new Module { Id = moduleId, Name = "CRM Module", IsActive = true };

        _dealerRepositoryMock.Setup(r => r.GetByIdAsync(dealerId)).ReturnsAsync(dealer);
        _moduleRepositoryMock.Setup(r => r.GetByIdAsync(moduleId)).ReturnsAsync(module);
        _dealerModuleRepositoryMock.Setup(r => r.IsSubscribedAsync(dealerId, moduleId)).ReturnsAsync(true);

        var command = new SubscribeModuleCommand(dealerId, moduleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}

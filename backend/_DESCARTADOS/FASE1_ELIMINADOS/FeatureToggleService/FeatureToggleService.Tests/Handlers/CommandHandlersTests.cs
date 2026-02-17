using FeatureToggleService.Application.Commands;
using FeatureToggleService.Application.Handlers;
using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using FeatureToggleService.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using Environment = FeatureToggleService.Domain.Enums.Environment;

namespace FeatureToggleService.Tests.Handlers;

public class CommandHandlersTests
{
    private readonly Mock<IFeatureFlagRepository> _repositoryMock;
    private readonly Mock<IFeatureFlagHistoryRepository> _historyRepositoryMock;

    public CommandHandlersTests()
    {
        _repositoryMock = new Mock<IFeatureFlagRepository>();
        _historyRepositoryMock = new Mock<IFeatureFlagHistoryRepository>();
    }

    [Fact]
    public async Task CreateFeatureFlagHandler_ShouldCreateAndReturnFlag()
    {
        // Arrange
        var command = new CreateFeatureFlagCommand(
            Key: "new-feature",
            Name: "New Feature",
            Description: "A new feature",
            Environment: Environment.Development,
            Tags: new List<string> { "test" },
            CreatedBy: "admin"
        );

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<FeatureFlag>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeatureFlag f, CancellationToken _) => f);

        var handler = new CreateFeatureFlagHandler(_repositoryMock.Object, _historyRepositoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Key.Should().Be("new-feature");
        result.Name.Should().Be("New Feature");
        result.Status.Should().Be(FlagStatus.Draft);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<FeatureFlag>(), It.IsAny<CancellationToken>()), Times.Once);
        _historyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<FeatureFlagHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnableFeatureFlagHandler_ShouldEnableFlag()
    {
        // Arrange
        var flag = new FeatureFlag { Id = Guid.NewGuid(), Key = "test", Name = "Test" };
        var command = new EnableFeatureFlagCommand(flag.Id, "admin");

        _repositoryMock.Setup(r => r.GetByIdAsync(flag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<FeatureFlag>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeatureFlag f, CancellationToken _) => f);

        var handler = new EnableFeatureFlagHandler(_repositoryMock.Object, _historyRepositoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsEnabled.Should().BeTrue();
        result.Status.Should().Be(FlagStatus.Active);
    }

    [Fact]
    public async Task DisableFeatureFlagHandler_ShouldDisableFlag()
    {
        // Arrange
        var flag = new FeatureFlag { Id = Guid.NewGuid(), Key = "test", Name = "Test" };
        flag.Enable("admin");
        var command = new DisableFeatureFlagCommand(flag.Id, "admin");

        _repositoryMock.Setup(r => r.GetByIdAsync(flag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<FeatureFlag>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeatureFlag f, CancellationToken _) => f);

        var handler = new DisableFeatureFlagHandler(_repositoryMock.Object, _historyRepositoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsEnabled.Should().BeFalse();
        result.Status.Should().Be(FlagStatus.Inactive);
    }

    [Fact]
    public async Task TriggerKillSwitchHandler_ShouldTriggerKillSwitch()
    {
        // Arrange
        var flag = new FeatureFlag { Id = Guid.NewGuid(), Key = "test", Name = "Test" };
        flag.Enable("admin");
        var command = new TriggerKillSwitchCommand(flag.Id, "admin", "Emergency shutdown");

        _repositoryMock.Setup(r => r.GetByIdAsync(flag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<FeatureFlag>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeatureFlag f, CancellationToken _) => f);

        var handler = new TriggerKillSwitchHandler(_repositoryMock.Object, _historyRepositoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsEnabled.Should().BeFalse();
        result.KillSwitchTriggered.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteFeatureFlagHandler_ShouldReturnTrueWhenDeleted()
    {
        // Arrange
        var flagId = Guid.NewGuid();
        var flag = new FeatureFlag { Id = flagId, Key = "test", Name = "Test" };
        var command = new DeleteFeatureFlagCommand(flagId);

        _repositoryMock.Setup(r => r.GetByIdAsync(flagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);
        _repositoryMock.Setup(r => r.DeleteAsync(flagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeleteFeatureFlagHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(flagId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetRolloutPercentageHandler_ShouldUpdatePercentage()
    {
        // Arrange
        var flag = new FeatureFlag { Id = Guid.NewGuid(), Key = "test", Name = "Test" };
        var command = new SetRolloutPercentageCommand(flag.Id, 50, "admin");

        _repositoryMock.Setup(r => r.GetByIdAsync(flag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<FeatureFlag>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeatureFlag f, CancellationToken _) => f);

        var handler = new SetRolloutPercentageHandler(_repositoryMock.Object, _historyRepositoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.RolloutPercentage.Should().Be(50);
    }

    [Fact]
    public async Task AddTargetUsersHandler_ShouldAddUsers()
    {
        // Arrange
        var flag = new FeatureFlag { Id = Guid.NewGuid(), Key = "test", Name = "Test" };
        var userIds = new List<string> { "user1", "user2" };
        var command = new AddTargetUsersCommand(flag.Id, userIds, "admin");

        _repositoryMock.Setup(r => r.GetByIdAsync(flag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<FeatureFlag>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeatureFlag f, CancellationToken _) => f);

        var handler = new AddTargetUsersHandler(_repositoryMock.Object, _historyRepositoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TargetUserIds.Should().Contain("user1");
        result.TargetUserIds.Should().Contain("user2");
    }
}

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RoleService.Application.Interfaces;
using RoleService.Application.UseCases.Roles.DeleteRole;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using Xunit;

namespace RoleService.Tests.Handlers.Roles;

public class DeleteRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IAuditServiceClient> _auditClientMock;
    private readonly Mock<IUserContextService> _userContextMock;
    private readonly Mock<ILogger<DeleteRoleCommandHandler>> _loggerMock;
    private readonly DeleteRoleCommandHandler _handler;

    public DeleteRoleCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _auditClientMock = new Mock<IAuditServiceClient>();
        _userContextMock = new Mock<IUserContextService>();
        _loggerMock = new Mock<ILogger<DeleteRoleCommandHandler>>();

        _userContextMock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid());

        _handler = new DeleteRoleCommandHandler(
            _roleRepositoryMock.Object,
            _auditClientMock.Object,
            _userContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingRole_ShouldDeleteRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new Role
        {
            Id = roleId,
            Name = "TestRole",
            IsSystemRole = false,
            UserRoleCount = 0
        };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _roleRepositoryMock.Setup(x => x.GetUserCountForRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _roleRepositoryMock.Setup(x => x.DeleteAsync(roleId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteRoleCommand(roleId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _roleRepositoryMock.Verify(x => x.DeleteAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingRole_ShouldThrowNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var command = new DeleteRoleCommand(roleId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithSystemRole_ShouldThrowBadRequestException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var systemRole = new Role
        {
            Id = roleId,
            Name = "SuperAdmin",
            IsSystemRole = true
        };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(systemRole);

        var command = new DeleteRoleCommand(roleId);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithRoleHavingUsers_ShouldThrowBadRequestException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleWithUsers = new Role
        {
            Id = roleId,
            Name = "UsedRole",
            IsSystemRole = false
        };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleWithUsers);

        _roleRepositoryMock.Setup(x => x.GetUserCountForRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5); // 5 users assigned

        var command = new DeleteRoleCommand(roleId);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }
}

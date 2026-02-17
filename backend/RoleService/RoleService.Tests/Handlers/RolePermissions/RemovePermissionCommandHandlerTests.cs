using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RoleService.Application.Interfaces;
using RoleService.Application.UseCases.RolePermissions.RemovePermission;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using Xunit;

namespace RoleService.Tests.Handlers.RolePermissions;

public class RemovePermissionCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IRolePermissionRepository> _rolePermissionRepositoryMock;
    private readonly Mock<IPermissionCacheService> _cacheServiceMock;
    private readonly Mock<IAuditServiceClient> _auditClientMock;
    private readonly Mock<IUserContextService> _userContextMock;
    private readonly Mock<ILogger<RemovePermissionCommandHandler>> _loggerMock;
    private readonly RemovePermissionCommandHandler _handler;

    public RemovePermissionCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _rolePermissionRepositoryMock = new Mock<IRolePermissionRepository>();
        _cacheServiceMock = new Mock<IPermissionCacheService>();
        _auditClientMock = new Mock<IAuditServiceClient>();
        _userContextMock = new Mock<IUserContextService>();
        _loggerMock = new Mock<ILogger<RemovePermissionCommandHandler>>();

        _userContextMock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid());

        _handler = new RemovePermissionCommandHandler(
            _roleRepositoryMock.Object,
            _permissionRepositoryMock.Object,
            _rolePermissionRepositoryMock.Object,
            _cacheServiceMock.Object,
            _auditClientMock.Object,
            _userContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingAssignment_ShouldRemovePermission()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var role = new Role { Id = roleId, Name = "TestRole", IsSystemRole = false };
        var permission = new Permission { Id = permissionId, Name = "test:read" };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _permissionRepositoryMock.Setup(x => x.GetByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);

        _rolePermissionRepositoryMock.Setup(x => x.ExistsAsync(roleId, permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _rolePermissionRepositoryMock.Setup(x => x.RemoveAsync(roleId, permissionId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RemovePermissionCommand(roleId, permissionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _rolePermissionRepositoryMock.Verify(x => x.RemoveAsync(roleId, permissionId, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.InvalidateRolePermissionsCacheAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingRole_ShouldThrowNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var command = new RemovePermissionCommand(roleId, permissionId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonExistingAssignment_ShouldThrowNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var role = new Role { Id = roleId, Name = "TestRole" };
        var permission = new Permission { Id = permissionId, Name = "test:read" };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _permissionRepositoryMock.Setup(x => x.GetByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);

        _rolePermissionRepositoryMock.Setup(x => x.ExistsAsync(roleId, permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Not assigned

        var command = new RemovePermissionCommand(roleId, permissionId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithSystemRoleCorePermission_ShouldThrowBadRequestException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var systemRole = new Role { Id = roleId, Name = "SuperAdmin", IsSystemRole = true };
        var corePermission = new Permission { Id = permissionId, Name = "admin:all", IsCore = true };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(systemRole);

        _permissionRepositoryMock.Setup(x => x.GetByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(corePermission);

        _rolePermissionRepositoryMock.Setup(x => x.ExistsAsync(roleId, permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new RemovePermissionCommand(roleId, permissionId);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }
}

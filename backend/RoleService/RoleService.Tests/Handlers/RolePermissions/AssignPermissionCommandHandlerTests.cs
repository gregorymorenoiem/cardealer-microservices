using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RoleService.Application.Interfaces;
using RoleService.Application.UseCases.RolePermissions.AssignPermission;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using Xunit;

namespace RoleService.Tests.Handlers.RolePermissions;

public class AssignPermissionCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IRolePermissionRepository> _rolePermissionRepositoryMock;
    private readonly Mock<IPermissionCacheService> _cacheServiceMock;
    private readonly Mock<IAuditServiceClient> _auditClientMock;
    private readonly Mock<IUserContextService> _userContextMock;
    private readonly Mock<ILogger<AssignPermissionCommandHandler>> _loggerMock;
    private readonly AssignPermissionCommandHandler _handler;

    public AssignPermissionCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _rolePermissionRepositoryMock = new Mock<IRolePermissionRepository>();
        _cacheServiceMock = new Mock<IPermissionCacheService>();
        _auditClientMock = new Mock<IAuditServiceClient>();
        _userContextMock = new Mock<IUserContextService>();
        _loggerMock = new Mock<ILogger<AssignPermissionCommandHandler>>();

        _userContextMock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid());

        _handler = new AssignPermissionCommandHandler(
            _roleRepositoryMock.Object,
            _permissionRepositoryMock.Object,
            _rolePermissionRepositoryMock.Object,
            _cacheServiceMock.Object,
            _auditClientMock.Object,
            _userContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRoleAndPermission_ShouldAssignPermission()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var role = new Role { Id = roleId, Name = "TestRole", IsActive = true };
        var permission = new Permission { Id = permissionId, Name = "test:read", IsActive = true };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _permissionRepositoryMock.Setup(x => x.GetByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);

        _rolePermissionRepositoryMock.Setup(x => x.ExistsAsync(roleId, permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _rolePermissionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new AssignPermissionCommand(roleId, permissionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _rolePermissionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>()), Times.Once);
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

        var command = new AssignPermissionCommand(roleId, permissionId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonExistingPermission_ShouldThrowNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var role = new Role { Id = roleId, Name = "TestRole" };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _permissionRepositoryMock.Setup(x => x.GetByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission?)null);

        var command = new AssignPermissionCommand(roleId, permissionId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithExistingAssignment_ShouldThrowConflictException()
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
            .ReturnsAsync(true); // Already exists

        var command = new AssignPermissionCommand(roleId, permissionId);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
    }
}

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RoleService.Application.Interfaces;
using RoleService.Application.UseCases.RolePermissions.CheckPermission;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using Xunit;

namespace RoleService.Tests.Handlers.RolePermissions;

public class CheckPermissionQueryHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IRolePermissionRepository> _rolePermissionRepositoryMock;
    private readonly Mock<IPermissionCacheService> _cacheServiceMock;
    private readonly Mock<ILogger<CheckPermissionQueryHandler>> _loggerMock;
    private readonly CheckPermissionQueryHandler _handler;

    public CheckPermissionQueryHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _rolePermissionRepositoryMock = new Mock<IRolePermissionRepository>();
        _cacheServiceMock = new Mock<IPermissionCacheService>();
        _loggerMock = new Mock<ILogger<CheckPermissionQueryHandler>>();

        _handler = new CheckPermissionQueryHandler(
            _roleRepositoryMock.Object,
            _rolePermissionRepositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRoleAndPermission_ShouldReturnTrue()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionName = "vehicles:read";

        var role = new Role { Id = roleId, Name = "TestRole", IsActive = true };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _cacheServiceMock.Setup(x => x.HasPermissionAsync(roleId, permissionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((bool?)null);

        _rolePermissionRepositoryMock.Setup(x => x.HasPermissionAsync(roleId, permissionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var query = new CheckPermissionQuery(roleId, permissionName);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithMissingPermission_ShouldReturnFalse()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionName = "admin:delete";

        var role = new Role { Id = roleId, Name = "TestRole", IsActive = true };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _cacheServiceMock.Setup(x => x.HasPermissionAsync(roleId, permissionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((bool?)null);

        _rolePermissionRepositoryMock.Setup(x => x.HasPermissionAsync(roleId, permissionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var query = new CheckPermissionQuery(roleId, permissionName);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithCachedPermission_ShouldReturnCachedValue()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionName = "cached:permission";

        var role = new Role { Id = roleId, Name = "TestRole", IsActive = true };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _cacheServiceMock.Setup(x => x.HasPermissionAsync(roleId, permissionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Cached as true

        var query = new CheckPermissionQuery(roleId, permissionName);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeTrue();
        _rolePermissionRepositoryMock.Verify(x => x.HasPermissionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveRole_ShouldReturnFalse()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionName = "vehicles:read";

        var inactiveRole = new Role { Id = roleId, Name = "InactiveRole", IsActive = false };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveRole);

        var query = new CheckPermissionQuery(roleId, permissionName);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithSuperAdminRole_ShouldReturnTrueForAnyPermission()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionName = "any:permission";

        var superAdminRole = new Role 
        { 
            Id = roleId, 
            Name = "SuperAdmin", 
            IsActive = true,
            IsSystemRole = true 
        };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(superAdminRole);

        var query = new CheckPermissionQuery(roleId, permissionName);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCacheResult()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionName = "test:cache";

        var role = new Role { Id = roleId, Name = "TestRole", IsActive = true };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _cacheServiceMock.Setup(x => x.HasPermissionAsync(roleId, permissionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((bool?)null);

        _rolePermissionRepositoryMock.Setup(x => x.HasPermissionAsync(roleId, permissionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var query = new CheckPermissionQuery(roleId, permissionName);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(x => x.SetPermissionCacheAsync(roleId, permissionName, true, It.IsAny<CancellationToken>()), Times.Once);
    }
}

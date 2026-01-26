using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RoleService.Application.Interfaces;
using RoleService.Application.UseCases.Permissions.GetPermissions;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using Xunit;

namespace RoleService.Tests.Handlers.Permissions;

public class GetPermissionsQueryHandlerTests
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IPermissionCacheService> _cacheServiceMock;
    private readonly Mock<ILogger<GetPermissionsQueryHandler>> _loggerMock;
    private readonly GetPermissionsQueryHandler _handler;

    public GetPermissionsQueryHandlerTests()
    {
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _cacheServiceMock = new Mock<IPermissionCacheService>();
        _loggerMock = new Mock<ILogger<GetPermissionsQueryHandler>>();

        _handler = new GetPermissionsQueryHandler(
            _permissionRepositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithPermissions_ShouldReturnAllPermissions()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new Permission { Id = Guid.NewGuid(), Name = "vehicles:read", DisplayName = "Read Vehicles", Category = "Vehicles", IsActive = true },
            new Permission { Id = Guid.NewGuid(), Name = "vehicles:write", DisplayName = "Write Vehicles", Category = "Vehicles", IsActive = true },
            new Permission { Id = Guid.NewGuid(), Name = "users:read", DisplayName = "Read Users", Category = "Users", IsActive = true }
        };

        _cacheServiceMock.Setup(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Permission>?)null);

        _permissionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var query = new GetPermissionsQuery(null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldReturnFilteredPermissions()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new Permission { Id = Guid.NewGuid(), Name = "vehicles:read", Category = "Vehicles", IsActive = true },
            new Permission { Id = Guid.NewGuid(), Name = "vehicles:write", Category = "Vehicles", IsActive = true }
        };

        _cacheServiceMock.Setup(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Permission>?)null);

        _permissionRepositoryMock.Setup(x => x.GetByCategoryAsync("Vehicles", It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var query = new GetPermissionsQuery("Vehicles", null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.All(p => p.Category == "Vehicles").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithCachedData_ShouldReturnCachedPermissions()
    {
        // Arrange
        var cachedPermissions = new List<Permission>
        {
            new Permission { Id = Guid.NewGuid(), Name = "cached:permission", DisplayName = "Cached", IsActive = true }
        };

        _cacheServiceMock.Setup(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPermissions);

        var query = new GetPermissionsQuery(null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        _permissionRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithIsActiveFilter_ShouldReturnActiveOnly()
    {
        // Arrange
        var activePermissions = new List<Permission>
        {
            new Permission { Id = Guid.NewGuid(), Name = "active:perm", IsActive = true }
        };

        _cacheServiceMock.Setup(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Permission>?)null);

        _permissionRepositoryMock.Setup(x => x.GetActivePermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activePermissions);

        var query = new GetPermissionsQuery(null, true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.All(p => p.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNoPermissions_ShouldReturnEmptyList()
    {
        // Arrange
        _cacheServiceMock.Setup(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Permission>?)null);

        _permissionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission>());

        var query = new GetPermissionsQuery(null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

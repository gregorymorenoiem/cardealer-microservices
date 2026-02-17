using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RoleService.Application.DTOs.Permissions;
using RoleService.Application.Interfaces;
using RoleService.Application.UseCases.Permissions.CreatePermission;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using Xunit;

namespace RoleService.Tests.Handlers.Permissions;

public class CreatePermissionCommandHandlerTests
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IPermissionCacheService> _cacheServiceMock;
    private readonly Mock<IAuditServiceClient> _auditClientMock;
    private readonly Mock<IUserContextService> _userContextMock;
    private readonly Mock<ILogger<CreatePermissionCommandHandler>> _loggerMock;
    private readonly CreatePermissionCommandHandler _handler;

    public CreatePermissionCommandHandlerTests()
    {
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _cacheServiceMock = new Mock<IPermissionCacheService>();
        _auditClientMock = new Mock<IAuditServiceClient>();
        _userContextMock = new Mock<IUserContextService>();
        _loggerMock = new Mock<ILogger<CreatePermissionCommandHandler>>();

        _userContextMock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid());

        _handler = new CreatePermissionCommandHandler(
            _permissionRepositoryMock.Object,
            _cacheServiceMock.Object,
            _auditClientMock.Object,
            _userContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreatePermission()
    {
        // Arrange
        var request = new CreatePermissionRequest
        {
            Name = "vehicles:read",
            DisplayName = "Read Vehicles",
            Description = "Allows reading vehicle listings",
            Category = "Vehicles",
            IsActive = true
        };

        _permissionRepositoryMock.Setup(x => x.GetByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission?)null);

        _permissionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreatePermissionCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.DisplayName.Should().Be(request.DisplayName);
        _permissionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.InvalidateCacheAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        var request = new CreatePermissionRequest
        {
            Name = "vehicles:read",
            DisplayName = "Read Vehicles"
        };

        var existingPermission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = request.Name
        };

        _permissionRepositoryMock.Setup(x => x.GetByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPermission);

        var command = new CreatePermissionCommand(request);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithCategory_ShouldSetCategory()
    {
        // Arrange
        var request = new CreatePermissionRequest
        {
            Name = "admin:settings",
            DisplayName = "Admin Settings",
            Category = "Administration"
        };

        Permission? capturedPermission = null;

        _permissionRepositoryMock.Setup(x => x.GetByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission?)null);

        _permissionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
            .Callback<Permission, CancellationToken>((p, ct) => capturedPermission = p)
            .Returns(Task.CompletedTask);

        var command = new CreatePermissionCommand(request);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedPermission.Should().NotBeNull();
        capturedPermission!.Category.Should().Be(request.Category);
    }

    [Fact]
    public async Task Handle_ShouldInvalidateCache()
    {
        // Arrange
        var request = new CreatePermissionRequest
        {
            Name = "test:permission",
            DisplayName = "Test Permission"
        };

        _permissionRepositoryMock.Setup(x => x.GetByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission?)null);

        _permissionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreatePermissionCommand(request);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(x => x.InvalidateCacheAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

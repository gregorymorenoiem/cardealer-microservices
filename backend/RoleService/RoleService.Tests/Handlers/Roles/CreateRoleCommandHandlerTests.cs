using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RoleService.Application.DTOs.Roles;
using RoleService.Application.Interfaces;
using RoleService.Application.UseCases.Roles.CreateRole;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using Xunit;

namespace RoleService.Tests.Handlers.Roles;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IRolePermissionRepository> _rolePermissionRepositoryMock;
    private readonly Mock<IAuditServiceClient> _auditClientMock;
    private readonly Mock<INotificationServiceClient> _notificationClientMock;
    private readonly Mock<IUserContextService> _userContextMock;
    private readonly Mock<ILogger<CreateRoleCommandHandler>> _loggerMock;
    private readonly CreateRoleCommandHandler _handler;

    public CreateRoleCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _rolePermissionRepositoryMock = new Mock<IRolePermissionRepository>();
        _auditClientMock = new Mock<IAuditServiceClient>();
        _notificationClientMock = new Mock<INotificationServiceClient>();
        _userContextMock = new Mock<IUserContextService>();
        _loggerMock = new Mock<ILogger<CreateRoleCommandHandler>>();

        _userContextMock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid());

        _handler = new CreateRoleCommandHandler(
            _roleRepositoryMock.Object,
            _permissionRepositoryMock.Object,
            _rolePermissionRepositoryMock.Object,
            _auditClientMock.Object,
            _notificationClientMock.Object,
            _userContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateRole()
    {
        // Arrange
        var request = new CreateRoleRequest
        {
            Name = "TestRole",
            DisplayName = "Test Role",
            Description = "A test role",
            IsActive = true,
            PermissionIds = new List<Guid>()
        };
        var command = new CreateRoleCommand(request);

        _roleRepositoryMock.Setup(x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        _roleRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Name.Should().Be("TestRole");
        _roleRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        var request = new CreateRoleRequest
        {
            Name = "ExistingRole",
            DisplayName = "Existing Role",
            IsActive = true
        };
        var command = new CreateRoleCommand(request);

        _roleRepositoryMock.Setup(x => x.GetByNameAsync("ExistingRole", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = Guid.NewGuid(), Name = "ExistingRole" });

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInvalidPermissionId_ShouldThrowBadRequestException()
    {
        // Arrange
        var invalidPermissionId = Guid.NewGuid();
        var request = new CreateRoleRequest
        {
            Name = "NewRole",
            DisplayName = "New Role",
            IsActive = true,
            PermissionIds = new List<Guid> { invalidPermissionId }
        };
        var command = new CreateRoleCommand(request);

        _roleRepositoryMock.Setup(x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        _permissionRepositoryMock.Setup(x => x.GetByIdAsync(invalidPermissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithValidPermissions_ShouldAssignPermissionsToRole()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var request = new CreateRoleRequest
        {
            Name = "RoleWithPermissions",
            DisplayName = "Role With Permissions",
            IsActive = true,
            PermissionIds = new List<Guid> { permissionId }
        };
        var command = new CreateRoleCommand(request);

        _roleRepositoryMock.Setup(x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        _permissionRepositoryMock.Setup(x => x.GetByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Permission { Id = permissionId, Name = "test:read" });

        _roleRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _rolePermissionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _rolePermissionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

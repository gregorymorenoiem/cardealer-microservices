using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RoleService.Application.DTOs.Roles;
using RoleService.Application.Interfaces;
using RoleService.Application.UseCases.Roles.UpdateRole;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using Xunit;

namespace RoleService.Tests.Handlers.Roles;

public class UpdateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IRolePermissionRepository> _rolePermissionRepositoryMock;
    private readonly Mock<IAuditServiceClient> _auditClientMock;
    private readonly Mock<IUserContextService> _userContextMock;
    private readonly Mock<ILogger<UpdateRoleCommandHandler>> _loggerMock;
    private readonly UpdateRoleCommandHandler _handler;

    public UpdateRoleCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _rolePermissionRepositoryMock = new Mock<IRolePermissionRepository>();
        _auditClientMock = new Mock<IAuditServiceClient>();
        _userContextMock = new Mock<IUserContextService>();
        _loggerMock = new Mock<ILogger<UpdateRoleCommandHandler>>();

        _userContextMock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid());

        _handler = new UpdateRoleCommandHandler(
            _roleRepositoryMock.Object,
            _permissionRepositoryMock.Object,
            _rolePermissionRepositoryMock.Object,
            _auditClientMock.Object,
            _userContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingRole_ShouldUpdateRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var existingRole = new Role
        {
            Id = roleId,
            Name = "TestRole",
            DisplayName = "Test Role",
            IsActive = true,
            IsSystemRole = false
        };

        var request = new UpdateRoleRequest
        {
            DisplayName = "Updated Test Role",
            Description = "Updated description",
            IsActive = true
        };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRole);

        _roleRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateRoleCommand(roleId, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _roleRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingRole_ShouldThrowNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var request = new UpdateRoleRequest { DisplayName = "Test" };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var command = new UpdateRoleCommand(roleId, request);

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

        var request = new UpdateRoleRequest { DisplayName = "Hacked Admin" };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(systemRole);

        var command = new UpdateRoleCommand(roleId, request);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotChangeRoleName()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var originalName = "OriginalName";
        var existingRole = new Role
        {
            Id = roleId,
            Name = originalName,
            DisplayName = "Original Display",
            IsSystemRole = false
        };

        var request = new UpdateRoleRequest
        {
            DisplayName = "New Display Name"
        };

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRole);

        _roleRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .Callback<Role, CancellationToken>((r, ct) =>
            {
                // Verify name hasn't changed
                r.Name.Should().Be(originalName);
            })
            .Returns(Task.CompletedTask);

        var command = new UpdateRoleCommand(roleId, request);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Role>(r => r.Name == originalName), It.IsAny<CancellationToken>()), Times.Once);
    }
}

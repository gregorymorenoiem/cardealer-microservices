using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RoleService.Application.DTOs.Roles;
using RoleService.Application.UseCases.Roles.GetRole;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using Xunit;

namespace RoleService.Tests.Handlers.Roles;

public class GetRoleByIdQueryHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<ILogger<GetRoleByIdQueryHandler>> _loggerMock;
    private readonly GetRoleByIdQueryHandler _handler;

    public GetRoleByIdQueryHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _loggerMock = new Mock<ILogger<GetRoleByIdQueryHandler>>();

        _handler = new GetRoleByIdQueryHandler(
            _roleRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingRole_ShouldReturnRoleDetails()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new Role
        {
            Id = roleId,
            Name = "TestRole",
            DisplayName = "Test Role",
            Description = "A test role",
            IsActive = true,
            RolePermissions = new List<RolePermission>()
        };

        _roleRepositoryMock.Setup(x => x.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var query = new GetRoleByIdQuery(roleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(roleId);
        result.Name.Should().Be("TestRole");
    }

    [Fact]
    public async Task Handle_WithNonExistingRole_ShouldReturnNull()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        _roleRepositoryMock.Setup(x => x.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var query = new GetRoleByIdQuery(roleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithRoleHavingPermissions_ShouldIncludePermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var role = new Role
        {
            Id = roleId,
            Name = "AdminRole",
            DisplayName = "Admin Role",
            IsActive = true,
            RolePermissions = new List<RolePermission>
            {
                new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    Permission = new Permission
                    {
                        Id = permissionId,
                        Name = "users:read",
                        Resource = "users",
                        Action = "read"
                    }
                }
            }
        };

        _roleRepositoryMock.Setup(x => x.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var query = new GetRoleByIdQuery(roleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Permissions.Should().HaveCount(1);
        result.Permissions.First().Name.Should().Be("users:read");
    }
}

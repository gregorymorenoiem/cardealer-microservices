using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RoleService.Application.DTOs.Roles;
using RoleService.Application.Interfaces;
using RoleService.Application.UseCases.Roles.CreateRole;
using RoleService.Application.UseCases.Roles.UpdateRole;
using RoleService.Application.UseCases.RolePermissions.AssignPermission;
using RoleService.Domain.Entities;
using RoleService.Domain.Enums;
using RoleService.Domain.Interfaces;
using RoleService.Infrastructure.Services;
using System.Security.Claims;

namespace RoleService.Tests.Integration;

public class JwtClaimsIntegrationTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IRolePermissionRepository> _rolePermissionRepositoryMock;
    private readonly Mock<IAuditServiceClient> _auditClientMock;
    private readonly Mock<INotificationServiceClient> _notificationClientMock;
    private readonly IUserContextService _userContextService;

    public JwtClaimsIntegrationTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _rolePermissionRepositoryMock = new Mock<IRolePermissionRepository>();
        _auditClientMock = new Mock<IAuditServiceClient>();
        _notificationClientMock = new Mock<INotificationServiceClient>();

        // Setup HttpContextAccessor with authenticated user
        var services = new ServiceCollection();
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = CreateHttpContextWithJwt("test-user-id-123", "test.user", "test@example.com", new[] { "Admin", "User" })
        };
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
        services.AddScoped<IUserContextService, UserContextService>();

        var serviceProvider = services.BuildServiceProvider();
        _userContextService = serviceProvider.GetRequiredService<IUserContextService>();
    }

    private HttpContext CreateHttpContextWithJwt(string userId, string userName, string email, string[] roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Email, email)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, "TestJwtBearer");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        return httpContext;
    }

    [Fact]
    public async Task CreateRole_WithJwtToken_SetsCreatedByCorrectly()
    {
        // Arrange
        var request = new CreateRoleRequest(
            Name: "TestRole",
            Description: "Test Description",
            Priority: 1,
            IsSystemRole: false
        );
        var command = new CreateRoleCommand(request);

        _roleRepositoryMock
            .Setup(x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        _roleRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditClientMock
            .Setup(x => x.LogRoleCreatedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _notificationClientMock
            .Setup(x => x.SendRoleCreatedNotificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateRoleCommandHandler(
            _roleRepositoryMock.Object,
            _auditClientMock.Object,
            _notificationClientMock.Object,
            _userContextService
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestRole", result.Name);

        _roleRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Role>(r => r.CreatedBy == "test-user-id-123" && r.Name == "TestRole"),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        _auditClientMock.Verify(x => x.LogRoleCreatedAsync(
            It.IsAny<Guid>(),
            "TestRole",
            "test-user-id-123"
        ), Times.Once);
    }

    [Fact]
    public async Task UpdateRole_WithJwtToken_SetsUpdatedByCorrectly()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var existingRole = new Role
        {
            Id = roleId,
            Name = "OriginalName",
            Description = "Original Description",
            Priority = 1,
            IsActive = true,
            IsSystemRole = false,
            CreatedBy = "original-creator",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var request = new UpdateRoleRequest(
            Name: "UpdatedName",
            Description: "Updated Description",
            Priority: 2,
            IsActive: true
        );
        var command = new UpdateRoleCommand(roleId, request);

        _roleRepositoryMock
            .Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRole);

        _roleRepositoryMock
            .Setup(x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        _roleRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditClientMock
            .Setup(x => x.LogRoleUpdatedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateRoleCommandHandler(
            _roleRepositoryMock.Object,
            _auditClientMock.Object,
            _userContextService
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UpdatedName", result.Name);
        Assert.Equal("test-user-id-123", existingRole.UpdatedBy);
        Assert.NotNull(existingRole.UpdatedAt);

        _auditClientMock.Verify(x => x.LogRoleUpdatedAsync(
            roleId,
            It.IsAny<string>(),
            "test-user-id-123"
        ), Times.Once);
    }

    [Fact]
    public async Task AssignPermission_WithJwtToken_AuditsCorrectUser()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var command = new AssignPermissionCommand(roleId, permissionId);

        var role = new Role
        {
            Id = roleId,
            Name = "TestRole",
            Description = "Test",
            Priority = 1,
            IsActive = true,
            IsSystemRole = false,
            CreatedBy = "creator",
            CreatedAt = DateTime.UtcNow
        };

        var permission = new Permission
        {
            Id = permissionId,
            Name = "TestPermission",
            Description = "Test Permission",
            Resource = "TestResource",
            Action = PermissionAction.Read,
            IsActive = true
        };

        _roleRepositoryMock
            .Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _permissionRepositoryMock
            .Setup(x => x.GetByIdAsync(permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);

        _rolePermissionRepositoryMock
            .Setup(x => x.HasPermissionAsync(roleId, permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _rolePermissionRepositoryMock
            .Setup(x => x.AssignPermissionToRoleAsync(roleId, permissionId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditClientMock
            .Setup(x => x.LogPermissionAssignedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var handler = new AssignPermissionCommandHandler(
            _roleRepositoryMock.Object,
            _permissionRepositoryMock.Object,
            _rolePermissionRepositoryMock.Object,
            _auditClientMock.Object,
            _userContextService
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);

        _rolePermissionRepositoryMock.Verify(x => x.AssignPermissionToRoleAsync(
            roleId,
            permissionId,
            "test-user-id-123",
            It.IsAny<CancellationToken>()
        ), Times.Once);

        _auditClientMock.Verify(x => x.LogPermissionAssignedAsync(
            roleId,
            permissionId,
            "test-user-id-123"
        ), Times.Once);
    }
}

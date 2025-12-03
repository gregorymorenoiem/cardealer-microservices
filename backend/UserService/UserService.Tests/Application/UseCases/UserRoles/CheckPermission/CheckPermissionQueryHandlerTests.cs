using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.UseCases.UserRoles.CheckPermission;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Tests.Application.UseCases.UserRoles.CheckPermission;

public class CheckPermissionQueryHandlerTests
{
    private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
    private readonly Mock<IRoleServiceClient> _roleServiceClientMock;
    private readonly MemoryCache _cache;
    private readonly CheckUserPermissionQueryHandler _handler;

    public CheckPermissionQueryHandlerTests()
    {
        _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
        _roleServiceClientMock = new Mock<IRoleServiceClient>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _handler = new CheckUserPermissionQueryHandler(
            _userRoleRepositoryMock.Object,
            _roleServiceClientMock.Object,
            _cache);
    }

    [Fact]
    public async Task Handle_UserHasExactPermission_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId, IsActive = true }
        };

        var roles = new List<RoleDetailsDto>
        {
            new RoleDetailsDto
            {
                Id = roleId,
                Name = "Admin",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "Users", Action = "Read" }
                }
            }
        };

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRoles);

        _roleServiceClientMock.Setup(x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(roles);

        var query = new CheckUserPermissionQuery(userId, "Users", "Read");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeTrue();
        result.GrantedByRoles.Should().Contain("Admin");
        result.Message.Should().Contain("Permission granted by roles");
    }

    [Fact]
    public async Task Handle_UserLacksPermission_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId, IsActive = true }
        };

        var roles = new List<RoleDetailsDto>
        {
            new RoleDetailsDto
            {
                Id = roleId,
                Name = "Reader",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "Users", Action = "Read" }
                }
            }
        };

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRoles);

        _roleServiceClientMock.Setup(x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(roles);

        var query = new CheckUserPermissionQuery(userId, "Users", "Delete");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeFalse();
        result.GrantedByRoles.Should().BeEmpty();
        result.Message.Should().Be("Permission denied");
    }

    [Fact]
    public async Task Handle_NoRolesAssigned_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRoles = new List<UserRole>();

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRoles);

        var query = new CheckUserPermissionQuery(userId, "Users", "Read");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeFalse();
        result.GrantedByRoles.Should().BeEmpty();
        result.Message.Should().Be("User has no roles assigned");

        _roleServiceClientMock.Verify(
            x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WildcardAction_GrantsAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId, IsActive = true }
        };

        var roles = new List<RoleDetailsDto>
        {
            new RoleDetailsDto
            {
                Id = roleId,
                Name = "ResourceAdmin",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "Users", Action = "All" }
                }
            }
        };

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRoles);

        _roleServiceClientMock.Setup(x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(roles);

        var query = new CheckUserPermissionQuery(userId, "Users", "Read");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeTrue();
        result.GrantedByRoles.Should().Contain("ResourceAdmin");
    }

    [Fact]
    public async Task Handle_WildcardResource_GrantsAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId, IsActive = true }
        };

        var roles = new List<RoleDetailsDto>
        {
            new RoleDetailsDto
            {
                Id = roleId,
                Name = "SuperAdmin",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "*", Action = "All" }
                }
            }
        };

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRoles);

        _roleServiceClientMock.Setup(x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(roles);

        var query = new CheckUserPermissionQuery(userId, "AnyResource", "AnyAction");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeTrue();
        result.GrantedByRoles.Should().Contain("SuperAdmin");
    }

    [Fact]
    public async Task Handle_PartialWildcard_DeniesAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId, IsActive = true }
        };

        var roles = new List<RoleDetailsDto>
        {
            new RoleDetailsDto
            {
                Id = roleId,
                Name = "LimitedRole",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "Users", Action = "Read" }
                }
            }
        };

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRoles);

        _roleServiceClientMock.Setup(x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(roles);

        var query = new CheckUserPermissionQuery(userId, "Roles", "Read");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CaseInsensitive_GrantsAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId, IsActive = true }
        };

        var roles = new List<RoleDetailsDto>
        {
            new RoleDetailsDto
            {
                Id = roleId,
                Name = "Admin",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "Users", Action = "Read" }
                }
            }
        };

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRoles);

        _roleServiceClientMock.Setup(x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(roles);

        var query = new CheckUserPermissionQuery(userId, "users", "read");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeTrue();
        result.GrantedByRoles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Handle_SecondCall_UsesCachedResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId, IsActive = true }
        };

        var roles = new List<RoleDetailsDto>
        {
            new RoleDetailsDto
            {
                Id = roleId,
                Name = "Admin",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "Users", Action = "Read" }
                }
            }
        };

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRoles);

        _roleServiceClientMock.Setup(x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(roles);

        var query = new CheckUserPermissionQuery(userId, "Users", "Read");

        // Act
        var result1 = await _handler.Handle(query, CancellationToken.None);
        var result2 = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.HasPermission.Should().Be(result2.HasPermission);

        // Repository should be called only once
        _userRoleRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId),
            Times.Once);

        _roleServiceClientMock.Verify(
            x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentUser_UsesSeparateCache()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = userId1, RoleId = roleId, IsActive = true }
        };

        var roles = new List<RoleDetailsDto>
        {
            new RoleDetailsDto
            {
                Id = roleId,
                Name = "Admin",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "Users", Action = "Read" }
                }
            }
        };

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(userRoles);

        _roleServiceClientMock.Setup(x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(roles);

        var query1 = new CheckUserPermissionQuery(userId1, "Users", "Read");
        var query2 = new CheckUserPermissionQuery(userId2, "Users", "Read");

        // Act
        await _handler.Handle(query1, CancellationToken.None);
        await _handler.Handle(query2, CancellationToken.None);

        // Assert - Repository called twice (different users = different cache keys)
        _userRoleRepositoryMock.Verify(
            x => x.GetByUserIdAsync(It.IsAny<Guid>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_DifferentResource_UsesSeparateCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId, IsActive = true }
        };

        var roles = new List<RoleDetailsDto>
        {
            new RoleDetailsDto
            {
                Id = roleId,
                Name = "Admin",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "Users", Action = "Read" },
                    new PermissionDto { Resource = "Roles", Action = "Read" }
                }
            }
        };

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRoles);

        _roleServiceClientMock.Setup(x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(roles);

        var query1 = new CheckUserPermissionQuery(userId, "Users", "Read");
        var query2 = new CheckUserPermissionQuery(userId, "Roles", "Read");

        // Act
        await _handler.Handle(query1, CancellationToken.None);
        await _handler.Handle(query2, CancellationToken.None);

        // Assert - Repository called twice (different resources = different cache keys)
        _userRoleRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_MultipleRoles_AnyGrantsPermission_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId1 = Guid.NewGuid();
        var roleId2 = Guid.NewGuid();

        var userRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId1, IsActive = true },
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId2, IsActive = true }
        };

        var roles = new List<RoleDetailsDto>
        {
            new RoleDetailsDto
            {
                Id = roleId1,
                Name = "Reader",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "Users", Action = "Read" }
                }
            },
            new RoleDetailsDto
            {
                Id = roleId2,
                Name = "Writer",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "Users", Action = "Write" }
                }
            }
        };

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRoles);

        _roleServiceClientMock.Setup(x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(roles);

        var query = new CheckUserPermissionQuery(userId, "Users", "Read");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeTrue();
        result.GrantedByRoles.Should().Contain("Reader");
        result.GrantedByRoles.Should().NotContain("Writer");
    }

    [Fact]
    public async Task Handle_MultipleRoles_NoneGrantPermission_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId1 = Guid.NewGuid();
        var roleId2 = Guid.NewGuid();

        var userRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId1, IsActive = true },
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId2, IsActive = true }
        };

        var roles = new List<RoleDetailsDto>
        {
            new RoleDetailsDto
            {
                Id = roleId1,
                Name = "Reader",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "Users", Action = "Read" }
                }
            },
            new RoleDetailsDto
            {
                Id = roleId2,
                Name = "Writer",
                Permissions = new List<PermissionDto>
                {
                    new PermissionDto { Resource = "Users", Action = "Write" }
                }
            }
        };

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRoles);

        _roleServiceClientMock.Setup(x => x.GetRolesByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(roles);

        var query = new CheckUserPermissionQuery(userId, "Users", "Delete");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeFalse();
        result.GrantedByRoles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoRolesAssigned_CachesNegativeResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRoles = new List<UserRole>();

        _userRoleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRoles);

        var query = new CheckUserPermissionQuery(userId, "Users", "Read");

        // Act
        var result1 = await _handler.Handle(query, CancellationToken.None);
        var result2 = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result1.HasPermission.Should().BeFalse();
        result2.HasPermission.Should().BeFalse();

        // Repository should be called only once (cached on second call)
        _userRoleRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId),
            Times.Once);
    }
}

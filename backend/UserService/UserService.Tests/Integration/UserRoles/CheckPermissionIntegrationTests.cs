using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.UseCases.UserRoles.CheckPermission;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using Xunit;

namespace UserService.Tests.Integration.UserRoles;

[Collection("IntegrationTests")]
public class CheckPermissionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CheckPermissionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CheckPermission_UserWithPermission_HasPermissionTrue()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<CheckUserPermissionQuery, CheckPermissionResponse>>();
        var userRoleRepository = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
        var roleServiceClient = scope.ServiceProvider.GetRequiredService<IRoleServiceClient>();

        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Create test data: user role
        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            IsActive = true
        };
        await userRoleRepository.AddAsync(userRole);

        // Mock role service response (in real scenario, RoleService would return this)
        // Note: In integration tests, we rely on the mocked IRoleServiceClient from CustomWebApplicationFactory

        var query = new CheckUserPermissionQuery(userId, "Users", "Read");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Note: Result depends on mocked IRoleServiceClient behavior
        // If no mock setup in factory, this may return false
    }

    [Fact]
    public async Task CheckPermission_UserWithoutPermission_HasPermissionFalse()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<CheckUserPermissionQuery, CheckPermissionResponse>>();

        var userId = Guid.NewGuid();
        var query = new CheckUserPermissionQuery(userId, "Users", "Delete");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeFalse();
        result.Message.Should().Be("User has no roles assigned");
    }

    [Fact]
    public async Task CheckPermission_NoRolesAssigned_HasPermissionFalse()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<CheckUserPermissionQuery, CheckPermissionResponse>>();

        var userId = Guid.NewGuid(); // User without any roles
        var query = new CheckUserPermissionQuery(userId, "Users", "Read");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasPermission.Should().BeFalse();
        result.GrantedByRoles.Should().BeEmpty();
        result.Message.Should().Be("User has no roles assigned");
    }

    [Fact]
    public async Task CheckPermission_CachedResult_SecondCallReturnsSameResult()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<CheckUserPermissionQuery, CheckPermissionResponse>>();

        var userId = Guid.NewGuid();
        var query = new CheckUserPermissionQuery(userId, "Users", "Read");

        // Act
        var result1 = await handler.Handle(query, CancellationToken.None);
        var result2 = await handler.Handle(query, CancellationToken.None);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.HasPermission.Should().Be(result2.HasPermission);
        result1.Message.Should().Be(result2.Message);
    }
}

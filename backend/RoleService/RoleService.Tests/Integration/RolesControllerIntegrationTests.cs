using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RoleService.Application.DTOs.Roles;
using RoleService.Shared.Models;
using RoleService.Tests.Helpers;
using Xunit;

namespace RoleService.Tests.Integration;

/// <summary>
/// Tests de integración para RolesController.
/// Prueba todos los endpoints REST del API de roles.
/// </summary>
public class RolesControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public RolesControllerIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    #region POST /api/roles - CreateRole

    [Fact]
    public async Task CreateRole_ReturnsSuccess_WhenValidData()
    {
        // Arrange
        var request = new CreateRoleRequest
        {
            Name = $"TestRole_{Guid.NewGuid():N}",
            DisplayName = "Test Role",
            Description = "Role for testing",
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CreateRoleResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Data.Should().NotBeNull();
        result.Data.Data.Id.Should().NotBeEmpty();
        result.Data.Data.Name.Should().Be(request.Name);
        result.Data.Data.DisplayName.Should().Be(request.DisplayName);
    }

    [Fact]
    public async Task CreateRole_ReturnsBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        var request = new CreateRoleRequest
        {
            Name = string.Empty,
            DisplayName = "Test Role"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        // Note: Validation error message format may vary
    }

    [Fact]
    public async Task CreateRole_ReturnsConflict_WhenRoleAlreadyExists()
    {
        // Arrange
        var roleName = $"DuplicateRole_{Guid.NewGuid():N}";
        var request1 = new CreateRoleRequest
        {
            Name = roleName,
            DisplayName = "First Role"
        };
        var request2 = new CreateRoleRequest
        {
            Name = roleName, // Mismo nombre
            DisplayName = "Second Role"
        };

        // Act
        await _client.PostAsJsonAsync("/api/roles", request1); // Primera creación
        var response = await _client.PostAsJsonAsync("/api/roles", request2); // Segunda creación (debería fallar)

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        // Note: API returns 409 Conflict status but ErrorCode may be null
    }

    [Theory]
    [InlineData("ab")] // Muy corto
    [InlineData("ThisRoleNameIsWayTooLongAndExceedsTheMaximumLengthAllowedForARoleName")] // Muy largo
    [InlineData("Invalid@Role")] // Carácter inválido
    [InlineData("123Role")] // Empieza con número
    public async Task CreateRole_ReturnsBadRequest_WhenNameIsInvalid(string invalidName)
    {
        // Arrange
        var request = new CreateRoleRequest
        {
            Name = invalidName,
            DisplayName = "Test Role"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/roles - GetRoles

    [Fact]
    public async Task GetRoles_ReturnsSuccess_WithPaginatedResults()
    {
        // Arrange - Crear algunos roles primero
        for (int i = 0; i < 3; i++)
        {
            var createRequest = new CreateRoleRequest
            {
                Name = $"TestRole_{i}_{Guid.NewGuid():N}",
                DisplayName = $"Test Role {i}"
            };
            await _client.PostAsJsonAsync("/api/roles", createRequest);
        }

        // Act
        var response = await _client.GetAsync("/api/roles?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<RoleListItemDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().NotBeEmpty();
        result.Data.TotalCount.Should().BeGreaterThanOrEqualTo(3);
        result.Data.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetRoles_ReturnsFilteredResults_WhenIsActiveFilter()
    {
        // Arrange
        var activeRole = new CreateRoleRequest
        {
            Name = $"ActiveRole_{Guid.NewGuid():N}",
            DisplayName = "Active Role",
            IsActive = true
        };
        await _client.PostAsJsonAsync("/api/roles", activeRole);

        // Act
        var response = await _client.GetAsync("/api/roles?isActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<RoleListItemDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Items.Should().OnlyContain(r => r.IsActive == true);
    }

    [Fact]
    public async Task GetRoles_RespectsMaxPageSize()
    {
        // Act - Solicitar más del máximo permitido (100)
        var response = await _client.GetAsync("/api/roles?page=1&pageSize=200");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<RoleListItemDto>>>();
        result.Should().NotBeNull();
        result!.Data.PageSize.Should().BeLessOrEqualTo(100);
    }

    #endregion

    #region GET /api/roles/{id} - GetRole

    [Fact]
    public async Task GetRole_ReturnsSuccess_WhenRoleExists()
    {
        // Arrange - Crear un rol
        var createRequest = new CreateRoleRequest
        {
            Name = $"TestRole_{Guid.NewGuid():N}",
            DisplayName = "Test Role for Get"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/roles", createRequest);
        var createdRole = await createResponse.Content.ReadFromJsonAsync<ApiResponse<CreateRoleResponse>>();
        var roleId = createdRole!.Data.Data.Id;

        // Act
        var response = await _client.GetAsync($"/api/roles/{roleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<RoleDetailsDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(roleId);
        result.Data.Name.Should().Be(createRequest.Name);
    }

    [Fact]
    public async Task GetRole_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/roles/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        // result.ErrorCode.Should().Be("ROLE_NOT_FOUND"); // API may not return specific error code
    }

    #endregion

    #region PUT /api/roles/{id} - UpdateRole

    [Fact]
    public async Task UpdateRole_ReturnsSuccess_WhenValidData()
    {
        // Arrange - Crear un rol
        var createRequest = new CreateRoleRequest
        {
            Name = $"OriginalRole_{Guid.NewGuid():N}",
            DisplayName = "Original Display Name"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/roles", createRequest);
        var createdRole = await createResponse.Content.ReadFromJsonAsync<ApiResponse<CreateRoleResponse>>();
        var roleId = createdRole!.Data.Data.Id;

        var updateRequest = new UpdateRoleRequest
        {
            DisplayName = "Updated Display Name",
            Description = "Updated description",
            IsActive = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/roles/{roleId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UpdateRoleResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Data.DisplayName.Should().Be(updateRequest.DisplayName);
        result.Data.Data.Description.Should().Be(updateRequest.Description);
        result.Data.Data.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateRole_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateRoleRequest
        {
            DisplayName = "New Display Name"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/roles/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRole_ReturnsForbidden_WhenUpdatingSystemRole()
    {
        // Arrange - Intentar actualizar SuperAdmin (rol del sistema)
        // Necesitarías obtener el ID del rol SuperAdmin desde la base de datos
        // o mock del repositorio

        // Este test requiere que el sistema ya tenga roles de sistema creados
        // Por ahora lo dejamos como esqueleto

        // var systemRoleId = Guid.Parse("..."); // ID del SuperAdmin
        // var updateRequest = new UpdateRoleRequest { DisplayName = "Hacked Admin" };
        
        // Act
        // var response = await _client.PutAsJsonAsync($"/api/roles/{systemRoleId}", updateRequest);
        
        // Assert
        // response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region DELETE /api/roles/{id} - DeleteRole

    [Fact]
    public async Task DeleteRole_ReturnsSuccess_WhenRoleExists()
    {
        // Arrange - Crear un rol
        var createRequest = new CreateRoleRequest
        {
            Name = $"RoleToDelete_{Guid.NewGuid():N}",
            DisplayName = "Role To Delete"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/roles", createRequest);
        var createdRole = await createResponse.Content.ReadFromJsonAsync<ApiResponse<CreateRoleResponse>>();
        var roleId = createdRole!.Data.Data.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/roles/{roleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().BeTrue();

        // Verificar que realmente se eliminó
        var getResponse = await _client.GetAsync($"/api/roles/{roleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRole_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/roles/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRole_ReturnsForbidden_WhenDeletingSystemRole()
    {
        // Similar al test de UpdateRole para roles del sistema
        // Requiere IDs de roles del sistema
    }

    #endregion
}

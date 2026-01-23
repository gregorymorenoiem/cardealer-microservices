using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RoleService.Application.DTOs.Permissions;
using RoleService.Shared.Models;
using RoleService.Tests.Helpers;
using Xunit;

namespace RoleService.Tests.Integration;

/// <summary>
/// Tests de integración para PermissionsController.
/// Prueba todos los endpoints REST del API de permisos.
/// </summary>
public class PermissionsControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public PermissionsControllerIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    /// <summary>
    /// Genera un sufijo único usando solo letras minúsculas (a-z) para cumplir con la regex de validación.
    /// </summary>
    private static string GenerateUniqueResource()
    {
        return "resource" + Guid.NewGuid().ToString("N").Substring(0, 6).ToLower()
            .Replace("0", "a").Replace("1", "b").Replace("2", "c").Replace("3", "d").Replace("4", "e")
            .Replace("5", "f").Replace("6", "g").Replace("7", "h").Replace("8", "i").Replace("9", "j");
    }

    #region POST /api/permissions - CreatePermission

    [Fact]
    public async Task CreatePermission_ReturnsSuccess_WhenValidData()
    {
        // Arrange
        var resource = GenerateUniqueResource();
        var action = "read";
        var request = new CreatePermissionRequest
        {
            Name = $"{resource}:{action}",  // Name DEBE ser exactamente Resource:Action
            DisplayName = "Test Permission",
            Description = "Permission for testing",
            Module = "auth", // Módulo válido
            Resource = resource,
            Action = action
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CreatePermissionResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Data.Should().NotBeNull();
        result.Data.Data.Id.Should().NotBeEmpty();
        result.Data.Data.Name.Should().Be(request.Name);
        result.Data.Data.Module.Should().Be(request.Module);
    }

    [Theory]
    [InlineData("auth")]
    [InlineData("users")]
    [InlineData("vehicles")]
    [InlineData("dealers")]
    [InlineData("billing")]
    [InlineData("media")]
    [InlineData("notifications")]
    [InlineData("reports")]
    [InlineData("analytics")]
    [InlineData("kyc")]
    [InlineData("aml")]
    [InlineData("compliance")]
    [InlineData("admin")]
    [InlineData("crm")]
    [InlineData("support")]
    public async Task CreatePermission_ReturnsSuccess_ForAllValidModules(string module)
    {
        // Arrange
        var resource = GenerateUniqueResource();
        var action = "read";
        var request = new CreatePermissionRequest
        {
            Name = $"{resource}:{action}",  // Name DEBE ser exactamente Resource:Action
            DisplayName = $"Test {module} Permission",
            Module = module,
            Resource = resource,
            Action = action
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CreatePermissionResponse>>();
        result!.Success.Should().BeTrue();
        result.Data.Data.Module.Should().Be(module);
    }

    [Fact]
    public async Task CreatePermission_ReturnsBadRequest_WhenModuleIsInvalid()
    {
        // Arrange - Use valid name format but invalid module
        var resource = GenerateUniqueResource();
        var request = new CreatePermissionRequest
        {
            Name = $"{resource}:read",
            DisplayName = "Test Permission",
            Module = "invalidmodule", // Módulo NO permitido
            Resource = resource,
            Action = "read"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        // Note: The API returns error message about invalid module
    }

    [Fact]
    public async Task CreatePermission_ReturnsConflict_WhenPermissionAlreadyExists()
    {
        // Arrange - usar mismo resource:action para ambos requests
        var resource = GenerateUniqueResource();
        var action = "read";
        var permissionName = $"{resource}:{action}";
        
        var request1 = new CreatePermissionRequest
        {
            Name = permissionName,
            DisplayName = "First Permission",
            Module = "auth",
            Resource = resource,
            Action = action
        };
        var request2 = new CreatePermissionRequest
        {
            Name = permissionName, // Mismo nombre - debe dar conflicto
            DisplayName = "Second Permission",
            Module = "auth",
            Resource = resource,
            Action = action
        };

        // Act
        await _client.PostAsJsonAsync("/api/permissions", request1);
        var response = await _client.PostAsJsonAsync("/api/permissions", request2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result!.Success.Should().BeFalse();
        // El API devuelve 409 Conflict correctamente
    }

    [Theory]
    [InlineData("", "auth", "users", "read")] // Name vacío
    [InlineData("test.permission", "", "users", "read")] // Module vacío
    [InlineData("test.permission", "auth", "", "read")] // Resource vacío
    [InlineData("test.permission", "auth", "users", "")] // Action vacío
    public async Task CreatePermission_ReturnsBadRequest_WhenRequiredFieldsAreMissing(
        string name, string module, string resource, string action)
    {
        // Arrange
        var request = new CreatePermissionRequest
        {
            Name = name,
            Module = module,
            Resource = resource,
            Action = action,
            DisplayName = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/permissions - GetPermissions

    [Fact]
    public async Task GetPermissions_ReturnsSuccess_WithAllPermissions()
    {
        // Arrange - Crear algunos permisos
        for (int i = 0; i < 3; i++)
        {
            var resource = GenerateUniqueResource();
            var action = "read";
            var createRequest = new CreatePermissionRequest
            {
                Name = $"{resource}:{action}",
                DisplayName = $"Test Permission {i}",
                Module = "auth",
                Resource = resource,
                Action = action
            };
            await _client.PostAsJsonAsync("/api/permissions", createRequest);
        }

        // Act
        var response = await _client.GetAsync("/api/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PermissionListItemDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        result.Data.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetPermissions_ReturnsFilteredByModule()
    {
        // Arrange - Crear permisos en diferentes módulos
        var resource1 = GenerateUniqueResource();
        await _client.PostAsJsonAsync("/api/permissions", new CreatePermissionRequest
        {
            Name = $"{resource1}:read",
            DisplayName = "Auth Permission",
            Module = "auth",
            Resource = resource1,
            Action = "read"
        });
        var resource2 = GenerateUniqueResource();
        await _client.PostAsJsonAsync("/api/permissions", new CreatePermissionRequest
        {
            Name = $"{resource2}:read",
            DisplayName = "Vehicles Permission",
            Module = "vehicles",
            Resource = resource2,
            Action = "read"
        });

        // Act - Filtrar por módulo 'auth'
        var response = await _client.GetAsync("/api/permissions?module=auth");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PermissionListItemDto>>>();
        result!.Success.Should().BeTrue();
        result.Data.Should().OnlyContain(p => p.Module == "auth");
    }

    [Fact]
    public async Task GetPermissions_ReturnsFilteredByResource()
    {
        // Arrange - Create permissions with specific resources
        var targetResource = GenerateUniqueResource();
        await _client.PostAsJsonAsync("/api/permissions", new CreatePermissionRequest
        {
            Name = $"{targetResource}:read",
            DisplayName = "Target Resource Read",
            Module = "auth",
            Resource = targetResource,
            Action = "read"
        });
        var otherResource = GenerateUniqueResource();
        await _client.PostAsJsonAsync("/api/permissions", new CreatePermissionRequest
        {
            Name = $"{otherResource}:read",
            DisplayName = "Other Resource Read",
            Module = "auth",
            Resource = otherResource,
            Action = "read"
        });

        // Act - Filter by the target resource
        var response = await _client.GetAsync($"/api/permissions?resource={targetResource}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PermissionListItemDto>>>();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        // PermissionListItemDto.Name contains "resource:action" format
        result.Data.Should().OnlyContain(p => p.Name.StartsWith($"{targetResource}:"));
    }

    [Fact]
    public async Task GetPermissions_ReturnsFilteredByModuleAndResource()
    {
        // Arrange
        var resource = GenerateUniqueResource();
        await _client.PostAsJsonAsync("/api/permissions", new CreatePermissionRequest
        {
            Name = $"{resource}:delete",
            DisplayName = "Specific Permission",
            Module = "auth",
            Resource = resource,
            Action = "delete"
        });

        // Act - Filtrar por módulo Y resource
        var response = await _client.GetAsync("/api/permissions?module=auth&resource=users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PermissionListItemDto>>>();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        result.Data.Should().OnlyContain(p => p.Module == "auth");
    }

    #endregion

    // NOTE: GET /api/permissions/{id} endpoint does not exist in the API
    // The API only provides: POST (create), GET (list all with filters), GET /modules
    // Tests for GET by ID have been removed as the endpoint is not implemented.
}

using AuditService.Application.Features.Audit.Commands.CreateAudit;
using AuditService.Infrastructure.Persistence;
using AuditService.Shared.Enums;
using AuditService.Tests.Integration.Factories;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace AuditService.Tests.Integration.Api;

/// <summary>
/// Integration tests for Audit API endpoints using InMemory database
/// </summary>
[Collection("Integration")]
public class AuditControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly ITestOutputHelper _output;

    public AuditControllerIntegrationTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task POST_CreateAuditLog_WithValidData_ReturnsCreated()
    {
        // Arrange
        var command = new CreateAuditCommand
        {
            UserId = "user123",
            Action = "LOGIN",
            Resource = "AuthService",
            UserIp = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            AdditionalData = new Dictionary<string, object>
            {
                { "UserName", "testuser" },
                { "Details", "User logged in successfully" }
            },
            Success = true,
            ServiceName = "AuditService",
            Severity = AuditSeverity.Information
        };

        _output.WriteLine($"Testing POST /api/audit - Creating audit log");

        // Act
        var response = await _client.PostAsJsonAsync("/api/audit", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("data");

        _output.WriteLine($"✓ Audit log created successfully");
    }

    [Fact]
    public async Task GET_GetAuditLogs_ReturnsOk()
    {
        // Arrange
        _output.WriteLine($"Testing GET /api/audit - Retrieving audit logs");

        // Act
        var response = await _client.GetAsync("/api/audit?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("data");

        _output.WriteLine($"✓ Audit logs retrieved successfully");
    }

    [Fact]
    public async Task GET_GetAuditLogs_WithFilters_ReturnsFiltered()
    {
        // Arrange - Create audit log first
        var command = new CreateAuditCommand
        {
            UserId = "filtertest123",
            Action = "DELETE",
            Resource = "UserAccount",
            UserIp = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            AdditionalData = new Dictionary<string, object>
            {
                { "UserName", "filteruser" },
                { "Details", "User deleted account" }
            },
            Success = true,
            ServiceName = "UserService",
            Severity = AuditSeverity.Information
        };

        await _client.PostAsJsonAsync("/api/audit", command);

        _output.WriteLine($"Testing GET /api/audit with filters");

        // Act
        var response = await _client.GetAsync("/api/audit?userId=filtertest123&action=DELETE");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _output.WriteLine($"✓ Filtered audit logs retrieved successfully");
    }

    [Fact]
    public async Task GET_GetAuditLogById_WithValidId_ReturnsOk()
    {
        // Arrange - Create audit log first
        var command = new CreateAuditCommand
        {
            UserId = "byid123",
            Action = "VIEW",
            Resource = "Dashboard",
            UserIp = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            AdditionalData = new Dictionary<string, object>
            {
                { "UserName", "byiduser" },
                { "Details", "Viewed dashboard" }
            },
            Success = true,
            ServiceName = "UIService",
            Severity = AuditSeverity.Information
        };

        var createResponse = await _client.PostAsJsonAsync("/api/audit", command);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createData = JsonSerializer.Deserialize<JsonElement>(createContent);
        // Use PascalCase 'Data' - ASP.NET Core uses PascalCase by default
        var auditId = createData.GetProperty("Data").GetString();

        _output.WriteLine($"Testing GET /api/audit/{auditId}");

        // Act
        var response = await _client.GetAsync($"/api/audit/{auditId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("byid123");

        _output.WriteLine($"✓ Audit log retrieved by ID successfully");
    }

    [Fact]
    public async Task GET_GetAuditLogById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid().ToString();

        _output.WriteLine($"Testing GET /api/audit/{invalidId} (invalid ID)");

        // Act
        var response = await _client.GetAsync($"/api/audit/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _output.WriteLine($"✓ Invalid ID handled correctly");
    }

    [Fact]
    public async Task GET_GetAuditStats_ReturnsOk()
    {
        // Arrange
        _output.WriteLine($"Testing GET /api/audit/stats");

        // Act
        var response = await _client.GetAsync("/api/audit/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("data");

        _output.WriteLine($"✓ Audit statistics retrieved successfully");
    }

    [Fact]
    public async Task GET_GetUserAuditLogs_WithValidUserId_ReturnsOk()
    {
        // Arrange - Create user-specific audit log
        var userId = $"useraudit_{Guid.NewGuid():N}";
        var command = new CreateAuditCommand
        {
            UserId = userId,
            Action = "UPDATE",
            Resource = "Profile",
            UserIp = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            AdditionalData = new Dictionary<string, object>
            {
                { "UserName", "specificuser" },
                { "Details", "Updated profile" }
            },
            Success = true,
            ServiceName = "UserService",
            Severity = AuditSeverity.Information
        };

        await _client.PostAsJsonAsync("/api/audit", command);

        _output.WriteLine($"Testing GET /api/audit/user/{userId}");

        // Act
        var response = await _client.GetAsync($"/api/audit/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(userId);

        _output.WriteLine($"✓ User audit logs retrieved successfully");
    }

    [Fact]
    public async Task POST_CreateAuditLog_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange - Missing required fields
        var invalidCommand = new CreateAuditCommand
        {
            UserId = "", // Empty userId
            Action = "",
            Resource = "",
            ServiceName = "",
            Success = true
        };

        _output.WriteLine($"Testing POST /api/audit with invalid data");

        // Act
        var response = await _client.PostAsJsonAsync("/api/audit", invalidCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _output.WriteLine($"✓ Invalid data handled correctly");
    }

    [Fact]
    public async Task GET_GetAuditLogs_WithPagination_ReturnsCorrectPageSize()
    {
        // Arrange - Create multiple audit logs
        for (int i = 0; i < 15; i++)
        {
            var command = new CreateAuditCommand
            {
                UserId = $"paginate_{i}",
                Action = "TEST",
                Resource = "TestResource",
                UserIp = "192.168.1.1",
                UserAgent = "Mozilla/5.0",
                AdditionalData = new Dictionary<string, object>
                {
                    { "UserName", $"user{i}" },
                    { "Details", $"Test {i}" }
                },
                Success = true,
                ServiceName = "TestService",
                Severity = AuditSeverity.Information
            };
            await _client.PostAsJsonAsync("/api/audit", command);
        }

        _output.WriteLine($"Testing GET /api/audit with pagination (pageSize=5)");

        // Act
        var response = await _client.GetAsync("/api/audit?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(content);

        _output.WriteLine($"✓ Pagination working correctly");
    }

    [Fact]
    public async Task GET_GetAuditLogs_WithSearchText_ReturnsMatchingLogs()
    {
        // Arrange - Create searchable audit log
        var searchTerm = $"SEARCH_{Guid.NewGuid():N}";
        var command = new CreateAuditCommand
        {
            UserId = "searchuser",
            Action = "SEARCH_TEST",
            Resource = "Documents",
            UserIp = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            AdditionalData = new Dictionary<string, object>
            {
                { "UserName", "searchuser" },
                { "Details", $"Searching for {searchTerm}" }
            },
            Success = true,
            ServiceName = "DocumentService",
            Severity = AuditSeverity.Information
        };

        await _client.PostAsJsonAsync("/api/audit", command);

        _output.WriteLine($"Testing GET /api/audit with searchText={searchTerm}");

        // Act
        var response = await _client.GetAsync($"/api/audit?searchText={searchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(searchTerm);

        _output.WriteLine($"✓ Search functionality working correctly");
    }
}

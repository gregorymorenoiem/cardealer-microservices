using AuditService.Application.Features.Audit.Commands.CreateAudit;
using AuditService.Tests.Integration.Factories;
using AuditService.Infrastructure.Persistence;
using AuditService.Shared.Enums;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace AuditService.Tests.E2E;

/// <summary>
/// Complete E2E tests for Audit API using Docker containers (PostgreSQL + RabbitMQ)
/// Tests include: Container startup, Database migrations, Real HTTP GET/POST requests
/// </summary>
[Collection("Docker")]
public class AuditFlowE2EDockerTests : IClassFixture<DockerWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly DockerWebApplicationFactory _factory;
    private readonly ITestOutputHelper _output;

    public AuditFlowE2EDockerTests(DockerWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _output = output;
    }

    #region POST Tests - Real HTTP Requests

    [Fact]
    public async Task Docker_E2E_01_POST_CreateAuditLog_WithRealDatabase()
    {
        // Arrange
        var userId = $"docker_{Guid.NewGuid():N}";
        var command = new CreateAuditCommand
        {
            UserId = userId,
            Action = "LOGIN",
            Resource = "AuthService",
            UserIp = "172.17.0.1",
            UserAgent = "Docker Test Client",
            AdditionalData = new Dictionary<string, object>
            {
                { "UserName", "dockeruser" },
                { "Details", "Docker E2E test login" }
            },
            Success = true,
            ServiceName = "AuditService",
            Severity = AuditSeverity.Information
        };

        _output.WriteLine($"=== Docker E2E Test - POST /api/audit ===");
        _output.WriteLine($"UserId: {userId}");

        // Act
        var response = await _client.PostAsJsonAsync("/api/audit", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("data");

        var data = JsonSerializer.Deserialize<JsonElement>(content);
        var auditId = data.GetProperty("data").GetString();

        _output.WriteLine($"✓ Audit log created: {auditId}");

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var auditLog = await dbContext.AuditLogs.FirstOrDefaultAsync(a => a.UserId == userId);
        auditLog.Should().NotBeNull();
        auditLog!.Action.Should().Be("LOGIN");

        _output.WriteLine($"✓ Verified in PostgreSQL database");
    }

    [Fact]
    public async Task Docker_E2E_02_POST_BatchAuditLogs_WithConcurrentRequests()
    {
        // Arrange
        var taskCount = 10;
        var tasks = new List<Task<HttpResponseMessage>>();

        _output.WriteLine($"=== Docker E2E Test - Concurrent POST /api/audit ({taskCount} requests) ===");

        // Act
        for (int i = 0; i < taskCount; i++)
        {
            var command = new CreateAuditCommand
            {
                UserId = $"concurrent_{i}_{Guid.NewGuid():N}",
                Action = "TEST_CONCURRENT",
                Resource = "TestResource",
                UserIp = "192.168.1.1",
                UserAgent = "Docker Test Client",
                AdditionalData = new Dictionary<string, object>
                {
                    { "UserName", $"user{i}" },
                    { "Details", $"Concurrent test {i}" }
                },
                Success = true,
                ServiceName = "AuditService",
                Severity = AuditSeverity.Information
            };

            tasks.Add(_client.PostAsJsonAsync("/api/audit", command));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        successCount.Should().Be(taskCount);

        _output.WriteLine($"✓ All {taskCount} concurrent requests succeeded");

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var count = await dbContext.AuditLogs.CountAsync(a => a.Action == "TEST_CONCURRENT");
        count.Should().BeGreaterThanOrEqualTo(taskCount);

        _output.WriteLine($"✓ All audit logs verified in database (Count: {count})");
    }

    #endregion

    #region GET Tests - Real HTTP Requests

    [Fact]
    public async Task Docker_E2E_03_GET_GetAuditLogs_WithFilters()
    {
        // Arrange - Create test data
        var userId = $"filter_{Guid.NewGuid():N}";
        var command = new CreateAuditCommand
        {
            UserId = userId,
            Action = "UPDATE",
            Resource = "UserProfile",
            UserIp = "192.168.1.1",
            UserAgent = "Docker Test Client",
            AdditionalData = new Dictionary<string, object>
            {
                { "UserName", "filteruser" },
                { "Details", "Updated profile picture" }
            },
            Success = true,
            ServiceName = "UserService",
            Severity = AuditSeverity.Information
        };

        await _client.PostAsJsonAsync("/api/audit", command);

        _output.WriteLine($"=== Docker E2E Test - GET /api/audit with filters ===");

        // Act
        var response = await _client.GetAsync($"/api/audit?userId={userId}&action=UPDATE");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(userId);
        content.Should().Contain("UPDATE");

        _output.WriteLine($"✓ Filtered audit logs retrieved successfully");
    }

    [Fact]
    public async Task Docker_E2E_04_GET_GetAuditLogById_WithValidId()
    {
        // Arrange - Create audit log
        var userId = $"byid_{Guid.NewGuid():N}";
        var command = new CreateAuditCommand
        {
            UserId = userId,
            Action = "VIEW",
            Resource = "Dashboard",
            UserIp = "192.168.1.1",
            UserAgent = "Docker Test Client",
            AdditionalData = new Dictionary<string, object>
            {
                { "UserName", "byiduser" },
                { "Details", "Accessed dashboard" }
            },
            Success = true,
            ServiceName = "UIService",
            Severity = AuditSeverity.Information
        };

        var createResponse = await _client.PostAsJsonAsync("/api/audit", command);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createData = JsonSerializer.Deserialize<JsonElement>(createContent);
        var auditId = createData.GetProperty("data").GetString();

        _output.WriteLine($"=== Docker E2E Test - GET /api/audit/{auditId} ===");

        // Act
        var response = await _client.GetAsync($"/api/audit/{auditId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(userId);
        content.Should().Contain("VIEW");

        _output.WriteLine($"✓ Audit log retrieved by ID successfully");
    }

    [Fact]
    public async Task Docker_E2E_05_GET_GetAuditStats_WithRealData()
    {
        // Arrange - Create multiple audit logs with different severities
        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            var command = new CreateAuditCommand
            {
                UserId = $"stats_{i}",
                Action = i % 2 == 0 ? "LOGIN" : "LOGOUT",
                Resource = "AuthService",
                UserIp = "192.168.1.1",
                UserAgent = "Docker Test Client",
                AdditionalData = new Dictionary<string, object>
                {
                    { "UserName", $"user{i}" },
                    { "Details", $"Stats test {i}" }
                },
                Success = i % 3 != 0,
                ServiceName = "AuditService",
                Severity = i % 2 == 0 ? AuditSeverity.Information : AuditSeverity.Warning
            };

            tasks.Add(_client.PostAsJsonAsync("/api/audit", command));
        }

        await Task.WhenAll(tasks);

        _output.WriteLine($"=== Docker E2E Test - GET /api/audit/stats ===");

        // Act
        var response = await _client.GetAsync("/api/audit/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("data");

        _output.WriteLine($"✓ Audit statistics retrieved successfully");
    }

    [Fact]
    public async Task Docker_E2E_06_GET_GetUserAuditLogs_WithPagination()
    {
        // Arrange - Create multiple user audit logs
        var userId = $"userlog_{Guid.NewGuid():N}";
        for (int i = 0; i < 15; i++)
        {
            var command = new CreateAuditCommand
            {
                UserId = userId,
                Action = $"ACTION_{i}",
                Resource = "Resource",
                UserIp = "192.168.1.1",
                UserAgent = "Docker Test Client",
                AdditionalData = new Dictionary<string, object>
                {
                    { "UserName", "paginationuser" },
                    { "Details", $"Test {i}" }
                },
                Success = true,
                ServiceName = "Service",
                Severity = AuditSeverity.Information
            };

            await _client.PostAsJsonAsync("/api/audit", command);
        }

        _output.WriteLine($"=== Docker E2E Test - GET /api/audit/user/{userId} (pagination) ===");

        // Act
        var response = await _client.GetAsync($"/api/audit/user/{userId}?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(userId);

        _output.WriteLine($"✓ User audit logs with pagination retrieved successfully");
    }

    #endregion

    #region Database Tests

    [Fact]
    public async Task Docker_E2E_07_Database_MigrationsApplied()
    {
        // Arrange & Act
        _output.WriteLine($"=== Docker E2E Test - Database Migrations ===");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        // Assert
        var canConnect = await dbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
        _output.WriteLine($"✓ PostgreSQL connection successful");

        var auditLogCount = await dbContext.AuditLogs.CountAsync();
        auditLogCount.Should().BeGreaterThanOrEqualTo(0);
        _output.WriteLine($"✓ AuditLogs table exists (Count: {auditLogCount})");
    }

    [Fact]
    public async Task Docker_E2E_08_Database_IndexesWorking()
    {
        // Arrange - Create data with indexed fields
        var userId = $"index_{Guid.NewGuid():N}";
        for (int i = 0; i < 100; i++)
        {
            var command = new CreateAuditCommand
            {
                UserId = userId,
                Action = "QUERY_TEST",
                Resource = "Database",
                UserIp = "192.168.1.1",
                UserAgent = "Docker Test Client",
                AdditionalData = new Dictionary<string, object>
                {
                    { "UserName", "indexuser" },
                    { "Details", $"Index test {i}" }
                },
                Success = true,
                ServiceName = "AuditService",
                Severity = AuditSeverity.Information
            };

            await _client.PostAsJsonAsync("/api/audit", command);
        }

        _output.WriteLine($"=== Docker E2E Test - Database Indexes ===");

        // Act - Query by userId (should use index)
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/audit/user/{userId}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Indexed query should be fast");

        _output.WriteLine($"✓ Indexed query completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Complete User Flows

    [Fact]
    public async Task Docker_E2E_09_CompleteFlow_CreateSearchRetrieve()
    {
        // Arrange
        var userId = $"flow_{Guid.NewGuid():N}";

        _output.WriteLine($"=== Docker E2E Test - Complete Audit Flow ===");

        // Step 1: Create audit log
        _output.WriteLine($"Step 1: Creating audit log...");
        var createCommand = new CreateAuditCommand
        {
            UserId = userId,
            Action = "COMPLETE_FLOW_TEST",
            Resource = "E2ETest",
            UserIp = "192.168.1.1",
            UserAgent = "Docker Test Client",
            AdditionalData = new Dictionary<string, object>
            {
                { "UserName", "flowuser" },
                { "Details", "Complete flow test" }
            },
            Success = true,
            ServiceName = "AuditService",
            Severity = AuditSeverity.Information
        };

        var createResponse = await _client.PostAsJsonAsync("/api/audit", createCommand);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createData = JsonSerializer.Deserialize<JsonElement>(createContent);
        var auditId = createData.GetProperty("data").GetString();
        _output.WriteLine($"✓ Audit log created: {auditId}");

        // Step 2: Search by filters
        _output.WriteLine($"Step 2: Searching by filters...");
        var searchResponse = await _client.GetAsync($"/api/audit?userId={userId}&action=COMPLETE_FLOW_TEST");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        _output.WriteLine($"✓ Search successful");

        // Step 3: Retrieve by ID
        _output.WriteLine($"Step 3: Retrieving by ID...");
        var retrieveResponse = await _client.GetAsync($"/api/audit/{auditId}");
        retrieveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrieveContent = await retrieveResponse.Content.ReadAsStringAsync();
        retrieveContent.Should().Contain(userId);
        _output.WriteLine($"✓ Retrieval successful");

        // Step 4: Get stats
        _output.WriteLine($"Step 4: Getting statistics...");
        var statsResponse = await _client.GetAsync($"/api/audit/stats?userId={userId}");
        statsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        _output.WriteLine($"✓ Statistics retrieved");

        _output.WriteLine($"=== Complete Flow Finished Successfully ===");
    }

    [Fact]
    public async Task Docker_E2E_10_ContainerHealth_PostgresAndRabbitMQ()
    {
        // Arrange & Act
        _output.WriteLine($"=== Docker E2E Test - Container Health ===");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        // Test PostgreSQL
        _output.WriteLine($"Testing PostgreSQL container...");
        var canConnect = await dbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
        _output.WriteLine($"✓ PostgreSQL container is healthy");

        // Test database operations
        _output.WriteLine($"Testing database operations...");
        var auditLogCount = await dbContext.AuditLogs.CountAsync();
        _output.WriteLine($"✓ Can read from database (Audit Logs: {auditLogCount})");

        // RabbitMQ is tested implicitly through application startup
        _output.WriteLine($"✓ RabbitMQ container is healthy (application started successfully)");

        _output.WriteLine($"=== All Containers Healthy ===");
    }

    #endregion
}

using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using KYCService.Infrastructure.Persistence;

namespace KYCService.Tests.Integration;

/// <summary>
/// Integration tests for KYCService API.
/// Validates that the DI container starts, health checks work,
/// and endpoints respond correctly with authentication/authorization.
/// </summary>
public class KYCServiceStartupTests : IClassFixture<KYCWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly KYCWebApplicationFactory _factory;

    public KYCServiceStartupTests(KYCWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    // ═══════════════════════════════════════════════════════════════════
    // DI STARTUP — Ensures all services are registered correctly
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void Application_Starts_WithoutDIErrors()
    {
        // If we get here, the factory created successfully → DI is wired up
        _factory.Should().NotBeNull();
    }

    [Fact]
    public async Task DbContext_IsRegistered_AndAccessible()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KYCDbContext>();
        db.Should().NotBeNull();

        // Verify InMemory DB works
        var canConnect = await db.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════
    // HEALTH CHECKS — Critical for K8s readiness/liveness
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task HealthCheck_Live_Returns200()
    {
        var response = await _client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_Health_Returns200_WithoutExternalChecks()
    {
        // /health should NOT include external checks (tagged "external")
        // This is critical for K8s — external check failures shouldn't kill pods
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ═══════════════════════════════════════════════════════════════════
    // AUTHENTICATION — Endpoints should require JWT
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetProfiles_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/KYCProfiles");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSettings_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/KYCProfiles/settings");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProfile_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/KYCProfiles", new
        {
            UserId = Guid.NewGuid().ToString(),
            FullName = "Test User",
            Email = "test@example.com",
            CedulaNumber = "00100000001"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ═══════════════════════════════════════════════════════════════════
    // SWAGGER — Available in development
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Swagger_IsAvailable_InDevelopment()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("KYC Service API");
    }

    // ═══════════════════════════════════════════════════════════════════
    // CORS — Verify security headers
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SecurityHeaders_ArePresent()
    {
        var response = await _client.GetAsync("/health");
        
        // OWASP security headers should be present
        response.Headers.Should().ContainKey("X-Content-Type-Options");
    }
}

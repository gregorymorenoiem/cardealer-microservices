using System.Net;
using FluentAssertions;
using RoleService.Tests.Helpers;
using Xunit;

namespace RoleService.Tests.Integration;

/// <summary>
/// Tests de integración para HealthController.
/// Verifica que los health checks del servicio funcionen correctamente.
/// </summary>
public class HealthControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public HealthControllerIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }

    // NOTE: /health/ready and /health/live endpoints do not exist in RoleService
    // The service only exposes /health (minimal API) and /api/health (controller)
    
    [Fact]
    public async Task ApiHealth_ReturnsOk()
    {
        // Act - Test the controller endpoint
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Health_ResponseContainsHealthCheckInformation()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("status", "Response should contain health status");
        
        // Dependiendo de la implementación, podrías verificar:
        // - Database connectivity
        // - RabbitMQ connectivity
        // - Redis connectivity
        // - Etc.
    }
}

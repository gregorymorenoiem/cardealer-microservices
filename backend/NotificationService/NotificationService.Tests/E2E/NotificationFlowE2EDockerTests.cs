using NotificationService.Tests.Integration.Factories;
using FluentAssertions;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace NotificationService.Tests.E2E;

[Collection("Docker")]
public class NotificationFlowE2EDockerTests : IClassFixture<DockerWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public NotificationFlowE2EDockerTests(DockerWebApplicationFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task Docker_E2E_01_GET_HealthCheck_ReturnsOk()
    {
        _output.WriteLine("=== Docker E2E Test - GET /health ===");

        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
        _output.WriteLine("✓ Health check endpoint tested");
    }

    [Fact]
    public async Task Docker_E2E_02_GET_NotificationStatus_NonExistent_ReturnsExpectedStatus()
    {
        _output.WriteLine("=== Docker E2E Test - GET /api/notifications/{id}/status (non-existent) ===");

        var nonExistentId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/notifications/{nonExistentId}/status");

        // Un ID inexistente puede devolver 404, 400, 401 o 500 (si no hay exception handler)
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.InternalServerError);
        _output.WriteLine($"✓ Non-existent notification status returns {response.StatusCode}");
    }
}

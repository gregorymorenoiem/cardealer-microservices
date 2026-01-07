using MediaService.Tests.Integration.Factories;
using FluentAssertions;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace MediaService.Tests.E2E;

[Collection("Docker")]
public class MediaFlowE2EDockerTests : IClassFixture<DockerWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public MediaFlowE2EDockerTests(DockerWebApplicationFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task Docker_E2E_01_GET_HealthCheck_ReturnsOk()
    {
        _output.WriteLine("=== Docker E2E Test - GET /health ===");

        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _output.WriteLine("✓ Health check successful");
    }

    [Fact]
    public async Task Docker_E2E_02_GET_MediaById_NonExistent_ReturnsNotFound()
    {
        _output.WriteLine("=== Docker E2E Test - GET /api/media/{id} (non-existent) ===");

        var nonExistentId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/media/{nonExistentId}");

        // Un ID inexistente debe devolver 404 o 400
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        _output.WriteLine("✓ Non-existent media returns expected status");
    }
}

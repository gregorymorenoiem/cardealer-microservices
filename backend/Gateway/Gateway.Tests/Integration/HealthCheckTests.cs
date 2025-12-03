using System.Net;

namespace Gateway.Tests.Integration;

/// <summary>
/// Integration tests for Gateway health check endpoint
/// </summary>
public class HealthCheckTests : IClassFixture<Infrastructure.GatewayWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(Infrastructure.GatewayWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task HealthCheck_HasCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");
    }

    [Fact]
    public async Task HealthCheck_RespondsQuickly()
    {
        // Arrange
        var startTime = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        var duration = DateTime.UtcNow - startTime;
        duration.Should().BeLessThan(TimeSpan.FromSeconds(2));
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}

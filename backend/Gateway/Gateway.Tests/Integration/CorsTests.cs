using System.Net;

namespace Gateway.Tests.Integration;

/// <summary>
/// Integration tests for Gateway CORS configuration
/// </summary>
public class CorsTests : IClassFixture<Infrastructure.GatewayWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CorsTests(Infrastructure.GatewayWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PreflightRequest_ReturnsOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Request_WithAllowedOrigin_HasCorsHeaders()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:5173");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
    }
}

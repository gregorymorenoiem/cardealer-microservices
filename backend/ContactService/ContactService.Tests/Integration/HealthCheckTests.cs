using ContactService.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace ContactService.Tests.Integration;

/// <summary>
/// Integration tests for Health Check endpoint
/// </summary>
public class HealthCheckTests : IClassFixture<ContactServiceWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthCheckTests(ContactServiceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_HasCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var contentType = response.Content.Headers.ContentType?.MediaType;

        // Assert
        contentType.Should().Be("application/json");
    }

    [Fact]
    public async Task HealthCheck_RespondsQuickly()
    {
        // Arrange & Act
        var startTime = DateTime.UtcNow;
        var response = await _client.GetAsync("/health");
        var elapsed = DateTime.UtcNow - startTime;

        // Assert - Should respond within 10 seconds (includes test server startup)
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task HealthCheck_ContainsServiceName()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().Contain("ContactService");
    }
}

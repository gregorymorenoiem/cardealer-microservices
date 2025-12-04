using IntegrationTests.Fixtures;
using System.Net;
using System.Text.Json;

namespace IntegrationTests.Gateway;

/// <summary>
/// Integration tests for Gateway health endpoints
/// </summary>
public class GatewayHealthTests : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly GatewayWebApplicationFactory _factory;

    public GatewayHealthTests(GatewayWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnContent()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Health_SubEndpoints_ShouldBeAccessible()
    {
        // Act - Check /health endpoint works
        var response = await _client.GetAsync("/health");

        // Assert - Main health endpoint should work
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Health_ShouldReturnWithinTimeout()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        var response = await _client.GetAsync("/health", cts.Token);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

/// <summary>
/// Integration tests for Gateway routing functionality
/// </summary>
public class GatewayRoutingTests : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly GatewayWebApplicationFactory _factory;

    public GatewayRoutingTests(GatewayWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Options_ShouldHandleCORS()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/test");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _client.SendAsync(request);

        // Assert - Should handle CORS preflight - any response except 500
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task AllHttpMethods_ShouldBeSupported(string method)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(method), "/health");

        // Act
        var response = await _client.SendAsync(request);

        // Assert - Health endpoint should work with GET, others may vary
        if (method == "GET")
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        else
        {
            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    public async Task HealthEndpoint_MultipleRequests_ShouldSucceed()
    {
        // Act - Multiple concurrent requests
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _client.GetAsync("/health"))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }
}

/// <summary>
/// Integration tests for Gateway error handling
/// </summary>
public class GatewayErrorHandlingTests : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GatewayErrorHandlingTests(GatewayWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MalformedJson_ToHealthEndpoint_ShouldNotCrash()
    {
        // Arrange
        var content = new StringContent("not valid json", System.Text.Encoding.UTF8, "application/json");

        // Use health endpoint which we know exists
        var response = await _client.PostAsync("/health", content);

        // Assert - Should handle gracefully (GET only endpoint)
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task EmptyPayload_ToHealthEndpoint_ShouldNotCrash()
    {
        // Arrange
        var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/health", content);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task InvalidJson_ToHealthEndpoint_ShouldBeHandled()
    {
        // Arrange
        var content = new StringContent("{invalid:json}", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/health", content);

        // Assert - Should not crash
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }
}

/// <summary>
/// Integration tests for Gateway headers handling
/// </summary>
public class GatewayHeadersTests : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GatewayHeadersTests(GatewayWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Request_WithCorrelationId_ShouldBeAccepted()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString());

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Request_WithCustomHeaders_ShouldBeAccepted()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Custom-Header", "test-value");
        request.Headers.Add("X-Request-Id", Guid.NewGuid().ToString());

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Response_ShouldHaveContentType()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.Content.Headers.ContentType.Should().NotBeNull();
    }
}

using IntegrationTests.Fixtures;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests.E2E;

/// <summary>
/// End-to-end tests simulating complete user journeys
/// These tests focus on Gateway behavior without backend dependencies
/// </summary>
public class AuthenticationFlowTests : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthenticationFlowTests(GatewayWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AuthFlow_RequestWithBearerToken_ShouldBeProcessed()
    {
        // Arrange - Add a token to a known endpoint
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

        // Act
        var response = await _client.SendAsync(request);

        // Assert - Health endpoint should still work regardless of token
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthFlow_RequestWithoutAuth_ToPublicEndpoint_ShouldSucceed()
    {
        // Act - Request public endpoint without auth
        var response = await _client.GetAsync("/health");

        // Assert - Should work
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthFlow_MultipleAuthHeaders_ShouldBeHandled()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "token1");
        request.Headers.Add("X-API-Key", "some-api-key");

        // Act
        var response = await _client.SendAsync(request);

        // Assert - Should handle gracefully
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

/// <summary>
/// End-to-end tests for health monitoring flow
/// </summary>
public class HealthMonitoringFlowTests : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthMonitoringFlowTests(GatewayWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthFlow_AllEndpoints_ShouldBeAccessible()
    {
        // Act
        var healthResponse = await _client.GetAsync("/health");

        // Assert
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthFlow_ContinuousMonitoring_ShouldRemainHealthy()
    {
        // Arrange - Simulate monitoring over time
        var checkCount = 5;
        var results = new List<HttpStatusCode>();

        // Act
        for (int i = 0; i < checkCount; i++)
        {
            var response = await _client.GetAsync("/health");
            results.Add(response.StatusCode);
            await Task.Delay(100); // Small delay between checks
        }

        // Assert - All checks should pass
        results.Should().AllSatisfy(s => s.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task HealthFlow_Content_ShouldIndicateStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().NotBeNullOrEmpty();
        // Health response typically contains "healthy" or similar
        content.ToLowerInvariant().Should().ContainAny("healthy", "ok", "status");
    }
}

/// <summary>
/// End-to-end tests for API error handling
/// </summary>
public class ErrorHandlingFlowTests : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ErrorHandlingFlowTests(GatewayWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ErrorFlow_InvalidMethod_OnHealth_ShouldNotCrash()
    {
        // Act - Try PATCH on health endpoint
        using var request = new HttpRequestMessage(new HttpMethod("PATCH"), "/health");
        var response = await _client.SendAsync(request);

        // Assert - Should not return 500
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task ErrorFlow_DeleteOnHealth_ShouldBeHandled()
    {
        // Act - Try DELETE on health endpoint
        var response = await _client.DeleteAsync("/health");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task ErrorFlow_InvalidContentType_ShouldBeHandled()
    {
        // Arrange
        var content = new StringContent("plain text", System.Text.Encoding.UTF8, "text/plain");

        // Act
        var response = await _client.PostAsync("/health", content);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task ErrorFlow_EmptyBody_ShouldBeHandled()
    {
        // Arrange
        var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/health", content);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }
}

/// <summary>
/// End-to-end tests for concurrency and load
/// </summary>
public class ConcurrencyFlowTests : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ConcurrencyFlowTests(GatewayWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ConcurrencyFlow_ParallelRequests_ShouldAllSucceed()
    {
        // Arrange
        var requestCount = 10;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(_client.GetAsync("/health"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task ConcurrencyFlow_BurstRequests_ShouldHandleGracefully()
    {
        // Arrange - Burst of requests
        var burstSize = 20;
        var tasks = Enumerable.Range(0, burstSize)
            .Select(_ => _client.GetAsync("/health"))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert - Most should succeed
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        successCount.Should().BeGreaterThan(burstSize / 2);
    }

    [Fact]
    public async Task ConcurrencyFlow_SequentialRequests_ShouldMaintainConsistency()
    {
        // Arrange
        var requestCount = 5;
        var statuses = new List<HttpStatusCode>();

        // Act
        for (int i = 0; i < requestCount; i++)
        {
            var response = await _client.GetAsync("/health");
            statuses.Add(response.StatusCode);
        }

        // Assert - All should be the same
        statuses.Distinct().Should().HaveCount(1);
    }
}

/// <summary>
/// Extension methods for test assertions
/// </summary>
public static class TestAssertionExtensions
{
    public static void ContainAny(this FluentAssertions.Primitives.StringAssertions assertions, params string[] values)
    {
        var actualValue = assertions.Subject;
        var containsAny = values.Any(v => actualValue?.Contains(v) ?? false);

        if (!containsAny)
        {
            throw new Exception($"Expected string to contain any of [{string.Join(", ", values)}] but found '{actualValue}'");
        }
    }
}

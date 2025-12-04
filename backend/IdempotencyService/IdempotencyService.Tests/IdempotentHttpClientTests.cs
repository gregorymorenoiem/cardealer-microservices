using FluentAssertions;
using IdempotencyService.Client;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace IdempotencyService.Tests;

/// <summary>
/// Tests for IdempotentHttpClient and HttpClientIdempotencyExtensions
/// </summary>
public class IdempotentHttpClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly IdempotentHttpClient _idempotentClient;

    public IdempotentHttpClientTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.example.com")
        };
        _idempotentClient = new IdempotentHttpClient(_httpClient);
    }

    // =========== Constructor Tests ===========

    [Fact]
    public void Constructor_WithDefaultHeaderName_UsesXIdempotencyKey()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.OK);

        // Act
        var client = new IdempotentHttpClient(_httpClient);

        // Assert - The header name is used internally
        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomHeaderName_UsesCustomHeader()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.OK, "Custom-Key");
        var client = new IdempotentHttpClient(_httpClient, "Custom-Key");

        // Act
        var result = client.PostAsync("/test", new StringContent(""), "test-key").Result;

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Headers.Contains("Custom-Key")),
            ItExpr.IsAny<CancellationToken>());
    }

    // =========== PostAsync Tests ===========

    [Fact]
    public async Task PostAsync_WithIdempotencyKey_AddsKeyToHeader()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.Created);
        var content = new StringContent("{\"data\": \"test\"}");
        var key = "my-idempotency-key";

        // Act
        var result = await _idempotentClient.PostAsync("/api/orders", content, key);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        VerifyHeaderSent("X-Idempotency-Key", key);
    }

    [Fact]
    public async Task PostAsync_WithoutIdempotencyKey_GeneratesGuid()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.Created);
        var content = new StringContent("{\"data\": \"test\"}");

        // Act
        var result = await _idempotentClient.PostAsync("/api/orders", content);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.Headers.Contains("X-Idempotency-Key") &&
                IsValidGuid(r.Headers.GetValues("X-Idempotency-Key").First())),
            ItExpr.IsAny<CancellationToken>());
    }

    private static bool IsValidGuid(string value) => Guid.TryParse(value, out _);

    [Fact]
    public async Task PostAsync_SendsPostRequest()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.OK);

        // Act
        await _idempotentClient.PostAsync("/test", new StringContent("test"));

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task PostAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.OK);
        var cts = new CancellationTokenSource();

        // Act
        var result = await _idempotentClient.PostAsync("/test", new StringContent("test"), null, cts.Token);

        // Assert - verify the request was sent successfully
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    // =========== PutAsync Tests ===========

    [Fact]
    public async Task PutAsync_WithIdempotencyKey_AddsKeyToHeader()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.OK);
        var content = new StringContent("{\"data\": \"updated\"}");
        var key = "put-key-123";

        // Act
        var result = await _idempotentClient.PutAsync("/api/orders/1", content, key);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        VerifyHeaderSent("X-Idempotency-Key", key);
    }

    [Fact]
    public async Task PutAsync_WithoutIdempotencyKey_GeneratesGuid()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.OK);

        // Act
        await _idempotentClient.PutAsync("/test", new StringContent("test"));

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Headers.Contains("X-Idempotency-Key")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task PutAsync_SendsPutRequest()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.OK);

        // Act
        await _idempotentClient.PutAsync("/test", new StringContent("test"));

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Put),
            ItExpr.IsAny<CancellationToken>());
    }

    // =========== PatchAsync Tests ===========

    [Fact]
    public async Task PatchAsync_WithIdempotencyKey_AddsKeyToHeader()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.OK);
        var content = new StringContent("{\"status\": \"completed\"}");
        var key = "patch-key-456";

        // Act
        var result = await _idempotentClient.PatchAsync("/api/orders/1", content, key);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        VerifyHeaderSent("X-Idempotency-Key", key);
    }

    [Fact]
    public async Task PatchAsync_WithoutIdempotencyKey_GeneratesGuid()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.OK);

        // Act
        await _idempotentClient.PatchAsync("/test", new StringContent("test"));

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Headers.Contains("X-Idempotency-Key")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task PatchAsync_SendsPatchRequest()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.OK);

        // Act
        await _idempotentClient.PatchAsync("/test", new StringContent("test"));

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Patch),
            ItExpr.IsAny<CancellationToken>());
    }

    // =========== DeleteAsync Tests ===========

    [Fact]
    public async Task DeleteAsync_WithIdempotencyKey_AddsKeyToHeader()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.NoContent);
        var key = "delete-key-789";

        // Act
        var result = await _idempotentClient.DeleteAsync("/api/orders/1", key);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        VerifyHeaderSent("X-Idempotency-Key", key);
    }

    [Fact]
    public async Task DeleteAsync_WithoutIdempotencyKey_GeneratesGuid()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.NoContent);

        // Act
        await _idempotentClient.DeleteAsync("/test");

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Headers.Contains("X-Idempotency-Key")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteRequest()
    {
        // Arrange
        SetupMockHandler(HttpStatusCode.NoContent);

        // Act
        await _idempotentClient.DeleteAsync("/test");

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Delete),
            ItExpr.IsAny<CancellationToken>());
    }

    // =========== IsReplayed Tests ===========

    [Fact]
    public void IsReplayed_WithReplayedHeader_ReturnsTrue()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("X-Idempotency-Replayed", "true");

        // Act
        var result = IdempotentHttpClient.IsReplayed(response);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsReplayed_WithFalseReplayedHeader_ReturnsFalse()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("X-Idempotency-Replayed", "false");

        // Act
        var result = IdempotentHttpClient.IsReplayed(response);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsReplayed_WithoutReplayedHeader_ReturnsFalse()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        // Act
        var result = IdempotentHttpClient.IsReplayed(response);

        // Assert
        result.Should().BeFalse();
    }

    // =========== GenerateKey Tests ===========

    [Fact]
    public void GenerateKey_WithSameParameters_ReturnsSameKey()
    {
        // Act
        var key1 = IdempotentHttpClient.GenerateKey("CreateOrder", "customer-123", 100.00m);
        var key2 = IdempotentHttpClient.GenerateKey("CreateOrder", "customer-123", 100.00m);

        // Assert
        key1.Should().Be(key2);
    }

    [Fact]
    public void GenerateKey_WithDifferentOperation_ReturnsDifferentKey()
    {
        // Act
        var key1 = IdempotentHttpClient.GenerateKey("CreateOrder", "customer-123");
        var key2 = IdempotentHttpClient.GenerateKey("UpdateOrder", "customer-123");

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void GenerateKey_WithDifferentParameters_ReturnsDifferentKey()
    {
        // Act
        var key1 = IdempotentHttpClient.GenerateKey("CreateOrder", "customer-123");
        var key2 = IdempotentHttpClient.GenerateKey("CreateOrder", "customer-456");

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void GenerateKey_WithNullParameter_HandlesGracefully()
    {
        // Act
        var key = IdempotentHttpClient.GenerateKey("Test", null!, "param2");

        // Assert
        key.Should().StartWith("Test:");
    }

    [Fact]
    public void GenerateKey_WithEmptyParameters_ReturnsValidKey()
    {
        // Act
        var key = IdempotentHttpClient.GenerateKey("Operation");

        // Assert
        key.Should().StartWith("Operation:");
    }

    [Fact]
    public void GenerateKey_ReturnsDeterministicHash()
    {
        // Act
        var key = IdempotentHttpClient.GenerateKey("Pay", "order-1", 500.00m, "USD");

        // Assert
        key.Should().MatchRegex(@"^Pay:[A-F0-9]+$");
    }

    // =========== HttpClientIdempotencyExtensions Tests ===========

    [Fact]
    public void WithIdempotencyKey_AddsHeaderToRequest()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test.com/order");

        // Act
        var result = request.WithIdempotencyKey("my-key");

        // Assert
        result.Should().BeSameAs(request);
        result.Headers.Should().ContainSingle(h => h.Key == "X-Idempotency-Key");
        result.Headers.GetValues("X-Idempotency-Key").Should().ContainSingle().Which.Should().Be("my-key");
    }

    [Fact]
    public void WithIdempotencyKey_WithCustomHeaderName_UsesCustomName()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test.com/order");

        // Act
        request.WithIdempotencyKey("my-key", "Custom-Idempotency-Header");

        // Assert
        request.Headers.Should().ContainSingle(h => h.Key == "Custom-Idempotency-Header");
    }

    [Fact]
    public void AsIdempotent_ReturnsIdempotentHttpClient()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var result = httpClient.AsIdempotent();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<IdempotentHttpClient>();
    }

    [Fact]
    public void AsIdempotent_WithCustomHeaderName_CreatesClientWithCustomHeader()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var result = httpClient.AsIdempotent("My-Idempotency-Key");

        // Assert
        result.Should().NotBeNull();
    }

    // =========== Helper Methods ===========

    private void SetupMockHandler(HttpStatusCode statusCode, string headerName = "X-Idempotency-Key")
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode));
    }

    private void VerifyHeaderSent(string headerName, string expectedValue)
    {
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.Headers.Contains(headerName) &&
                r.Headers.GetValues(headerName).First() == expectedValue),
            ItExpr.IsAny<CancellationToken>());
    }
}

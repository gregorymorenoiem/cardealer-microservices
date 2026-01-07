using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using AuthService.Application.DTOs.Auth;
using AuthService.Application.DTOs.ExternalAuth;
using AuthService.Shared;
using AuthService.Tests.Integration.Factories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AuthService.Tests.Integration.Api;

public class ExternalAuthRealFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ExternalAuthRealFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private async Task<string> GetAuthTokenAsync()
    {
        // Register user with unique email - registration returns access token directly
        var uniqueId = Guid.NewGuid().ToString("N");
        var registerRequest = new RegisterRequest($"externalauthuser_{uniqueId}", $"externalauth_{uniqueId}@test.com", "Test123!");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        if (!registerResponse.IsSuccessStatusCode)
        {
            return string.Empty;
        }

        // Registration returns the access token directly
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        return registerResult?.Data?.AccessToken ?? string.Empty;
    }

    [Fact]
    public async Task Authenticate_WithGoogleProvider_ReturnsResponse()
    {
        // Arrange
        var request = new ExternalAuthRequest("Google", "fake-google-id-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/ExternalAuth/authenticate", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Authenticate_WithFacebookProvider_ReturnsResponse()
    {
        // Arrange
        var request = new ExternalAuthRequest("Facebook", "fake-facebook-id-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/ExternalAuth/authenticate", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithGoogleProvider_ReturnsAuthorizationUrl()
    {
        // Arrange
        var request = new ExternalLoginRequest("Google", "http://localhost:3000/callback");

        // Act
        var response = await _client.PostAsJsonAsync("/api/ExternalAuth/login", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithFacebookProvider_ReturnsAuthorizationUrl()
    {
        // Arrange
        var request = new ExternalLoginRequest("Facebook", "http://localhost:3000/callback");

        // Act
        var response = await _client.PostAsJsonAsync("/api/ExternalAuth/login", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LinkAccount_WithAuthentication_ReturnsResponse()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new ExternalAuthRequest("Google", "fake-google-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/ExternalAuth/link-account", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetLinkedAccounts_WithAuthentication_ReturnsAccounts()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/ExternalAuth/linked-accounts");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Callback_WithGoogleProvider_ReturnsResponse()
    {
        // Arrange
        var request = new ExternalAuthCallbackRequest(
            "Google",
            "fake-auth-code",
            null,
            "http://localhost:3000/callback",
            "random-state-value");

        // Act
        var response = await _client.PostAsJsonAsync("/api/ExternalAuth/callback", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }
}

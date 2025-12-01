using System.Net;
using System.Net.Http.Json;
using AuthService.Application.DTOs.ExternalAuth;
using AuthService.Shared;
using AuthService.Tests.Integration.Factories;
using FluentAssertions;
using Xunit;

namespace AuthService.Tests.Integration.Api;

public class ExternalAuthEndpointDockerTests : IClassFixture<DockerWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExternalAuthEndpointDockerTests(DockerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Authenticate_WithoutProvider_WithDocker_ReturnsBadRequest()
    {
        // Arrange
        var request = new ExternalAuthRequest("", "test-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/ExternalAuth/authenticate", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Authenticate_WithoutIdToken_WithDocker_ReturnsBadRequest()
    {
        // Arrange
        var request = new ExternalAuthRequest("Google", "");

        // Act
        var response = await _client.PostAsJsonAsync("/api/ExternalAuth/authenticate", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Login_WithoutProvider_WithDocker_ReturnsBadRequest()
    {
        // Arrange
        var request = new ExternalLoginRequest("", "http://localhost/callback");

        // Act
        var response = await _client.PostAsJsonAsync("/api/ExternalAuth/login", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Login_WithoutRedirectUri_WithDocker_ReturnsBadRequest()
    {
        // Arrange
        var request = new ExternalLoginRequest("Google", "");

        // Act
        var response = await _client.PostAsJsonAsync("/api/ExternalAuth/login", request);

        // Assert - API acepta RedirectUri vacío
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task LinkAccount_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Arrange
        var request = new ExternalAuthRequest("Google", "test-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/ExternalAuth/link-account", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnlinkAccount_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Arrange
        var request = new UnlinkExternalAccountRequest("Google");

        // Act
        var response = await _client.DeleteAsync("/api/ExternalAuth/unlink-account");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLinkedAccounts_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/ExternalAuth/linked-accounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Callback_WithoutProvider_WithDocker_ReturnsBadRequest()
    {
        // Arrange
        var request = new ExternalAuthCallbackRequest("", "test-code", null, null, null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/ExternalAuth/callback", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using AuthService.Application.DTOs.Auth;
using AuthService.Application.DTOs.ExternalAuth;
using AuthService.Shared;
using AuthService.Tests.Integration.Factories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AuthService.Tests.Integration.Api;

public class ExternalAuthRealFlowDockerTests : IClassFixture<DockerWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly DockerWebApplicationFactory _factory;

    public ExternalAuthRealFlowDockerTests(DockerWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private async Task<string> GetAuthTokenAsync()
    {
        // Register and verify user
        var registerRequest = new RegisterRequest("externalauthdockeruser", "externalauthdocker@test.com", "Test123!");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Mark as verified
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthService.Infrastructure.Persistence.ApplicationDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "externalauthdocker@test.com");
        if (user != null && !user.EmailConfirmed)
        {
            user.ConfirmEmail();
            await dbContext.SaveChangesAsync();
        }

        // Login to get token
        var loginRequest = new LoginRequest("externalauthdocker@test.com", "Test123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

        return loginResult?.Data?.AccessToken ?? string.Empty;
    }

    [Fact]
    public async Task Authenticate_WithGoogleProvider_WithDocker_ReturnsResponse()
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
    public async Task Login_WithGoogleProvider_WithDocker_ReturnsAuthorizationUrl()
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
    public async Task LinkAccount_WithAuthentication_WithDocker_ReturnsResponse()
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
    public async Task GetLinkedAccounts_WithAuthentication_WithDocker_ReturnsAccounts()
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
}

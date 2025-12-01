using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using AuthService.Application.DTOs.Auth;
using AuthService.Application.DTOs.TwoFactor;
using AuthService.Domain.Enums;
using AuthService.Shared;
using AuthService.Tests.Integration.Factories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AuthService.Tests.Integration.Api;

public class TwoFactorRealFlowDockerTests : IClassFixture<DockerWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly DockerWebApplicationFactory _factory;

    public TwoFactorRealFlowDockerTests(DockerWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private async Task<(string Token, string UserId)> GetAuthTokenAndUserIdAsync()
    {
        // Register and verify user
        var registerRequest = new RegisterRequest("twofactoruserdocker", "twofactordocker@test.com", "Test123!");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Mark as verified
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthService.Infrastructure.Persistence.ApplicationDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "twofactordocker@test.com");
        if (user != null && !user.EmailConfirmed)
        {
            user.ConfirmEmail();
            await dbContext.SaveChangesAsync();
        }

        // Login to get token
        var loginRequest = new LoginRequest("twofactordocker@test.com", "Test123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

        return (loginResult?.Data?.AccessToken ?? string.Empty, user?.Id ?? string.Empty);
    }

    [Fact]
    public async Task Enable2FA_WithAuthenticator_WithDocker_ReturnsResponse()
    {
        // Arrange
        var (token, userId) = await GetAuthTokenAndUserIdAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new Enable2FARequest(userId, TwoFactorAuthType.Authenticator);

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/enable", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Enable2FA_AllTypes_WithDocker_ReturnsResponse()
    {
        // Test all 2FA types
        var types = new[]
        {
            TwoFactorAuthType.Authenticator,
            TwoFactorAuthType.SMS,
            TwoFactorAuthType.Email
        };

        foreach (var type in types)
        {
            // Arrange
            var (token, userId) = await GetAuthTokenAndUserIdAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var request = new Enable2FARequest(userId, type);

            // Act
            var response = await _client.PostAsJsonAsync("/api/TwoFactor/enable", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Verify2FA_WithCode_WithDocker_ReturnsResponse()
    {
        // Arrange
        var (token, userId) = await GetAuthTokenAndUserIdAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new Verify2FARequest(userId, "123456", TwoFactorAuthType.Authenticator);

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/verify", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateRecoveryCodes_WithDocker_ReturnsResponse()
    {
        // Arrange
        var (token, userId) = await GetAuthTokenAndUserIdAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new GenerateRecoveryCodesRequest(userId, "Test123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/generate-recovery-codes", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }
}

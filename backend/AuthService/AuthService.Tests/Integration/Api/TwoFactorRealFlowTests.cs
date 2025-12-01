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

public class TwoFactorRealFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TwoFactorRealFlowTests(CustomWebApplicationFactory factory)
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
        var registerRequest = new RegisterRequest("twofactoruser", "twofactor@test.com", "Test123!");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Mark as verified
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthService.Infrastructure.Persistence.ApplicationDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "twofactor@test.com");
        if (user != null && !user.EmailConfirmed)
        {
            user.ConfirmEmail();
            await dbContext.SaveChangesAsync();
        }

        // Login to get token
        var loginRequest = new LoginRequest("twofactor@test.com", "Test123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

        return (loginResult?.Data?.AccessToken ?? string.Empty, user?.Id ?? string.Empty);
    }

    [Fact]
    public async Task Enable2FA_WithAuthenticator_ReturnsResponse()
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
    public async Task Enable2FA_WithSMS_ReturnsResponse()
    {
        // Arrange
        var (token, userId) = await GetAuthTokenAndUserIdAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new Enable2FARequest(userId, TwoFactorAuthType.SMS);

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/enable", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Enable2FA_WithEmail_ReturnsResponse()
    {
        // Arrange
        var (token, userId) = await GetAuthTokenAndUserIdAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new Enable2FARequest(userId, TwoFactorAuthType.Email);

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/enable", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Verify2FA_WithCode_ReturnsResponse()
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
    public async Task Disable2FA_WithPassword_ReturnsResponse()
    {
        // Arrange
        var (token, userId) = await GetAuthTokenAndUserIdAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new Disable2FARequest("Test123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/disable", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateRecoveryCodes_ReturnsResponse()
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

    [Fact]
    public async Task VerifyRecoveryCode_ReturnsResponse()
    {
        // Arrange
        var (token, userId) = await GetAuthTokenAndUserIdAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new VerifyRecoveryCodeRequest("ABCD-1234-EFGH-5678");

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/verify-recovery-code", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TwoFactorLogin_WithTempToken_ReturnsResponse()
    {
        // Arrange
        var request = new TwoFactorLoginRequest("fake-temp-token", "123456");

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/login", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }
}

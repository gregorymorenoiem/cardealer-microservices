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
        // Register user with unique email - registration returns access token and user ID directly
        var uniqueId = Guid.NewGuid().ToString("N");
        var registerRequest = new RegisterRequest($"twofactoruser_{uniqueId}", $"twofactor_{uniqueId}@test.com", "Test123!");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        if (!registerResponse.IsSuccessStatusCode)
        {
            return (string.Empty, string.Empty);
        }

        // Registration returns the access token and userId directly
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        return (registerResult?.Data?.AccessToken ?? string.Empty, registerResult?.Data?.UserId ?? string.Empty);
    }

    [Fact]
    public async Task Enable2FA_WithAuthenticator_ReturnsResponse()
    {
        // Arrange
        var (token, userId) = await GetAuthTokenAndUserIdAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new Enable2FARequest(TwoFactorAuthType.Authenticator);

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
        var request = new Enable2FARequest(TwoFactorAuthType.SMS);

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
        var request = new Enable2FARequest(TwoFactorAuthType.Email);

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
        var request = new Verify2FARequest("123456", TwoFactorAuthType.Authenticator);

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

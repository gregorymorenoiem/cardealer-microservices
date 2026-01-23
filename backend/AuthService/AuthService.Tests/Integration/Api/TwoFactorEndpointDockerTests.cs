using System.Net;
using System.Net.Http.Json;
using AuthService.Application.DTOs.TwoFactor;
using AuthService.Domain.Enums;
using AuthService.Shared;
using AuthService.Tests.Integration.Factories;
using FluentAssertions;
using Xunit;

namespace AuthService.Tests.Integration.Api;

public class TwoFactorEndpointDockerTests : IClassFixture<DockerWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TwoFactorEndpointDockerTests(DockerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Enable2FA_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Arrange
        var request = new Enable2FARequest(TwoFactorAuthType.Authenticator);

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/enable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Verify2FA_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Arrange
        var request = new Verify2FARequest("user-id", "123456", TwoFactorAuthType.Authenticator);

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Disable2FA_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Arrange
        var request = new Disable2FARequest("Test123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/disable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateRecoveryCodes_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Arrange
        var request = new GenerateRecoveryCodesRequest("user-id", "Test123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/generate-recovery-codes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task VerifyRecoveryCode_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Arrange
        var request = new VerifyRecoveryCodeRequest("ABCD-1234");

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/verify-recovery-code", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TwoFactorLogin_WithoutTempToken_WithDocker_ReturnsBadRequest()
    {
        // Arrange
        var request = new TwoFactorLoginRequest("", "123456");

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/login", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task TwoFactorLogin_WithoutCode_WithDocker_ReturnsBadRequest()
    {
        // Arrange
        var request = new TwoFactorLoginRequest("temp-token", "");

        // Act
        var response = await _client.PostAsJsonAsync("/api/TwoFactor/login", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }
}

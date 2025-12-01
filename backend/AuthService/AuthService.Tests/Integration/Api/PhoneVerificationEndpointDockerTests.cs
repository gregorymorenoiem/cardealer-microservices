using System.Net;
using System.Net.Http.Json;
using AuthService.Application.DTOs.PhoneVerification;
using AuthService.Shared;
using AuthService.Tests.Integration.Factories;
using FluentAssertions;
using Xunit;

namespace AuthService.Tests.Integration.Api;

public class PhoneVerificationEndpointDockerTests : IClassFixture<DockerWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PhoneVerificationEndpointDockerTests(DockerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SendVerification_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Arrange
        var request = new SendPhoneVerificationRequest("+1234567890");

        // Act
        var response = await _client.PostAsJsonAsync("/api/PhoneVerification/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Verify_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Arrange
        var request = new VerifyPhoneRequest("+1234567890", "123456");

        // Act
        var response = await _client.PostAsJsonAsync("/api/PhoneVerification/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResendVerification_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Arrange
        var request = new ResendPhoneVerificationRequest("+1234567890");

        // Act
        var response = await _client.PostAsJsonAsync("/api/PhoneVerification/resend", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVerificationStatus_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/PhoneVerification/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePhoneNumber_WithoutAuthentication_WithDocker_ReturnsUnauthorized()
    {
        // Arrange
        var request = new UpdatePhoneNumberRequest("+1234567890");

        // Act
        var response = await _client.PutAsJsonAsync("/api/PhoneVerification/update", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

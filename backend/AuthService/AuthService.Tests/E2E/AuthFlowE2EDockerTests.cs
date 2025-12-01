using AuthService.Application.DTOs.Auth;
using AuthService.Tests.Integration.Factories;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AuthService.Tests.E2E;

/// <summary>
/// End-to-End tests for authentication workflows using Docker containers (PostgreSQL + RabbitMQ)
/// </summary>
public class AuthFlowE2EDockerTests : IClassFixture<DockerWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly DockerWebApplicationFactory _factory;

    public AuthFlowE2EDockerTests(DockerWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task E2E_Docker_RegisterUser_ReturnsSuccess()
    {
        // Arrange
        var username = $"docker_{Guid.NewGuid():N}";
        var email = $"{username}@test.com";
        var password = "Test123!@#";

        // Act
        var request = new RegisterRequest(username, email, password);
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("userId");
    }

    [Fact]
    public async Task E2E_Docker_ForgotPassword_ReturnsSuccess()
    {
        // Arrange
        var email = "docker_forgot@test.com";

        // Act
        var request = new ForgotPasswordRequest(email);
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

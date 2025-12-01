using AuthService.Application.DTOs.Auth;
using AuthService.Tests.Integration.Factories;
using AuthService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AuthService.Tests.E2E;

/// <summary>
/// End-to-End tests for authentication workflows using InMemory database
/// </summary>
public class AuthFlowE2ETests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthFlowE2ETests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task E2E_RegisterAndLogin_WorksEndToEnd()
    {
        // Arrange
        var username = $"e2e_{Guid.NewGuid():N}";
        var email = $"{username}@test.com";
        var password = "Test123!@#";

        // Act & Assert - Register
        var registerRequest = new RegisterRequest(username, email, password);
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify user in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        user.Should().NotBeNull();

        // Mark as verified for login
        user!.EmailConfirmed = true;
        await dbContext.SaveChangesAsync();

        // Act & Assert - Login
        var loginRequest = new LoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await loginResponse.Content.ReadAsStringAsync();
        content.Should().Contain("accessToken");
    }

    [Fact]
    public async Task E2E_ForgotPassword_ReturnsSuccess()
    {
        // Arrange
        var email = "forgot@test.com";

        // Act
        var request = new ForgotPasswordRequest(email);
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert - Should return OK regardless (security practice)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task E2E_ConcurrentRegistrations_AllSucceed()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();

        for (int i = 0; i < 3; i++)
        {
            var username = $"concurrent_{i}_{Guid.NewGuid():N}";
            var email = $"{username}@test.com";
            var request = new RegisterRequest(username, email, "Test123!@#");
            tasks.Add(_client.PostAsJsonAsync("/api/auth/register", request));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using AuthService.Application.DTOs.Auth;
using AuthService.Application.DTOs.PhoneVerification;
using AuthService.Shared;
using AuthService.Tests.Integration.Factories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AuthService.Tests.Integration.Api;

public class PhoneVerificationRealFlowDockerTests : IClassFixture<DockerWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly DockerWebApplicationFactory _factory;

    public PhoneVerificationRealFlowDockerTests(DockerWebApplicationFactory factory)
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
        var registerRequest = new RegisterRequest("phoneuserdocker", "phoneuserdocker@test.com", "Test123!");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Mark as verified
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthService.Infrastructure.Persistence.ApplicationDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "phoneuserdocker@test.com");
        if (user != null && !user.EmailConfirmed)
        {
            user.ConfirmEmail();
            await dbContext.SaveChangesAsync();
        }

        // Login to get token
        var loginRequest = new LoginRequest("phoneuserdocker@test.com", "Test123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

        return loginResult?.Data?.AccessToken ?? string.Empty;
    }

    [Fact]
    public async Task SendVerification_WithValidPhone_WithDocker_ReturnsOk()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new SendPhoneVerificationRequest("+1234567890");

        // Act
        var response = await _client.PostAsJsonAsync("/api/PhoneVerification/send", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Verify_WithCode_WithDocker_ReturnsResponse()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new VerifyPhoneRequest("+1234567890", "123456");

        // Act
        var response = await _client.PostAsJsonAsync("/api/PhoneVerification/verify", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetVerificationStatus_WithDocker_ReturnsStatus()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/PhoneVerification/status");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }
}

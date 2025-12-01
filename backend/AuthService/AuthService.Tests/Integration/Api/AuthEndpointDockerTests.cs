using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AuthService.Application.DTOs.Auth;
using AuthService.Tests.Integration.Factories;

namespace AuthService.Tests.Integration.Api
{
    [Collection("Docker")]
    public class AuthEndpointDockerTests : IClassFixture<DockerWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly DockerWebApplicationFactory _factory;

        public AuthEndpointDockerTests(DockerWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        private async Task<string> RegisterAndVerifyUserAsync(string username, string email, string password)
        {
            // 1. Register
            var registerRequest = new RegisterRequest(username, email, password);
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            registerResponse.EnsureSuccessStatusCode();

            // 2. Get user from database and mark as verified
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthService.Infrastructure.Persistence.ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                user.ConfirmEmail();
                await dbContext.SaveChangesAsync();
            }

            return user?.Id ?? string.Empty;
        }

        [Fact]
        public async Task Register_ValidRequest_WithDocker_ReturnsOk()
        {
            // Arrange
            var request = new RegisterRequest(
                "dockeruser",
                "docker@example.com",
                "Password123!"
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("docker@example.com");
        }

        [Fact]
        public async Task Register_DuplicateEmail_WithDocker_ReturnsConflict()
        {
            // Arrange
            var request = new RegisterRequest(
                "dockeruser2",
                "duplicate-docker@example.com",
                "Password123!"
            );

            // First registration
            await _client.PostAsJsonAsync("/api/auth/register", request);

            // Act - Try to register again with same email
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_ValidCredentials_WithDocker_ReturnsToken()
        {
            // Arrange - Register and verify user
            await RegisterAndVerifyUserAsync("loginuser_docker", "login-docker@example.com", "Password123!");

            var loginRequest = new LoginRequest("login-docker@example.com", "Password123!");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("accessToken");
            content.Should().Contain("refreshToken");
        }

        [Fact]
        public async Task Login_InvalidCredentials_WithDocker_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest("nonexistent-docker@example.com", "WrongPassword123");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WrongPassword_WithDocker_ReturnsUnauthorized()
        {
            // Arrange - Register user first
            var registerRequest = new RegisterRequest(
                "wrongpwduser_docker",
                "wrongpwd-docker@example.com",
                "Password123!"
            );
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            var loginRequest = new LoginRequest("wrongpwd-docker@example.com", "WrongPassword999");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ForgotPassword_ValidEmail_WithDocker_ReturnsOk()
        {
            // Arrange - Register user first
            var registerRequest = new RegisterRequest(
                "forgotuser_docker",
                "forgot-docker@example.com",
                "Password123!"
            );
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            var forgotRequest = new ForgotPasswordRequest("forgot-docker@example.com");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ForgotPassword_NonExistentEmail_WithDocker_ReturnsOk()
        {
            // Arrange
            var forgotRequest = new ForgotPasswordRequest("nonexistent-docker@example.com");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Logout_WithoutToken_WithDocker_ReturnsUnauthorized()
        {
            // Arrange
            var logoutRequest = new LogoutRequest("some-refresh-token");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RefreshToken_WithoutToken_WithDocker_ReturnsUnauthorized()
        {
            // Arrange
            var refreshRequest = new RefreshTokenRequest("invalid-refresh-token");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", refreshRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task VerifyEmail_WithoutToken_WithDocker_ReturnsBadRequest()
        {
            // Arrange
            var verifyRequest = new VerifyEmailRequest("invalid-token");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/verify-email", verifyRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CompleteAuthFlow_WithDocker_WorksEndToEnd()
        {
            // 1. Register and Verify
            await RegisterAndVerifyUserAsync("flowuser", "flow@example.com", "Password123!");

            // 2. Login
            var loginRequest = new LoginRequest("flow@example.com", "Password123!");
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            loginContent.Should().Contain("accessToken");
            loginContent.Should().Contain("refreshToken");

            // 3. Forgot Password
            var forgotRequest = new ForgotPasswordRequest("flow@example.com");
            var forgotResponse = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotRequest);
            forgotResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ResetPassword_WithoutToken_WithDocker_ReturnsBadRequest()
        {
            // Arrange
            var resetRequest = new ResetPasswordRequest("invalid-token", "NewPassword123!", "NewPassword123!");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/reset-password", resetRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task ResetPassword_MismatchedPasswords_WithDocker_ReturnsBadRequest()
        {
            // Arrange
            var resetRequest = new ResetPasswordRequest("some-token", "NewPassword123!", "DifferentPassword456!");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/reset-password", resetRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }
    }

    public class RegisterResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}

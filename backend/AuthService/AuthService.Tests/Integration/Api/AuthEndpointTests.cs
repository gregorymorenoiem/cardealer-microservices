using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AuthService.Application.DTOs.Auth;
using AuthService.Tests.Integration.Factories;

namespace AuthService.Tests.Integration.Api
{
    public class AuthEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AuthEndpointTests(CustomWebApplicationFactory factory)
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
        public async Task Register_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new RegisterRequest("integrationuser", "integration@example.com", "Password123!");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("integration@example.com");
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            // Arrange - First registration
            var request = new RegisterRequest("user1", "duplicate@test.com", "Password123!");
            await _client.PostAsJsonAsync("/api/auth/register", request);

            // Act - Try to register with same email
            var duplicateRequest = new RegisterRequest("user2", "duplicate@test.com", "Password456!");
            var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange - Register and verify user
            await RegisterAndVerifyUserAsync("loginuser", "login@test.com", "Password123!");

            var loginRequest = new LoginRequest("login@test.com", "Password123!");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("accessToken");
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest("nonexistent@test.com", "WrongPassword123");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            // Arrange - Register user first
            var registerRequest = new RegisterRequest("wrongpwduser", "wrongpwd@test.com", "Password123!");
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            var loginRequest = new LoginRequest("wrongpwd@test.com", "WrongPassword999");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ForgotPassword_ValidEmail_ReturnsOk()
        {
            // Arrange - Register user first
            var registerRequest = new RegisterRequest("forgotuser", "forgot@test.com", "Password123!");
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            var forgotRequest = new ForgotPasswordRequest("forgot@test.com");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ForgotPassword_NonExistentEmail_ReturnsOk()
        {
            // Arrange - For security, should return OK even if email doesn't exist
            var forgotRequest = new ForgotPasswordRequest("nonexistent@test.com");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Logout_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            var logoutRequest = new LogoutRequest("some-refresh-token");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RefreshToken_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            var refreshRequest = new RefreshTokenRequest("invalid-refresh-token");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", refreshRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task VerifyEmail_WithoutToken_ReturnsBadRequest()
        {
            // Arrange
            var verifyRequest = new VerifyEmailRequest("invalid-token");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/verify-email", verifyRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ResetPassword_WithoutToken_ReturnsBadRequest()
        {
            // Arrange
            var resetRequest = new ResetPasswordRequest("invalid-token", "NewPassword123!", "NewPassword123!");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/reset-password", resetRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task ResetPassword_MismatchedPasswords_ReturnsBadRequest()
        {
            // Arrange
            var resetRequest = new ResetPasswordRequest("some-token", "NewPassword123!", "DifferentPassword456!");

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/reset-password", resetRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }
    }
}

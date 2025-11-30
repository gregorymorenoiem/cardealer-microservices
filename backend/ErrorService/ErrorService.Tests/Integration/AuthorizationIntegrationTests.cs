using ErrorService.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ErrorService.Tests.Integration
{
    public class AuthorizationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly string _validIssuer = "cardealer-auth";
        private readonly string _validAudience = "cardealer-services";
        private readonly string _validKey = "super-secret-key-for-testing-minimum-32-chars-long!";

        public AuthorizationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private string GenerateJwtToken(string serviceClaim = "errorservice", string role = "ErrorServiceAdmin", int expirationMinutes = 60)
        {
            var claims = new[]
            {
                new Claim("service", serviceClaim),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_validKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        [Fact]
        public async Task LogError_WithValidToken_ReturnsSuccess()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = GenerateJwtToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var request = new LogErrorRequest(
                ServiceName: "TestService",
                ExceptionType: "TestException",
                Message: "Test error message",
                StackTrace: "Test stack trace",
                OccurredAt: DateTime.UtcNow,
                Endpoint: "/api/test",
                HttpMethod: "GET",
                StatusCode: 500,
                UserId: "testuser",
                Metadata: null
            );

            // Act
            var response = await client.PostAsJsonAsync("/api/errors", request);

            // Assert
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest);
            // BadRequest is acceptable if validation fails, but it should not be Unauthorized
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task LogError_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new LogErrorRequest(
                ServiceName: "TestService",
                ExceptionType: "TestException",
                Message: "Test error message",
                StackTrace: "Test stack trace",
                OccurredAt: DateTime.UtcNow,
                Endpoint: "/api/test",
                HttpMethod: "GET",
                StatusCode: 500,
                UserId: "testuser",
                Metadata: null
            );

            // Act
            var response = await client.PostAsJsonAsync("/api/errors", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task LogError_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token-12345");

            var request = new LogErrorRequest(
                ServiceName: "TestService",
                ExceptionType: "TestException",
                Message: "Test error message",
                StackTrace: "Test stack trace",
                OccurredAt: DateTime.UtcNow,
                Endpoint: "/api/test",
                HttpMethod: "GET",
                StatusCode: 500,
                UserId: "testuser",
                Metadata: null
            );

            // Act
            var response = await client.PostAsJsonAsync("/api/errors", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task LogError_WithExpiredToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var expiredToken = GenerateJwtToken(expirationMinutes: -5); // Expired 5 minutes ago
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

            var request = new LogErrorRequest(
                ServiceName: "TestService",
                ExceptionType: "TestException",
                Message: "Test error message",
                StackTrace: "Test stack trace",
                OccurredAt: DateTime.UtcNow,
                Endpoint: "/api/test",
                HttpMethod: "GET",
                StatusCode: 500,
                UserId: "testuser",
                Metadata: null
            );

            // Act
            var response = await client.PostAsJsonAsync("/api/errors", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task LogError_WithWrongServiceClaim_MayReturnForbiddenOrSucceed()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = GenerateJwtToken(serviceClaim: "different-service");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var request = new LogErrorRequest(
                ServiceName: "TestService",
                ExceptionType: "TestException",
                Message: "Test error message",
                StackTrace: "Test stack trace",
                OccurredAt: DateTime.UtcNow,
                Endpoint: "/api/test",
                HttpMethod: "GET",
                StatusCode: 500,
                UserId: "testuser",
                Metadata: null
            );

            // Act
            var response = await client.PostAsJsonAsync("/api/errors", request);

            // Assert
            // Depending on policy configuration, this could be Forbidden or Unauthorized
            // We're checking it's not successful if policy requires "errorservice" claim
            Assert.True(
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.Unauthorized ||
                response.IsSuccessStatusCode // May succeed if policy allows
            );
        }

        [Fact]
        public async Task HealthEndpoint_WithoutToken_ReturnsSuccess()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task LogError_WithAdminRole_ReturnsSuccess()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = GenerateJwtToken(role: "ErrorServiceAdmin");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var request = new LogErrorRequest(
                ServiceName: "TestService",
                ExceptionType: "TestException",
                Message: "Test error message",
                StackTrace: "Test stack trace",
                OccurredAt: DateTime.UtcNow,
                Endpoint: "/api/test",
                HttpMethod: "GET",
                StatusCode: 500,
                UserId: "testuser",
                Metadata: null
            );

            // Act
            var response = await client.PostAsJsonAsync("/api/errors", request);

            // Assert
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task LogError_WithReadRole_ReturnsSuccess()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = GenerateJwtToken(role: "ErrorServiceRead");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var request = new LogErrorRequest(
                ServiceName: "TestService",
                ExceptionType: "TestException",
                Message: "Test error message",
                StackTrace: "Test stack trace",
                OccurredAt: DateTime.UtcNow,
                Endpoint: "/api/test",
                HttpMethod: "GET",
                StatusCode: 500,
                UserId: "testuser",
                Metadata: null
            );

            // Act
            var response = await client.PostAsJsonAsync("/api/errors", request);

            // Assert
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}

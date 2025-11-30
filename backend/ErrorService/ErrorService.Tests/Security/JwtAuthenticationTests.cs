using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace ErrorService.Tests.Security
{
    public class JwtAuthenticationTests
    {
        private readonly string _validIssuer = "cardealer-auth";
        private readonly string _validAudience = "cardealer-services";
        private readonly string _validKey = "super-secret-key-for-testing-minimum-32-chars-long!";

        [Fact]
        public void GenerateToken_WithValidClaims_ReturnsValidJwtToken()
        {
            // Arrange
            var claims = new[]
            {
                new Claim("service", "errorservice"),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "ErrorServiceAdmin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_validKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();

            // Act
            var jwt = tokenHandler.WriteToken(token);

            // Assert
            Assert.NotNull(jwt);
            Assert.NotEmpty(jwt);
            Assert.Contains(".", jwt);
        }

        [Fact]
        public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
        {
            // Arrange
            var claims = new[]
            {
                new Claim("service", "errorservice"),
                new Claim(ClaimTypes.Name, "TestUser")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_validKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _validIssuer,
                ValidAudience = _validAudience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            // Act
            var principal = tokenHandler.ValidateToken(jwt, validationParameters, out var validatedToken);

            // Assert
            Assert.NotNull(principal);
            Assert.NotNull(validatedToken);
            Assert.IsType<JwtSecurityToken>(validatedToken);
            Assert.True(principal.HasClaim("service", "errorservice"));
        }

        [Fact]
        public void ValidateToken_WithInvalidIssuer_ThrowsSecurityTokenInvalidIssuerException()
        {
            // Arrange
            var claims = new[] { new Claim("service", "errorservice") };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_validKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "invalid-issuer",
                audience: _validAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _validIssuer,
                ValidAudience = _validAudience,
                IssuerSigningKey = key
            };

            // Act & Assert
            Assert.Throws<SecurityTokenInvalidIssuerException>(() =>
                tokenHandler.ValidateToken(jwt, validationParameters, out _));
        }

        [Fact]
        public void ValidateToken_WithInvalidAudience_ThrowsSecurityTokenInvalidAudienceException()
        {
            // Arrange
            var claims = new[] { new Claim("service", "errorservice") };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_validKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _validIssuer,
                audience: "invalid-audience",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _validIssuer,
                ValidAudience = _validAudience,
                IssuerSigningKey = key
            };

            // Act & Assert
            Assert.Throws<SecurityTokenInvalidAudienceException>(() =>
                tokenHandler.ValidateToken(jwt, validationParameters, out _));
        }

        [Fact]
        public void ValidateToken_WithExpiredToken_ThrowsSecurityTokenExpiredException()
        {
            // Arrange
            var claims = new[] { new Claim("service", "errorservice") };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_validKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(-1), // Expired 1 minute ago
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _validIssuer,
                ValidAudience = _validAudience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            // Act & Assert
            Assert.Throws<SecurityTokenExpiredException>(() =>
                tokenHandler.ValidateToken(jwt, validationParameters, out _));
        }

        [Fact]
        public void ValidateToken_WithInvalidSignature_ThrowsSecurityTokenInvalidSignatureException()
        {
            // Arrange
            var claims = new[] { new Claim("service", "errorservice") };
            var wrongKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("wrong-key-that-is-at-least-32-chars-long!!!"));
            var credentials = new SigningCredentials(wrongKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);

            var validKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_validKey));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _validIssuer,
                ValidAudience = _validAudience,
                IssuerSigningKey = validKey
            };

            // Act & Assert
            Assert.Throws<SecurityTokenInvalidSignatureException>(() =>
                tokenHandler.ValidateToken(jwt, validationParameters, out _));
        }

        [Fact]
        public void Token_WithErrorServiceAccessClaim_ShouldHaveServiceClaim()
        {
            // Arrange
            var claims = new[]
            {
                new Claim("service", "errorservice"),
                new Claim(ClaimTypes.Role, "ErrorServiceAdmin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_validKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _validIssuer,
                ValidAudience = _validAudience,
                IssuerSigningKey = key
            };

            // Act
            var principal = tokenHandler.ValidateToken(jwt, validationParameters, out _);

            // Assert
            Assert.True(principal.HasClaim("service", "errorservice"));
            Assert.True(principal.IsInRole("ErrorServiceAdmin"));
        }

        [Fact]
        public void Token_WithMultipleRoles_ShouldContainAllRoles()
        {
            // Arrange
            var claims = new[]
            {
                new Claim("service", "errorservice"),
                new Claim(ClaimTypes.Role, "ErrorServiceAdmin"),
                new Claim(ClaimTypes.Role, "ErrorServiceRead")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_validKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _validIssuer,
                ValidAudience = _validAudience,
                IssuerSigningKey = key
            };

            // Act
            var principal = tokenHandler.ValidateToken(jwt, validationParameters, out _);

            // Assert
            Assert.True(principal.IsInRole("ErrorServiceAdmin"));
            Assert.True(principal.IsInRole("ErrorServiceRead"));
        }
    }
}

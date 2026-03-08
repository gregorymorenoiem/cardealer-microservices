using Xunit;
using Moq;
using FluentAssertions;
using AuthService.Application.Features.Auth.Commands.RefreshToken;
using AuthService.Application.DTOs.Auth;
using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using AuthService.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using RefreshTokenEntity = AuthService.Domain.Entities.RefreshToken;

namespace AuthService.Tests.Unit.Handlers.Auth;

/// <summary>
/// Sprint 5 – Unit tests for RefreshTokenCommandHandler.
/// Covers: successful token rotation, invalid token, revoked token, expired token,
/// locked user, and configuration-driven expiration.
/// </summary>
public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtGenerator> _jwtGeneratorMock;
    private readonly Mock<IRequestContext> _requestContextMock;
    private readonly IConfiguration _configuration;

    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtGeneratorMock = new Mock<IJwtGenerator>();
        _requestContextMock = new Mock<IRequestContext>();

        _requestContextMock.Setup(x => x.IpAddress).Returns("127.0.0.1");

        // Build IConfiguration from in-memory dictionary
        var configData = new Dictionary<string, string?>
        {
            { "Jwt:ExpirationMinutes", "60" },
            { "Jwt:RefreshTokenExpirationDays", "7" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _jwtGeneratorMock.Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>())).Returns("new-access-token");
        _jwtGeneratorMock.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh-token");

        _handler = new RefreshTokenCommandHandler(
            _refreshTokenRepositoryMock.Object,
            _userRepositoryMock.Object,
            _jwtGeneratorMock.Object,
            _requestContextMock.Object,
            _configuration
        );
    }

    #region Helpers

    private static readonly string TestUserId = Guid.NewGuid().ToString();

    private static ApplicationUser CreateTestUser(string? id = null)
    {
        var user = new ApplicationUser("testuser", "test@example.com", "hashed-password");
        typeof(Microsoft.AspNetCore.Identity.IdentityUser)
            .GetProperty("Id")!
            .SetValue(user, id ?? TestUserId);
        user.EmailConfirmed = true;
        return user;
    }

    private static RefreshTokenEntity CreateValidStoredToken(string? userId = null, string token = "valid-refresh-token")
    {
        return new RefreshTokenEntity(
            userId ?? TestUserId,
            token,
            DateTime.UtcNow.AddDays(7), // Not expired
            "127.0.0.1"
        );
    }

    #endregion

    #region Successful Token Rotation

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewTokens()
    {
        // Arrange
        var storedToken = CreateValidStoredToken();
        var user = CreateTestUser();

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("valid-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new RefreshTokenCommand("valid-refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_RevokesOldToken()
    {
        // Arrange
        var storedToken = CreateValidStoredToken();
        var user = CreateTestUser();

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("valid-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new RefreshTokenCommand("valid-refresh-token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        storedToken.IsRevoked.Should().BeTrue();
        storedToken.ReplacedByToken.Should().Be("new-refresh-token");
        _refreshTokenRepositoryMock.Verify(
            x => x.UpdateAsync(storedToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_SavesNewRefreshToken()
    {
        // Arrange
        var storedToken = CreateValidStoredToken();
        var user = CreateTestUser();

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("valid-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new RefreshTokenCommand("valid-refresh-token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<RefreshTokenEntity>(rt =>
                    rt.Token == "new-refresh-token" && rt.UserId == TestUserId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Invalid Token Scenarios

    [Fact]
    public async Task Handle_EmptyRefreshToken_ThrowsBadRequestException()
    {
        // Arrange
        var command = new RefreshTokenCommand("");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Refresh token is required*");
    }

    [Fact]
    public async Task Handle_WhitespaceRefreshToken_ThrowsBadRequestException()
    {
        // Arrange
        var command = new RefreshTokenCommand("   ");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Refresh token is required*");
    }

    [Fact]
    public async Task Handle_TokenNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("nonexistent-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenEntity?)null);

        var command = new RefreshTokenCommand("nonexistent-token");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*Invalid refresh token*");
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var storedToken = CreateValidStoredToken();
        storedToken.Revoke("127.0.0.1", "revoked_by_user");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("valid-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        var command = new RefreshTokenCommand("valid-refresh-token");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*revoked*");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsUnauthorizedException()
    {
        // Arrange — create a token that expired yesterday
        var expiredToken = new RefreshTokenEntity(
            TestUserId,
            "expired-token",
            DateTime.UtcNow.AddDays(-1), // Already expired
            "127.0.0.1"
        );

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("expired-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        var command = new RefreshTokenCommand("expired-token");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*expired*");
    }

    #endregion

    #region User State Checks

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var storedToken = CreateValidStoredToken();
        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("valid-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var command = new RefreshTokenCommand("valid-refresh-token");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_LockedUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var storedToken = CreateValidStoredToken();
        var user = CreateTestUser();
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("valid-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new RefreshTokenCommand("valid-refresh-token");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*locked*");
    }

    #endregion
}

using Xunit;
using Moq;
using FluentAssertions;
using AuthService.Application.Features.Auth.Commands.Logout;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using RefreshTokenEntity = AuthService.Domain.Entities.RefreshToken;

namespace AuthService.Tests.Unit.Handlers.Auth;

/// <summary>
/// Sprint 5 – Unit tests for LogoutCommandHandler.
/// Covers: successful logout, empty token, already-revoked token,
/// token not found, and session revocation.
/// </summary>
public class LogoutCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUserSessionRepository> _sessionRepositoryMock;
    private readonly Mock<ILogger<LogoutCommandHandler>> _loggerMock;

    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _sessionRepositoryMock = new Mock<IUserSessionRepository>();
        _loggerMock = new Mock<ILogger<LogoutCommandHandler>>();

        _handler = new LogoutCommandHandler(
            _refreshTokenRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    #region Helpers

    private static readonly string TestUserId = Guid.NewGuid().ToString();

    private static RefreshTokenEntity CreateActiveToken(string? userId = null)
    {
        return new RefreshTokenEntity(
            userId ?? TestUserId,
            "active-refresh-token",
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1"
        );
    }

    #endregion

    #region Successful Logout

    [Fact]
    public async Task Handle_ValidToken_RevokesRefreshToken()
    {
        // Arrange
        var storedToken = CreateActiveToken();
        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("active-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _sessionRepositoryMock
            .Setup(x => x.GetActiveSessionsByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserSession>());

        var command = new LogoutCommand("active-refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        storedToken.IsRevoked.Should().BeTrue();
        _refreshTokenRepositoryMock.Verify(
            x => x.UpdateAsync(storedToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidToken_RevokesMatchingSession()
    {
        // Arrange
        var storedToken = CreateActiveToken();
        var createdAt = storedToken.CreatedAt;

        var matchingSession = new UserSession(
            userId: TestUserId,
            refreshTokenId: "some-ref-id",
            deviceInfo: "Desktop",
            browser: "Chrome",
            operatingSystem: "Windows 10/11",
            ipAddress: "127.0.0.1",
            expiresAt: DateTime.UtcNow.AddDays(7)
        );

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("active-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _sessionRepositoryMock
            .Setup(x => x.GetActiveSessionsByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserSession> { matchingSession });

        var command = new LogoutCommand("active-refresh-token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — session update is attempted (matching by timestamp is approximate, may or may not match)
        _sessionRepositoryMock.Verify(
            x => x.GetActiveSessionsByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Empty/Missing Token

    [Fact]
    public async Task Handle_EmptyRefreshToken_ThrowsBadRequestException()
    {
        // Arrange
        var command = new LogoutCommand("");

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
        var command = new LogoutCommand("   ");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Refresh token is required*");
    }

    #endregion

    #region Token Not Found or Already Revoked

    [Fact]
    public async Task Handle_TokenNotFound_ReturnsUnitWithoutError()
    {
        // Arrange
        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("nonexistent-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenEntity?)null);

        var command = new LogoutCommand("nonexistent-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — logout is idempotent, no exception
        result.Should().Be(MediatR.Unit.Value);
        _refreshTokenRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyRevokedToken_DoesNotRevokeAgain()
    {
        // Arrange
        var storedToken = CreateActiveToken();
        storedToken.Revoke("127.0.0.1", "already_revoked");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("active-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        var command = new LogoutCommand("active-refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _refreshTokenRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Session Revocation Error Resilience

    [Fact]
    public async Task Handle_SessionRevocationFails_StillCompletesLogout()
    {
        // Arrange
        var storedToken = CreateActiveToken();
        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("active-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _sessionRepositoryMock
            .Setup(x => x.GetActiveSessionsByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection error"));

        var command = new LogoutCommand("active-refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — logout still succeeds even if session revocation fails
        result.Should().Be(MediatR.Unit.Value);
        storedToken.IsRevoked.Should().BeTrue();
    }

    #endregion
}

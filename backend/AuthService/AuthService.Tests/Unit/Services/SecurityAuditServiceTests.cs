using AuthService.Domain.Interfaces.Services;
using AuthService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthService.Tests.Unit.Services;

/// <summary>
/// US-18.5: Unit tests for SecurityAuditService (SIEM logging).
/// </summary>
public class SecurityAuditServiceTests
{
    private readonly Mock<ILogger<SecurityAuditService>> _loggerMock;
    private readonly SecurityAuditService _service;

    public SecurityAuditServiceTests()
    {
        _loggerMock = new Mock<ILogger<SecurityAuditService>>();
        _service = new SecurityAuditService(_loggerMock.Object);
    }

    [Fact]
    public async Task LogLoginSuccessAsync_LogsCorrectEvent()
    {
        // Act
        await _service.LogLoginSuccessAsync(
            userId: "user-123",
            email: "test@example.com",
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            deviceFingerprint: "fp-hash-123",
            usedTwoFactor: true);

        // Assert
        VerifyLogCalled(LogLevel.Information, "AUTH_LOGIN_SUCCESS");
    }

    [Fact]
    public async Task LogLoginFailureAsync_LogsCorrectEvent()
    {
        // Act
        await _service.LogLoginFailureAsync(
            email: "test@example.com",
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            reason: "invalid_password",
            attemptCount: 3);

        // Assert
        VerifyLogCalled(LogLevel.Warning, "AUTH_LOGIN_FAILURE");
    }

    [Fact]
    public async Task LogTwoFactorSuccessAsync_LogsCorrectEvent()
    {
        // Act
        await _service.LogTwoFactorSuccessAsync(
            userId: "user-123",
            method: "TOTP",
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0");

        // Assert
        VerifyLogCalled(LogLevel.Information, "AUTH_2FA_SUCCESS");
    }

    [Fact]
    public async Task LogTwoFactorFailureAsync_LogsCorrectEvent()
    {
        // Act
        await _service.LogTwoFactorFailureAsync(
            userId: "user-123",
            method: "TOTP",
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            attemptCount: 2);

        // Assert
        VerifyLogCalled(LogLevel.Warning, "AUTH_2FA_FAILURE");
    }

    [Fact]
    public async Task LogPasswordChangeAsync_LogsCorrectEvent()
    {
        // Act
        await _service.LogPasswordChangeAsync(
            userId: "user-123",
            email: "test@example.com",
            ipAddress: "192.168.1.1",
            wasForced: false,
            reason: "user_initiated");

        // Assert
        VerifyLogCalled(LogLevel.Information, "AUTH_PASSWORD_CHANGE");
    }

    [Fact]
    public async Task LogAccountLockoutAsync_LogsCorrectEvent()
    {
        // Act
        await _service.LogAccountLockoutAsync(
            userId: "user-123",
            email: "test@example.com",
            ipAddress: "192.168.1.1",
            lockoutDuration: TimeSpan.FromMinutes(15),
            failedAttempts: 5);

        // Assert
        VerifyLogCalled(LogLevel.Warning, "AUTH_ACCOUNT_LOCKOUT");
    }

    [Fact]
    public async Task LogNewDeviceLoginAsync_LogsCorrectEvent()
    {
        // Act
        await _service.LogNewDeviceLoginAsync(
            userId: "user-123",
            email: "test@example.com",
            deviceName: "Chrome on Windows",
            ipAddress: "192.168.1.1",
            location: "Santo Domingo, DO");

        // Assert
        VerifyLogCalled(LogLevel.Information, "AUTH_NEW_DEVICE");
    }

    [Fact]
    public async Task LogRecoveryCodesGeneratedAsync_LogsCorrectEvent()
    {
        // Act
        await _service.LogRecoveryCodesGeneratedAsync(
            userId: "user-123",
            email: "test@example.com",
            codeCount: 10);

        // Assert
        VerifyLogCalled(LogLevel.Information, "AUTH_RECOVERY_CODES_GEN");
    }

    [Fact]
    public async Task LogRecoveryCodeUsedAsync_LogsCorrectEvent()
    {
        // Act
        await _service.LogRecoveryCodeUsedAsync(
            userId: "user-123",
            email: "test@example.com",
            ipAddress: "192.168.1.1",
            remainingCodes: 9);

        // Assert
        VerifyLogCalled(LogLevel.Warning, "AUTH_RECOVERY_CODE_USED");
    }

    [Fact]
    public async Task LogTwoFactorStatusChangeAsync_LogsCorrectEvent()
    {
        // Act
        await _service.LogTwoFactorStatusChangeAsync(
            userId: "user-123",
            email: "test@example.com",
            method: "TOTP",
            enabled: true,
            ipAddress: "192.168.1.1");

        // Assert
        VerifyLogCalled(LogLevel.Information, "AUTH_2FA_STATUS_CHANGE");
    }

    [Fact]
    public async Task LogSessionsRevokedAsync_LogsCorrectEvent()
    {
        // Act
        await _service.LogSessionsRevokedAsync(
            userId: "user-123",
            email: "test@example.com",
            reason: "password_change",
            ipAddress: "192.168.1.1");

        // Assert
        VerifyLogCalled(LogLevel.Warning, "AUTH_SESSIONS_REVOKED");
    }

    [Fact]
    public async Task LogSuspiciousActivityAsync_LogsCorrectEvent()
    {
        // Act
        await _service.LogSuspiciousActivityAsync(
            userId: "user-123",
            email: "test@example.com",
            activityType: "brute_force_attempt",
            description: "Multiple failed logins from different IPs",
            ipAddress: "192.168.1.1",
            userAgent: "curl/7.64.1");

        // Assert - LogSuspiciousActivityAsync uses LogLevel.Error (not Warning)
        VerifyLogCalled(LogLevel.Error, "AUTH_SUSPICIOUS_ACTIVITY");
    }

    private void VerifyLogCalled(LogLevel level, string eventType)
    {
        _loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(eventType)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

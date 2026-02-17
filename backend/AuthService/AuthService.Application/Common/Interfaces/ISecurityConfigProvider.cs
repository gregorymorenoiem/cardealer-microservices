namespace AuthService.Application.Common.Interfaces;

/// <summary>
/// Reads platform-wide security settings from ConfigurationService.
/// Falls back to appsettings.json defaults when ConfigurationService is unavailable.
/// Values are cached (60s) to avoid hitting ConfigurationService on every request.
///
/// Keys read from ConfigurationService:
///   security.max_login_attempts        → MaxLoginAttempts
///   security.lockout_duration_minutes  → LockoutDurationMinutes
///   security.session_expiration_hours  → SessionExpirationHours
///   security.min_password_length       → MinPasswordLength
///   security.jwt_expires_minutes       → JwtExpiresMinutes
///   security.refresh_token_days        → RefreshTokenDays
///   security.require_email_verification → RequireEmailVerification
///   security.allow_2fa                 → Allow2FA
///   security.force_https               → ForceHttps
/// </summary>
public interface ISecurityConfigProvider
{
    /// <summary>Maximum failed login attempts before lockout (default: 5)</summary>
    Task<int> GetMaxLoginAttemptsAsync(CancellationToken ct = default);

    /// <summary>Account lockout duration in minutes (default: 15)</summary>
    Task<int> GetLockoutDurationMinutesAsync(CancellationToken ct = default);

    /// <summary>Session expiration in hours (default: 24)</summary>
    Task<int> GetSessionExpirationHoursAsync(CancellationToken ct = default);

    /// <summary>Minimum password length (default: 8)</summary>
    Task<int> GetMinPasswordLengthAsync(CancellationToken ct = default);

    /// <summary>JWT access token expiration in minutes (default: 60)</summary>
    Task<int> GetJwtExpiresMinutesAsync(CancellationToken ct = default);

    /// <summary>Refresh token lifetime in days (default: 7)</summary>
    Task<int> GetRefreshTokenDaysAsync(CancellationToken ct = default);

    /// <summary>Whether email verification is required (default: true)</summary>
    Task<bool> GetRequireEmailVerificationAsync(CancellationToken ct = default);

    /// <summary>Whether two-factor authentication is allowed (default: true)</summary>
    Task<bool> GetAllow2FAAsync(CancellationToken ct = default);

    /// <summary>Whether HTTPS is forced (default: true)</summary>
    Task<bool> GetForceHttpsAsync(CancellationToken ct = default);
}

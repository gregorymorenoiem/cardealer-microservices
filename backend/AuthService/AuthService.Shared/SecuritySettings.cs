namespace AuthService.Shared;

/// <summary>
/// Security-related configuration settings
/// </summary>
public class SecuritySettings
{
    /// <summary>Password policy configuration</summary>
    public PasswordPolicySettings PasswordPolicy { get; set; } = new();

    /// <summary>Lockout policy configuration</summary>
    public LockoutPolicySettings LockoutPolicy { get; set; } = new();

    /// <summary>Two-factor authentication settings</summary>
    public TwoFactorSettings TwoFactor { get; set; } = new();

    /// <summary>Rate limiting settings</summary>
    public RateLimitSettings RateLimit { get; set; } = new();
}

/// <summary>
/// Password policy configuration
/// </summary>
public class PasswordPolicySettings
{
    /// <summary>Minimum password length</summary>
    public int RequiredLength { get; set; } = 8;

    /// <summary>Whether to require uppercase characters</summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>Whether to require lowercase characters</summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>Whether to require digits</summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>Whether to require non-alphanumeric characters</summary>
    public bool RequireNonAlphanumeric { get; set; } = false;

    /// <summary>Maximum number of failed password attempts before lockout</summary>
    public int MaxFailedAttempts { get; set; } = 5;
}

/// <summary>
/// Account lockout policy configuration
/// </summary>
public class LockoutPolicySettings
{
    /// <summary>Whether lockout is enabled</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Lockout duration in minutes</summary>
    public int DefaultLockoutMinutes { get; set; } = 30;

    /// <summary>Maximum number of failed attempts before lockout</summary>
    public int MaxFailedAccessAttempts { get; set; } = 5;
}

/// <summary>
/// Two-factor authentication settings
/// </summary>
public class TwoFactorSettings
{
    /// <summary>Whether two-factor authentication is enabled</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>Two-factor authentication providers</summary>
    public string[] Providers { get; set; } = ["Email", "SMS"];

    /// <summary>Remember browser duration in days</summary>
    public int RememberBrowserDays { get; set; } = 14;
}

/// <summary>
/// Rate limiting settings
/// </summary>
public class RateLimitSettings
{
    /// <summary>Whether rate limiting is enabled</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Maximum requests per minute</summary>
    public int RequestsPerMinute { get; set; } = 60;

    /// <summary>Maximum login attempts per hour</summary>
    public int LoginAttemptsPerHour { get; set; } = 10;

    /// <summary>Maximum password reset attempts per hour</summary>
    public int PasswordResetAttemptsPerHour { get; set; } = 5;
}

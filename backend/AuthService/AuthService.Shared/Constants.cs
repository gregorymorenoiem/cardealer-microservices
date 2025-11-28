namespace AuthService.Shared;

/// <summary>
/// Application constants
/// </summary>
public static class Constants
{
    /// <summary>
    /// Cache key patterns
    /// </summary>
    public static class CacheKeys
    {
        public const string UserById = "user_{0}";
        public const string UserByEmail = "user_email_{0}";
        public const string RefreshToken = "refresh_token_{0}";
        public const string VerificationToken = "verification_token_{0}";
        public const string RateLimit = "rate_limit_{0}";
    }

    /// <summary>
    /// Policy names for authorization
    /// </summary>
    public static class Policies
    {
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireVerifiedEmail = "RequireVerifiedEmail";
        public const string AllowAnonymous = "AllowAnonymous";
    }

    /// <summary>
    /// Role names
    /// </summary>
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string User = "User";
        public const string Moderator = "Moderator";
    }

    /// <summary>
    /// Claim types
    /// </summary>
    public static class ClaimTypes
    {
        public const string UserId = "sub";
        public const string Email = "email";
        public const string Name = "name";
        public const string Role = "role";
        public const string EmailVerified = "email_verified";
    }

    /// <summary>
    /// HTTP header names
    /// </summary>
    public static class Headers
    {
        public const string ApiKey = "X-API-Key";
        public const string CorrelationId = "X-Correlation-ID";
        public const string UserAgent = "User-Agent";
        public const string ForwardedFor = "X-Forwarded-For";
    }

    /// <summary>
    /// Token expiration times
    /// </summary>
    public static class TokenExpiration
    {
        public static readonly TimeSpan AccessToken = TimeSpan.FromMinutes(60);
        public static readonly TimeSpan RefreshToken = TimeSpan.FromDays(7);
        public static readonly TimeSpan EmailVerification = TimeSpan.FromHours(24);
        public static readonly TimeSpan PasswordReset = TimeSpan.FromHours(1);
    }

    /// <summary>
    /// Rate limit periods
    /// </summary>
    public static class RateLimitPeriods
    {
        public const int LoginPerMinute = 5;
        public const int LoginPerHour = 20;
        public const int RegistrationPerHour = 10;
        public const int PasswordResetPerHour = 5;
    }
}
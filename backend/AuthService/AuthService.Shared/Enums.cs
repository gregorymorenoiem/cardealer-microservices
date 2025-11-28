namespace AuthService.Shared;

/// <summary>
/// Application enums
/// </summary>
public static class Enums
{
    /// <summary>
    /// Token types for verification
    /// </summary>
    public enum TokenType
    {
        EmailVerification = 1,
        PasswordReset = 2,
        PhoneVerification = 3,
        TwoFactor = 4
    }

    /// <summary>
    /// User status
    /// </summary>
    public enum UserStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        Locked = 4
    }

    /// <summary>
    /// Authentication providers
    /// </summary>
    public enum AuthProvider
    {
        Internal = 1,
        Google = 2,
        Facebook = 3,
        Microsoft = 4
    }

    /// <summary>
    /// Log levels
    /// </summary>
    public enum LogLevel
    {
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5
    }
}
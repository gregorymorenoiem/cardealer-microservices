using System.Text.RegularExpressions;

namespace AuthService.Shared;

/// <summary>
/// Regular expression patterns for validation
/// </summary>
public static class ValidationPatterns
{
    /// <summary>Pattern for email validation</summary>
    public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    /// <summary>Pattern for password validation (at least 8 chars, 1 uppercase, 1 lowercase, 1 number)</summary>
    public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";

    /// <summary>Pattern for username validation (alphanumeric, underscores, hyphens, 3-20 chars)</summary>
    public const string Username = @"^[a-zA-Z0-9_-]{3,20}$";

    /// <summary>Pattern for phone number validation (international format)</summary>
    public const string PhoneNumber = @"^\+?[1-9]\d{1,14}$";

    /// <summary>Pattern for JWT token validation</summary>
    public const string JwtToken = @"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]*$";

    /// <summary>
    /// Validates an email address
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return Regex.IsMatch(email, Email, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Validates a password against complexity requirements
    /// </summary>
    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        return Regex.IsMatch(password, Password);
    }

    /// <summary>
    /// Validates a username
    /// </summary>
    public static bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        return Regex.IsMatch(username, Username);
    }
}
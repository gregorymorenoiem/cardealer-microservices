namespace AuthService.Domain.Interfaces.Services;

/// <summary>
/// US-18.3: Service for verifying CAPTCHA tokens (Google reCAPTCHA v3).
/// Used to prevent brute force attacks after 2+ failed login attempts.
/// </summary>
public interface ICaptchaService
{
    /// <summary>
    /// Verifies a CAPTCHA token with Google reCAPTCHA API.
    /// </summary>
    /// <param name="token">The CAPTCHA token from the client.</param>
    /// <param name="expectedAction">The expected action name (e.g., "login", "register").</param>
    /// <param name="ipAddress">The user's IP address for additional verification.</param>
    /// <returns>True if the token is valid and score is above threshold; otherwise false.</returns>
    Task<bool> VerifyAsync(string token, string expectedAction, string? ipAddress = null);

    /// <summary>
    /// Gets the last verification score (0.0 to 1.0).
    /// Score 0.0 = likely bot, 1.0 = likely human.
    /// </summary>
    decimal LastScore { get; }

    /// <summary>
    /// Checks if CAPTCHA is required based on failed attempt count.
    /// CAPTCHA is required after 2+ failed attempts.
    /// </summary>
    /// <param name="failedAttemptCount">Number of failed login attempts.</param>
    /// <returns>True if CAPTCHA is required.</returns>
    bool IsCaptchaRequired(int failedAttemptCount);
}

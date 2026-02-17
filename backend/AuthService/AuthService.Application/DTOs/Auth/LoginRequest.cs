namespace AuthService.Application.DTOs.Auth;

/// <summary>
/// Login request DTO with optional CAPTCHA token.
/// CaptchaToken is required after 2+ failed login attempts.
/// </summary>
public record LoginRequest(string Email, string Password, string? CaptchaToken = null);

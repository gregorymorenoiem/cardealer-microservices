namespace AuthService.Application.DTOs.Auth;

/// <summary>
/// DTO for user registration from frontend.
/// JSON deserialization is case-insensitive, so camelCase from frontend maps correctly.
/// Positional order preserves backward compatibility with existing test code.
/// Frontend sends: { firstName, lastName, email, phone, password, acceptTerms }
/// </summary>
public record RegisterRequest(
    string? UserName = null,
    string Email = "",
    string Password = "",
    string? FirstName = null,
    string? LastName = null,
    string? Phone = null,
    bool AcceptTerms = true
);

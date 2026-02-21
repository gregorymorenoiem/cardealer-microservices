using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs.Auth;

/// <summary>
/// DTO for user registration from frontend.
/// JSON deserialization is case-insensitive, so camelCase from frontend maps correctly.
/// Positional order preserves backward compatibility with existing test code.
/// Frontend sends: { firstName, lastName, email, phone, password, acceptTerms, accountType?, userIntent? }
/// accountType: "buyer" (default) | "seller"
/// userIntent: "browse" (default) | "buy" | "sell" | "buy_and_sell"
/// </summary>
public record RegisterRequest(
    string? UserName = null,
    string Email = "",
    string Password = "",
    string? FirstName = null,
    string? LastName = null,
    string? Phone = null,
    bool AcceptTerms = true,
    string? AccountType = null,   // "buyer" | "seller" — defaults to Buyer
    string? UserIntent = null     // "browse" | "buy" | "sell" | "buy_and_sell" — defaults to Browse
);

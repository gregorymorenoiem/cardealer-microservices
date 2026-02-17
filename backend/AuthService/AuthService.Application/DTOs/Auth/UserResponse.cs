namespace AuthService.Application.DTOs.Auth;

/// <summary>
/// Response for current authenticated user endpoint (/me)
/// </summary>
public record UserResponse
{
    public required string Id { get; init; }
    public required string Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string FullName => $"{FirstName ?? ""} {LastName ?? ""}".Trim();
    public string? AvatarUrl { get; init; }
    public string? Phone { get; init; }
    public required string AccountType { get; init; }
    public bool IsVerified { get; init; }
    public bool IsEmailVerified { get; init; }
    public bool IsPhoneVerified { get; init; }
    public string PreferredLocale { get; init; } = "es-DO";
    public string PreferredCurrency { get; init; } = "DOP";
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

using AuthService.Domain.Enums;

namespace AuthService.Domain.Interfaces.Services;

/// <summary>
/// Result of external token validation with user profile information
/// </summary>
public record ExternalTokenValidationResult(
    bool IsValid,
    string Email,
    string UserId,
    string Name,
    string? FirstName = null,
    string? LastName = null,
    string? ProfilePictureUrl = null
);

public interface IExternalTokenValidator
{
    Task<ExternalTokenValidationResult> ValidateGoogleTokenAsync(string idToken);
    Task<ExternalTokenValidationResult> ValidateMicrosoftTokenAsync(string idToken);
    Task<ExternalTokenValidationResult> ValidateFacebookTokenAsync(string accessToken);
    Task<ExternalTokenValidationResult> ValidateAppleTokenAsync(string idToken);
}

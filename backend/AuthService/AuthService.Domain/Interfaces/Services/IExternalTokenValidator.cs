using AuthService.Domain.Enums;

namespace AuthService.Domain.Interfaces.Services;

public interface IExternalTokenValidator
{
    Task<(bool isValid, string email, string userId, string name)> ValidateGoogleTokenAsync(string idToken);
    Task<(bool isValid, string email, string userId, string name)> ValidateMicrosoftTokenAsync(string idToken);
}

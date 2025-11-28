using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.ValueObjects;

namespace AuthService.Domain.Interfaces.Services;

public interface IExternalAuthService
{
    Task<(ApplicationUser user, bool isNewUser)> AuthenticateAsync(ExternalAuthProvider provider, string idToken);
    Task<ApplicationUser?> FindUserByExternalIdAsync(ExternalAuthProvider provider, string externalUserId);
    Task<bool> ValidateTokenAsync(ExternalAuthProvider provider, string idToken);
    Task<string> GetUserInfoAsync(ExternalAuthProvider provider, string accessToken);
    Task<ExternalUserInfo> GetUserInfoTypedAsync(ExternalAuthProvider provider, string accessToken);
}
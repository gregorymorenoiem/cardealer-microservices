using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces.Services;

public interface ITokenService
{
    Task<(string accessToken, string refreshToken)> GenerateTokensAsync(ApplicationUser user, string ipAddress);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, string ipAddress, string reason = "revoked");
    Task<bool> IsAccessTokenValidAsync(string accessToken, string userId);
    Task CleanupExpiredTokensAsync();
}
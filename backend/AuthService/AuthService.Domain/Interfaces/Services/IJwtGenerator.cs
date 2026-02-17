using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces.Services;

public interface IJwtGenerator
{
    string GenerateToken(ApplicationUser user);

    /// <summary>
    /// Generate a JWT with a custom expiration (read from ConfigurationService / admin panel).
    /// When <paramref name="expiresMinutes"/> is null the appsettings default is used.
    /// </summary>
    string GenerateToken(ApplicationUser user, int? expiresMinutes);

    string GenerateRefreshToken();
    (string userId, string email)? ValidateToken(string token);
    string GenerateTempToken(string userId);
    (string userId, string email)? ValidateTempToken(string token);
}

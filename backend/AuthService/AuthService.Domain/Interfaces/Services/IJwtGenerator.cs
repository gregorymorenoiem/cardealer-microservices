using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces.Services;

public interface IJwtGenerator
{
    string GenerateToken(ApplicationUser user);
    string GenerateRefreshToken();
    (string userId, string email)? ValidateToken(string token);
    string GenerateTempToken(string userId);
    (string userId, string email)? ValidateTempToken(string token);
}

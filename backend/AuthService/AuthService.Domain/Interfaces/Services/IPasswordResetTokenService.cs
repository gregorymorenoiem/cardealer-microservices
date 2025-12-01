namespace AuthService.Domain.Interfaces.Services;

public interface IPasswordResetTokenService
{
    string GenerateResetToken(string email);
    bool ValidateResetToken(string token, out string email);
    Task<bool> IsTokenValidAsync(string email, string token);
    Task InvalidateTokenAsync(string email);
}

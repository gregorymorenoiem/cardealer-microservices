using AuthService.Domain.Interfaces.Services;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Enums;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using AuthService.Shared.Exceptions;

namespace AuthService.Infrastructure.Services.Identity;

public class PasswordResetTokenService : IPasswordResetTokenService
{
    private readonly IVerificationTokenRepository _verificationTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public PasswordResetTokenService(
        IVerificationTokenRepository verificationTokenRepository,
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager)
    {
        _verificationTokenRepository = verificationTokenRepository;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public string GenerateResetToken(string email)
    {
        // Este método ahora solo genera el formato del token
        // La generación real del token se hará en el use case usando UserManager
        return Guid.NewGuid().ToString("N");
    }

    public bool ValidateResetToken(string token, out string email)
    {
        email = string.Empty; // Inicializa con string.Empty en lugar de null

        var verificationToken = _verificationTokenRepository.GetByTokenAndTypeAsync(
            token, VerificationTokenType.PasswordReset).Result;

        if (verificationToken == null || !verificationToken.IsValid())
            return false;

        email = verificationToken.Email;
        return true;
    }

    public async Task<bool> IsTokenValidAsync(string email, string token)
    {
        var verificationToken = await _verificationTokenRepository.GetByTokenAndTypeAsync(
            token, VerificationTokenType.PasswordReset);

        return verificationToken != null &&
               verificationToken.Email == email &&
               verificationToken.IsValid() &&
               !verificationToken.IsUsed;
    }

    public async Task InvalidateTokenAsync(string email)
    {
        var tokens = await _verificationTokenRepository.GetByEmailAsync(email);
        var resetTokens = tokens.Where(t =>
            t.Type == VerificationTokenType.PasswordReset &&
            t.IsValid() &&
            !t.IsUsed);

        foreach (var token in resetTokens)
        {
            token.MarkAsUsed();
            await _verificationTokenRepository.UpdateAsync(token);
        }
    }

    public async Task<string> GenerateIdentityResetTokenAsync(ApplicationUser user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ResetPasswordWithTokenAsync(ApplicationUser user, string token, string newPassword)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }
}

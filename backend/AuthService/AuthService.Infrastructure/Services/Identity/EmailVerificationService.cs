using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using AuthService.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using ErrorService.Shared.Exceptions;

namespace AuthService.Infrastructure.Services.Identity;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IUserRepository _userRepository;
    private readonly IVerificationTokenRepository _verificationTokenRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmailVerificationService(
        IUserRepository userRepository,
        IVerificationTokenRepository verificationTokenRepository,
        UserManager<ApplicationUser> userManager)
    {
        _userRepository = userRepository;
        _verificationTokenRepository = verificationTokenRepository;
        _userManager = userManager;
    }

    public async Task SendVerificationEmailAsync(ApplicationUser user)
    {
        // Generar token usando Identity
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Crear y guardar el token de verificación
        var verificationToken = new VerificationToken(
            user.Id,
            VerificationTokenType.EmailVerification,
            TimeSpan.FromHours(24))
        {
            Token = token,
            Email = user.Email ?? throw new InvalidOperationException("User email is null") // Manejo de null
        };

        await _verificationTokenRepository.AddAsync(verificationToken);
    }
    public async Task<bool> VerifyAsync(string token)
    {
        var verificationToken = await _verificationTokenRepository.GetByTokenAndTypeAsync(
            token, VerificationTokenType.EmailVerification);

        if (verificationToken == null || !verificationToken.IsValid())
            return false;

        var user = await _userRepository.GetByIdAsync(verificationToken.UserId);
        if (user == null)
            return false;

        // Verificar el token usando Identity
        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            // Marcar el token como usado
            verificationToken.MarkAsUsed();
            await _verificationTokenRepository.UpdateAsync(verificationToken);

            // Confirmar el email en la entidad de usuario
            user.ConfirmEmail();
            await _userRepository.UpdateAsync(user);

            return true;
        }

        return false;
    }
}
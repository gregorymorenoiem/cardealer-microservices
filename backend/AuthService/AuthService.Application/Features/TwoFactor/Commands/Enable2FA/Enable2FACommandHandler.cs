using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using ErrorService.Shared.Exceptions;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.Enable2FA;

public class Enable2FACommandHandler : IRequestHandler<Enable2FACommand, Enable2FAResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IAuthNotificationService _notificationService;

    public Enable2FACommandHandler(
        IUserRepository userRepository,
        ITwoFactorService twoFactorService,
        IAuthNotificationService notificationService)
    {
        _userRepository = userRepository;
        _twoFactorService = twoFactorService;
        _notificationService = notificationService;
    }

    public async Task<Enable2FAResponse> Handle(Enable2FACommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException("User not found.");

        if (user.IsTwoFactorEnabled)
            throw new BadRequestException("Two-factor authentication is already enabled.");

        string secret = string.Empty;
        string qrCodeUri = string.Empty;
        List<string> recoveryCodes;

        // Generar según el tipo de 2FA
        switch (request.Type)
        {
            case TwoFactorAuthType.Authenticator:
                // Generar clave para Authenticator App
                (secret, qrCodeUri) = await _twoFactorService.GenerateAuthenticatorKeyAsync(user.Id, user.Email!);
                break;

            case TwoFactorAuthType.SMS:
            case TwoFactorAuthType.Email:
                // Para SMS/Email, generar códigos pero no QR
                secret = await _twoFactorService.GenerateEmailCodeAsync(user.Id);
                qrCodeUri = string.Empty;
                break;

            default:
                throw new BadRequestException("Invalid two-factor authentication type.");
        }

        // Generar códigos de recuperación
        recoveryCodes = await _twoFactorService.GenerateRecoveryCodesAsync(user.Id);

        // Crear y guardar entidad 2FA
        var twoFactorAuth = new TwoFactorAuth(user.Id, request.Type);
        twoFactorAuth.Enable(secret, recoveryCodes);

        // GUARDAR en base de datos
        await _userRepository.AddOrUpdateTwoFactorAuthAsync(twoFactorAuth);

        // Enviar códigos de recuperación por email
        await _notificationService.SendTwoFactorBackupCodesAsync(user.Email!, recoveryCodes);

        return new Enable2FAResponse(secret, qrCodeUri, recoveryCodes);
    }
}
using AuthService.Shared.Exceptions;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.GenerateRecoveryCodes;

public class GenerateRecoveryCodesCommandHandler : IRequestHandler<GenerateRecoveryCodesCommand, GenerateRecoveryCodesResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IAuthNotificationService _notificationService;

    public GenerateRecoveryCodesCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITwoFactorService twoFactorService,
        IAuthNotificationService notificationService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _twoFactorService = twoFactorService;
        _notificationService = notificationService;
    }

    public async Task<GenerateRecoveryCodesResponse> Handle(GenerateRecoveryCodesCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException("User not found.");

        // Verificar contraseña
        if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid password.");

        if (!user.IsTwoFactorEnabled)
            throw new BadRequestException("Two-factor authentication is not enabled.");

        // Generar nuevos códigos de recuperación
        var recoveryCodes = await _twoFactorService.GenerateRecoveryCodesAsync(user.Id);

        // Enviar códigos por email
        await _notificationService.SendTwoFactorBackupCodesAsync(user.Email!, recoveryCodes);

        return new GenerateRecoveryCodesResponse(recoveryCodes);
    }
}

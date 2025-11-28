// AuthService.Application/UseCases/Verify2FA/Verify2FACommandHandler.cs
using AuthService.Shared.Exceptions;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.Verify2FA;

public class Verify2FACommandHandler : IRequestHandler<Verify2FACommand, Verify2FAResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITwoFactorService _twoFactorService;

    public Verify2FACommandHandler(
        IUserRepository userRepository,
        ITwoFactorService twoFactorService)
    {
        _userRepository = userRepository;
        _twoFactorService = twoFactorService;
    }

    public async Task<Verify2FAResponse> Handle(Verify2FACommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException("User not found.");

        bool isValid;

        // CORRECCIÓN: Usar PrimaryMethod en lugar de Type
        switch (request.Type)
        {
            case TwoFactorAuthType.Authenticator:
                if (user.TwoFactorAuth?.Secret == null)
                    throw new BadRequestException("Two-factor authentication is not set up for this user.");

                isValid = _twoFactorService.VerifyAuthenticatorCode(user.TwoFactorAuth.Secret, request.Code);
                break;

            case TwoFactorAuthType.SMS:
            case TwoFactorAuthType.Email:
                isValid = await _twoFactorService.VerifyCodeAsync(user.Id, request.Code, request.Type);
                break;

            default:
                throw new BadRequestException("Invalid two-factor authentication type.");
        }

        if (isValid)
        {
            // Marcar como usado si es necesario
            user.TwoFactorAuth?.MarkAsUsed();
            await _userRepository.UpdateAsync(user, cancellationToken);

            return new Verify2FAResponse(true, "Two-factor authentication verified successfully.");
        }

        return new Verify2FAResponse(false, "Invalid verification code.");
    }
}

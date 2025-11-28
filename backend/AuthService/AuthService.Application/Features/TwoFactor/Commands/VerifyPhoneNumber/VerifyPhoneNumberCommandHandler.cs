using ErrorService.Shared.Exceptions;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using AuthService.Domain.Enums;
using AuthService.Application.DTOs.Auth;

namespace AuthService.Application.Features.TwoFactor.Commands.VerifyPhoneNumber;

public class VerifyPhoneNumberCommandHandler : IRequestHandler<VerifyPhoneNumberCommand, VerifyPhoneNumberResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IAuthNotificationService _notificationService;

    public VerifyPhoneNumberCommandHandler(
        IUserRepository userRepository,
        ITwoFactorService twoFactorService,
        IAuthNotificationService notificationService)
    {
        _userRepository = userRepository;
        _twoFactorService = twoFactorService;
        _notificationService = notificationService;
    }

    public async Task<VerifyPhoneNumberResponse> Handle(VerifyPhoneNumberCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException("User not found.");

        // Verificar si el número ya está verificado
        if (user.PhoneNumberConfirmed && user.PhoneNumber == request.PhoneNumber)
        {
            return new VerifyPhoneNumberResponse(true, "Phone number is already verified.", true);
        }

        // Validar código de verificación
        bool isValidCode = await _twoFactorService.VerifyCodeAsync(
            user.Id,
            request.VerificationCode,
            TwoFactorAuthType.SMS);

        if (!isValidCode)
        {
            throw new BadRequestException("Invalid verification code.");
        }

        // Actualizar número de teléfono y marcarlo como verificado
        user.PhoneNumber = request.PhoneNumber;
        user.PhoneNumberConfirmed = true;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new VerifyPhoneNumberResponse(true, "Phone number verified successfully.", true);
    }
}
using AuthService.Shared.Exceptions;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using AuthService.Domain.Enums;
using AuthService.Application.DTOs.PhoneVerification;

namespace AuthService.Application.Features.TwoFactor.Commands.SendPhoneVerification;

public class SendPhoneVerificationCommandHandler : IRequestHandler<SendPhoneVerificationCommand, SendPhoneVerificationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IAuthNotificationService _notificationService;

    public SendPhoneVerificationCommandHandler(
        IUserRepository userRepository,
        ITwoFactorService twoFactorService,
        IAuthNotificationService notificationService)
    {
        _userRepository = userRepository;
        _twoFactorService = twoFactorService;
        _notificationService = notificationService;
    }

    public async Task<SendPhoneVerificationResponse> Handle(SendPhoneVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException("User not found.");

        // Generar código de verificación
        var verificationCode = await _twoFactorService.GenerateSmsCodeAsync(user.Id);

        await _notificationService.SendTwoFactorCodeAsync(
            request.PhoneNumber,
            verificationCode,
            TwoFactorAuthType.SMS);

        return new SendPhoneVerificationResponse(
            true,
            "Verification code sent successfully.",
            DateTime.UtcNow.AddMinutes(10)
        );
    }
}

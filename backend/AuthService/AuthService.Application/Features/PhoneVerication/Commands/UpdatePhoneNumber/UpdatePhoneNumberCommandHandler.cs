using AuthService.Application.DTOs.Auth;
using AuthService.Application.DTOs.PhoneVerification;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using ErrorService.Shared.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.PhoneVerification.Commands.UpdatePhoneNumber;

public class UpdatePhoneNumberCommandHandler : IRequestHandler<UpdatePhoneNumberCommand, SendPhoneVerificationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IAuthNotificationService _notificationService;
    private readonly ILogger<UpdatePhoneNumberCommandHandler> _logger;

    public UpdatePhoneNumberCommandHandler(
        IUserRepository userRepository,
        ITwoFactorService twoFactorService,
        IAuthNotificationService notificationService,
        ILogger<UpdatePhoneNumberCommandHandler> logger)
    {
        _userRepository = userRepository;
        _twoFactorService = twoFactorService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<SendPhoneVerificationResponse> Handle(UpdatePhoneNumberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId)
                ?? throw new NotFoundException("User not found.");

            // Update phone number and mark as unverified
            user.PhoneNumber = request.NewPhoneNumber;
            user.PhoneNumberConfirmed = false;

            await _userRepository.UpdateAsync(user, cancellationToken);

            // Generate and send verification code
            var verificationCode = await _twoFactorService.GenerateSmsCodeAsync(user.Id);

            await _notificationService.SendTwoFactorCodeAsync(
                request.NewPhoneNumber,
                verificationCode,
                AuthService.Domain.Enums.TwoFactorAuthType.SMS);

            _logger.LogInformation("Phone number updated and verification sent for user {UserId}", request.UserId);

            return new SendPhoneVerificationResponse(
                true,
                "Phone number updated and verification code sent successfully.",
                DateTime.UtcNow.AddMinutes(10)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating phone number for user {UserId}", request.UserId);
            throw;
        }
    }
}
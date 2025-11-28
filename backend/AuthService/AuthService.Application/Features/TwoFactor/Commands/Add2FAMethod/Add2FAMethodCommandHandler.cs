using ErrorService.Shared.Exceptions;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using AuthService.Domain.Enums;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.Add2FAMethod;

public class Add2FAMethodCommandHandler : IRequestHandler<Add2FAMethodCommand, Change2FAMethodResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IAuthNotificationService _notificationService;

    public Add2FAMethodCommandHandler(
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

    public async Task<Change2FAMethodResponse> Handle(Add2FAMethodCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException("User not found.");

        if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid password.");

        if (!user.IsTwoFactorEnabled)
            throw new BadRequestException("Two-factor authentication is not enabled.");

        var twoFactorAuth = await _userRepository.GetTwoFactorAuthAsync(user.Id);
        if (twoFactorAuth == null)
            throw new NotFoundException("Two-factor authentication configuration not found.");

        if (twoFactorAuth.IsMethodEnabled(request.Method))
            throw new BadRequestException($"Method {request.Method} is already enabled.");

        string? secret = null;
        string? qrCodeUri = null;
        string? phoneNumber = request.PhoneNumber;

        switch (request.Method)
        {
            case TwoFactorAuthType.Authenticator:
                (secret, qrCodeUri) = await _twoFactorService.GenerateAuthenticatorKeyAsync(user.Id, user.Email!);
                break;

            case TwoFactorAuthType.SMS:
                if (string.IsNullOrEmpty(phoneNumber))
                    throw new BadRequestException("Phone number is required for SMS 2FA.");

                if (!IsValidPhoneNumber(phoneNumber))
                    throw new BadRequestException("Invalid phone number format.");

                secret = await _twoFactorService.GenerateSmsCodeAsync(user.Id);
                await _notificationService.SendTwoFactorCodeAsync(phoneNumber, secret, TwoFactorAuthType.SMS);
                break;

            case TwoFactorAuthType.Email:
                secret = await _twoFactorService.GenerateEmailCodeAsync(user.Id);
                await _notificationService.SendTwoFactorCodeAsync(user.Email!, secret, TwoFactorAuthType.Email);
                break;

            default:
                throw new BadRequestException("Invalid two-factor authentication method.");
        }

        twoFactorAuth.AddSecondaryMethod(request.Method);
        await _userRepository.AddOrUpdateTwoFactorAuthAsync(twoFactorAuth);

        return new Change2FAMethodResponse(
            true,
            $"Two-factor authentication method {request.Method} added successfully.",
            secret,
            qrCodeUri,
            phoneNumber
        );
    }

    private bool IsValidPhoneNumber(string phoneNumber) =>
        !string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Length >= 10;
}
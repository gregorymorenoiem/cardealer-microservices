using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using AuthService.Shared.Exceptions;
using MediatR;

namespace AuthService.Application.Features.Auth.Commands.ResendVerification;

public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IVerificationTokenRepository _verificationTokenRepository;
    private readonly IAuthNotificationService _notificationService;

    public ResendVerificationCommandHandler(
        IUserRepository userRepository,
        IVerificationTokenRepository verificationTokenRepository,
        IAuthNotificationService notificationService)
    {
        _userRepository = userRepository;
        _verificationTokenRepository = verificationTokenRepository;
        _notificationService = notificationService;
    }

    public async Task<Unit> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            // Return success even if user not found (security: don't reveal if email exists)
            return Unit.Value;
        }

        // If already verified, no need to resend
        if (user.EmailConfirmed)
        {
            // Return success (don't reveal that user is already verified for security)
            return Unit.Value;
        }

        // Invalidate any existing verification tokens for this user's email
        var existingTokens = await _verificationTokenRepository.GetByEmailAsync(user.Email!);
        foreach (var token in existingTokens.Where(t => t.Type == VerificationTokenType.EmailVerification && !t.IsUsed))
        {
            token.MarkAsUsed(); // Invalidate old tokens
            await _verificationTokenRepository.UpdateAsync(token);
        }

        // Create new verification token
        var verificationToken = new VerificationToken(
            user.Id,
            VerificationTokenType.EmailVerification,
            TimeSpan.FromHours(24) // 24 hours expiration
        );

        await _verificationTokenRepository.AddAsync(verificationToken);

        // Send verification email
        await _notificationService.SendEmailConfirmationAsync(user.Email!, verificationToken.Token);

        return Unit.Value;
    }
}

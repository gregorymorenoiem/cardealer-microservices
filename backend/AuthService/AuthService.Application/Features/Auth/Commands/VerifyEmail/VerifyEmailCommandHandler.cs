using AuthService.Shared.Exceptions;
using AuthService.Domain.Interfaces.Services;
using MediatR;

namespace AuthService.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IAuthNotificationService _notificationService;

    public VerifyEmailCommandHandler(
        IEmailVerificationService emailVerificationService,
        IAuthNotificationService notificationService)
    {
        _emailVerificationService = emailVerificationService;
        _notificationService = notificationService;
    }

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var result = await _emailVerificationService.VerifyAsync(request.Token);

        if (!result.Success)
            throw new BadRequestException("Invalid or expired verification token.");

        // ✅ AHORA que el email está verificado, enviamos el email de bienvenida
        if (!string.IsNullOrEmpty(result.Email) && !string.IsNullOrEmpty(result.UserName))
        {
            await _notificationService.SendWelcomeEmailAsync(result.Email, result.UserName);
        }

        return Unit.Value;
    }
}

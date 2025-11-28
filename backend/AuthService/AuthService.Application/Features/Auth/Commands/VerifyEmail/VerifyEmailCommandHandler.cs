using ErrorService.Shared.Exceptions;
using AuthService.Domain.Interfaces.Services;
using MediatR;

namespace AuthService.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly IEmailVerificationService _emailVerificationService;

    public VerifyEmailCommandHandler(IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
    }

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var success = await _emailVerificationService.VerifyAsync(request.Token);

        if (!success)
            throw new BadRequestException("Invalid or expired verification token.");

        return Unit.Value;
    }
}
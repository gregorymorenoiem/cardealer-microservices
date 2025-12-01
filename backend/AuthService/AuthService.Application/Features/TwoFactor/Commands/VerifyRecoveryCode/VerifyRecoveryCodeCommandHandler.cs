using AuthService.Shared.Exceptions;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.VerifyRecoveryCode;

public class VerifyRecoveryCodeCommandHandler : IRequestHandler<VerifyRecoveryCodeCommand, Verify2FAResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITwoFactorService _twoFactorService;

    public VerifyRecoveryCodeCommandHandler(
        IUserRepository userRepository,
        ITwoFactorService twoFactorService)
    {
        _userRepository = userRepository;
        _twoFactorService = twoFactorService;
    }

    public async Task<Verify2FAResponse> Handle(VerifyRecoveryCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException("User not found.");

        if (!user.IsTwoFactorEnabled)
            throw new BadRequestException("Two-factor authentication is not enabled.");

        var isValid = await _twoFactorService.VerifyRecoveryCodeAsync(user.Id, request.Code);

        if (isValid)
        {
            return new Verify2FAResponse(true, "Recovery code verified successfully.");
        }

        return new Verify2FAResponse(false, "Invalid recovery code.");
    }
}

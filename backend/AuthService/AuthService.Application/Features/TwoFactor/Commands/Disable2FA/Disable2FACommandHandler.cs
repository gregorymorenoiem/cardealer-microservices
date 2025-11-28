using ErrorService.Shared.Exceptions;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.Disable2FA;

public class Disable2FACommandHandler : IRequestHandler<Disable2FACommand, Verify2FAResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITwoFactorService _twoFactorService;

    public Disable2FACommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITwoFactorService twoFactorService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _twoFactorService = twoFactorService;
    }

    public async Task<Verify2FAResponse> Handle(Disable2FACommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException("User not found.");

        // Verificar contraseña
        if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid password.");

        if (!user.IsTwoFactorEnabled)
            throw new BadRequestException("Two-factor authentication is not enabled.");

        // Deshabilitar 2FA
        user.DisableTwoFactorAuth();
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new Verify2FAResponse(true, "Two-factor authentication has been disabled successfully.");
    }
}
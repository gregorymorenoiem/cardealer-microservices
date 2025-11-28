using ErrorService.Shared.Exceptions;
using AuthService.Domain.Entities;
using RefreshTokenEntity = AuthService.Domain.Entities.RefreshToken;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using AuthService.Application.DTOs.Auth;

namespace AuthService.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtGenerator jwtGenerator,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
                   ?? throw new UnauthorizedException("Invalid credentials.");

        // Verificar que PasswordHash no sea nulo
        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials.");

        if (!user.EmailConfirmed)
            throw new UnauthorizedException("Please verify your email before logging in.");

        if (user.IsLockedOut())
            throw new UnauthorizedException("Account is temporarily locked. Please try again later.");

        // Verificar si requiere 2FA
        if (user.IsTwoFactorEnabled)
        {
            // Generar token temporal para 2FA
            var tempToken = _jwtGenerator.GenerateTempToken(user.Id);

            return new LoginResponse(
                user.Id,
                user.Email!,
                string.Empty, // No access token yet
                string.Empty, // No refresh token yet
                DateTime.UtcNow.AddMinutes(5), // Short expiration for 2FA
                true, // requiresTwoFactor
                tempToken
            );
        }

        // Flujo normal sin 2FA
        var accessToken = _jwtGenerator.GenerateToken(user);
        var refreshTokenValue = _jwtGenerator.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        var refreshTokenEntity = new RefreshTokenEntity(
            user.Id,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1"
        );

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        user.ResetAccessFailedCount();
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new LoginResponse(
            user.Id,
            user.Email!,
            accessToken,
            refreshTokenValue,
            expiresAt,
            false // requiresTwoFactor
        );
    }
}
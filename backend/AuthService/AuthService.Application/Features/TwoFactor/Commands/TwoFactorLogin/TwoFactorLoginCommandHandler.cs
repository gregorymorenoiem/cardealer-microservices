// AuthService.Application/UseCases/TwoFactorLogin/TwoFactorLoginCommandHandler.cs
using AuthService.Shared.Exceptions;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using AuthService.Domain.Enums;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.TwoFactorLogin;

public class TwoFactorLoginCommandHandler : IRequestHandler<TwoFactorLoginCommand, TwoFactorLoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITwoFactorService _twoFactorService;

    public TwoFactorLoginCommandHandler(
        IUserRepository userRepository,
        IJwtGenerator jwtGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        ITwoFactorService twoFactorService)
    {
        _userRepository = userRepository;
        _jwtGenerator = jwtGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _twoFactorService = twoFactorService;
    }

    public async Task<TwoFactorLoginResponse> Handle(TwoFactorLoginCommand request, CancellationToken cancellationToken)
    {
        // Validar token temporal
        var tempTokenData = _jwtGenerator.ValidateTempToken(request.TempToken);
        if (tempTokenData == null)
            throw new UnauthorizedException("Invalid or expired temporary token.");

        var user = await _userRepository.GetByIdAsync(tempTokenData.Value.userId)
            ?? throw new NotFoundException("User not found.");

        if (!user.IsTwoFactorEnabled)
            throw new BadRequestException("Two-factor authentication is not enabled for this user.");

        var twoFactorAuth = await _userRepository.GetTwoFactorAuthAsync(user.Id);
        if (twoFactorAuth == null)
            throw new NotFoundException("Two-factor authentication configuration not found.");

        bool isValid = false;

        // CORRECCIÓN: Usar PrimaryMethod en lugar de Type
        if (twoFactorAuth.PrimaryMethod == TwoFactorAuthType.Authenticator)
        {
            isValid = _twoFactorService.VerifyAuthenticatorCode(twoFactorAuth.Secret, request.TwoFactorCode);
        }
        else if (twoFactorAuth.PrimaryMethod == TwoFactorAuthType.SMS)
        {
            isValid = await _twoFactorService.VerifyCodeAsync(user.Id, request.TwoFactorCode, TwoFactorAuthType.SMS);
        }
        else if (twoFactorAuth.PrimaryMethod == TwoFactorAuthType.Email)
        {
            isValid = await _twoFactorService.VerifyCodeAsync(user.Id, request.TwoFactorCode, TwoFactorAuthType.Email);
        }

        if (!isValid)
        {
            // También verificar métodos secundarios si el primario falla
            foreach (var method in twoFactorAuth.EnabledMethods)
            {
                if (method == TwoFactorAuthType.Authenticator && method != twoFactorAuth.PrimaryMethod)
                {
                    isValid = _twoFactorService.VerifyAuthenticatorCode(twoFactorAuth.Secret, request.TwoFactorCode);
                    if (isValid) break;
                }
                else if (method == TwoFactorAuthType.SMS && method != twoFactorAuth.PrimaryMethod)
                {
                    isValid = await _twoFactorService.VerifyCodeAsync(user.Id, request.TwoFactorCode, TwoFactorAuthType.SMS);
                    if (isValid) break;
                }
                else if (method == TwoFactorAuthType.Email && method != twoFactorAuth.PrimaryMethod)
                {
                    isValid = await _twoFactorService.VerifyCodeAsync(user.Id, request.TwoFactorCode, TwoFactorAuthType.Email);
                    if (isValid) break;
                }
            }
        }

        // NOTE: Recovery codes are NOT accepted here.
        // Users must use POST /api/TwoFactor/login-with-recovery for recovery codes.
        // This follows industry standards (Google, GitHub, Microsoft pattern).

        if (!isValid)
        {
            twoFactorAuth.IncrementFailedAttempts();
            await _userRepository.AddOrUpdateTwoFactorAuthAsync(twoFactorAuth);
            throw new UnauthorizedException("Invalid two-factor authentication code. If you've lost access to your authenticator, use a recovery code instead.");
        }

        // Generar tokens finales
        var accessToken = _jwtGenerator.GenerateToken(user);
        var refreshTokenValue = _jwtGenerator.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        var refreshTokenEntity = new RefreshToken(
            user.Id,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1"
        );

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        user.ResetAccessFailedCount();
        twoFactorAuth.ResetFailedAttempts();
        twoFactorAuth.MarkAsUsed();
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.AddOrUpdateTwoFactorAuthAsync(twoFactorAuth);

        return new TwoFactorLoginResponse(
            user.Id,
            user.Email!,
            accessToken,
            refreshTokenValue,
            expiresAt
        );
    }
}

using AuthService.Shared.Exceptions;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using AuthService.Application.DTOs.Auth;

namespace AuthService.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtGenerator _jwtGenerator;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtGenerator jwtGenerator)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new BadRequestException("Refresh token is required.");

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken)
                          ?? throw new UnauthorizedException("Invalid refresh token.");

        if (storedToken.IsRevoked)
            throw new UnauthorizedException("Refresh token has been revoked.");

        if (storedToken.IsExpired)
            throw new UnauthorizedException("Refresh token has expired.");

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken)
                   ?? throw new NotFoundException("User not found.");

        if (user.IsLockedOut())
            throw new UnauthorizedException("Account is locked.");

        // Generate new access token
        var newAccessToken = _jwtGenerator.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        // Rotate refresh token
        var newRefreshTokenValue = _jwtGenerator.GenerateRefreshToken();

        // Revoke old token and save new one
        storedToken.Revoke("rotated", "system", newRefreshTokenValue);

        // Usar nombre completo para evitar conflicto
        var newRefreshTokenEntity = new Domain.Entities.RefreshToken(
            user.Id,
            newRefreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1" // TODO: Get actual IP from context
        );

        await _refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);
        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);

        return new RefreshTokenResponse(
            newAccessToken,
            newRefreshTokenValue,
            expiresAt
        );
    }
}

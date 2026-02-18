using AuthService.Shared.Exceptions;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using AuthService.Application.DTOs.Auth;
using AuthService.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AuthService.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IRequestContext _requestContext;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtGenerator jwtGenerator,
        IRequestContext requestContext,
        IConfiguration configuration)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtGenerator = jwtGenerator;
        _requestContext = requestContext;
        _configuration = configuration;
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

        // Read expiration from configuration instead of hardcoding
        var accessTokenMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);
        var refreshTokenDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);

        // Generate new access token
        var newAccessToken = _jwtGenerator.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(accessTokenMinutes);

        // Rotate refresh token
        var newRefreshTokenValue = _jwtGenerator.GenerateRefreshToken();

        // FIX: Correct parameter order — Revoke(revokedByIp, reason, replacedByToken)
        var clientIp = _requestContext.IpAddress ?? "unknown";
        storedToken.Revoke(clientIp, "rotated", newRefreshTokenValue);

        // Usar nombre completo para evitar conflicto
        var newRefreshTokenEntity = new Domain.Entities.RefreshToken(
            user.Id,
            newRefreshTokenValue,
            DateTime.UtcNow.AddDays(refreshTokenDays),
            _requestContext.IpAddress ?? string.Empty
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

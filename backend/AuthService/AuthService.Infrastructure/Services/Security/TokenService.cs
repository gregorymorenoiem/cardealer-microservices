using AuthService.Domain.Interfaces.Services;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Entities;
using AuthService.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AuthService.Infrastructure.Services.Security;

public class TokenService : ITokenService
{
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IDistributedCache _cache;
    private readonly ISecurityConfigProvider _securityConfig;

    public TokenService(
        IJwtGenerator jwtGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        IDistributedCache cache,
        ISecurityConfigProvider securityConfig)
    {
        _jwtGenerator = jwtGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _cache = cache;
        _securityConfig = securityConfig;
    }

    public async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(ApplicationUser user, string ipAddress)
    {
        // Read JWT expiration from ConfigurationService (admin panel: Seguridad → Expiración JWT)
        var jwtMinutes = await _securityConfig.GetJwtExpiresMinutesAsync();
        var accessToken = _jwtGenerator.GenerateToken(user, jwtMinutes);
        var refreshTokenValue = _jwtGenerator.GenerateRefreshToken();

        // Read refresh token days from ConfigurationService (admin panel: Seguridad → Vida del Refresh Token)
        var refreshTokenDays = await _securityConfig.GetRefreshTokenDaysAsync();
        var refreshTokenExpiration = TimeSpan.FromDays(refreshTokenDays);

        var refreshToken = new RefreshToken(
            user.Id,
            refreshTokenValue,
            DateTime.UtcNow.Add(refreshTokenExpiration),
            ipAddress
        );

        await _refreshTokenRepository.AddAsync(refreshToken);

        // Cache del token de acceso para validación rápida (reuse jwtMinutes from above)
        await CacheAccessTokenAsync(accessToken, user.Id, jwtMinutes);

        return (accessToken, refreshTokenValue);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string ipAddress, string reason = "revoked")
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken == null || storedToken.IsRevoked)
            return false;

        storedToken.Revoke(ipAddress, reason);
        await _refreshTokenRepository.UpdateAsync(storedToken);

        // Invalidar cache del token de acceso asociado
        await InvalidateAccessTokenCacheAsync(storedToken.UserId);

        return true;
    }

    public async Task<bool> IsAccessTokenValidAsync(string accessToken, string userId)
    {
        var cachedToken = await _cache.GetStringAsync($"access_token_{userId}");
        return cachedToken == accessToken;
    }

    private async Task CacheAccessTokenAsync(string accessToken, string userId, int jwtMinutes = 60)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(jwtMinutes)
        };

        await _cache.SetStringAsync($"access_token_{userId}", accessToken, options);
    }

    private async Task InvalidateAccessTokenCacheAsync(string userId)
    {
        await _cache.RemoveAsync($"access_token_{userId}");
    }

    public async Task CleanupExpiredTokensAsync()
    {
        await _refreshTokenRepository.CleanupExpiredTokensAsync();
    }
}

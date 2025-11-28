using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Services;
using AuthService.Domain.Interfaces.Repositories;
using MediatR;
using AuthService.Application.DTOs.ExternalAuth;
using ErrorService.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.ExternalAuth.Commands.ExternalAuth;

public class ExternalAuthCommandHandler : IRequestHandler<ExternalAuthCommand, ExternalAuthResponse>
{
    private readonly IExternalAuthService _externalAuthService;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<ExternalAuthCommandHandler> _logger;

    public ExternalAuthCommandHandler(
        IExternalAuthService externalAuthService,
        IJwtGenerator jwtGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<ExternalAuthCommandHandler> logger)
    {
        _externalAuthService = externalAuthService;
        _jwtGenerator = jwtGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<ExternalAuthResponse> Handle(ExternalAuthCommand request, CancellationToken cancellationToken)
    {
        // Validar provider
        if (!Enum.TryParse<ExternalAuthProvider>(request.Provider, true, out var provider))
            throw new BadRequestException($"Unsupported provider: {request.Provider}");

        // Autenticar con proveedor externo
        var (user, isNewUser) = await _externalAuthService.AuthenticateAsync(provider, request.IdToken);

        if (user.IsLockedOut())
            throw new UnauthorizedException("Account is temporarily locked. Please try again later.");

        // Generar tokens
        var accessToken = _jwtGenerator.GenerateToken(user);
        var refreshTokenValue = _jwtGenerator.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        var refreshTokenEntity = new Domain.Entities.RefreshToken(
            user.Id,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1" // TODO: Get actual IP from context
        );

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        _logger.LogInformation("External authentication successful for user {Email} with provider {Provider}",
            user.Email, provider);

        return new ExternalAuthResponse(
            user.Id,
            user.UserName!,
            user.Email!,
            accessToken,
            refreshTokenValue,
            expiresAt,
            isNewUser
        );
    }
}
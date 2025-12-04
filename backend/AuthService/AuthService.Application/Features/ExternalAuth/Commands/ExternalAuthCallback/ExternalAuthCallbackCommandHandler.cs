using AuthService.Application.DTOs.ExternalAuth;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Services;
using AuthService.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using AuthService.Application.Common.Interfaces;

namespace AuthService.Application.Features.ExternalAuth.Commands.ExternalAuthCallback;

public class ExternalAuthCallbackCommandHandler : IRequestHandler<ExternalAuthCallbackCommand, ExternalAuthResponse>
{
    private readonly IExternalAuthService _externalAuthService;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<ExternalAuthCallbackCommandHandler> _logger;
    private readonly IRequestContext _requestContext;

    public ExternalAuthCallbackCommandHandler(
        IExternalAuthService externalAuthService,
        IJwtGenerator jwtGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<ExternalAuthCallbackCommandHandler> logger,
        IRequestContext requestContext)
    {
        _externalAuthService = externalAuthService;
        _jwtGenerator = jwtGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
        _requestContext = requestContext;
    }

    public async Task<ExternalAuthResponse> Handle(ExternalAuthCallbackCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider
            if (!Enum.TryParse<ExternalAuthProvider>(request.Provider, true, out var provider))
                throw new ArgumentException($"Unsupported provider: {request.Provider}");

            string idToken;

            // If we have a code, we need to exchange it for an ID token
            if (!string.IsNullOrEmpty(request.Code))
            {
                idToken = await ExchangeCodeForIdToken(provider, request.Code, request.RedirectUri);
            }
            else if (!string.IsNullOrEmpty(request.IdToken))
            {
                idToken = request.IdToken;
            }
            else
            {
                throw new ArgumentException("Either Code or IdToken must be provided");
            }

            // Authenticate with external provider
            var (user, isNewUser) = await _externalAuthService.AuthenticateAsync(provider, idToken);

            // Generate tokens
            var accessToken = _jwtGenerator.GenerateToken(user);
            var refreshTokenValue = _jwtGenerator.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(60);

            var refreshTokenEntity = new Domain.Entities.RefreshToken(
                user.Id,
                refreshTokenValue,
                DateTime.UtcNow.AddDays(7),
                _requestContext.IpAddress
            );

            await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

            _logger.LogInformation("External auth callback processed successfully for user {UserId} from IP {IpAddress}",
                user.Id, _requestContext.IpAddress);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing external auth callback for provider {Provider}",
                request.Provider);
            throw;
        }
    }

    private async Task<string> ExchangeCodeForIdToken(ExternalAuthProvider provider, string code, string? redirectUri)
    {
        // This would make a server-side call to the provider's token endpoint
        // The actual implementation depends on the OAuth provider configuration
        _logger.LogInformation("Exchanging code for ID token for provider {Provider}", provider);

        // For now, throw not implemented - this should be configured per provider
        await Task.CompletedTask;
        throw new NotImplementedException(
            $"OAuth code exchange for provider {provider} requires provider-specific configuration. " +
            "Please implement the token exchange logic for your OAuth provider.");
    }
}

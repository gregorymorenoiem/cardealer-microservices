using AuthService.Application.DTOs.ExternalAuth;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using AuthService.Shared.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.ExternalAuth.Commands.LinkExternalAccount;

public class LinkExternalAccountCommandHandler : IRequestHandler<LinkExternalAccountCommand, ExternalAuthResponse>
{
    private readonly IExternalAuthService _externalAuthService;
    private readonly IUserRepository _userRepository;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<LinkExternalAccountCommandHandler> _logger;

    public LinkExternalAccountCommandHandler(
        IExternalAuthService externalAuthService,
        IUserRepository userRepository,
        IJwtGenerator jwtGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<LinkExternalAccountCommandHandler> logger)
    {
        _externalAuthService = externalAuthService;
        _userRepository = userRepository;
        _jwtGenerator = jwtGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<ExternalAuthResponse> Handle(LinkExternalAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider
            if (!Enum.TryParse<ExternalAuthProvider>(request.Provider, true, out var provider))
                throw new BadRequestException($"Unsupported provider: {request.Provider}");

            // Get user
            var user = await _userRepository.GetByIdAsync(request.UserId)
                ?? throw new NotFoundException("User not found.");

            // Check if user already has an external account
            if (user.IsExternalUser)
                throw new BadRequestException("User already has an external account linked.");

            // Authenticate with external provider
            var (externalUser, isNewUser) = await _externalAuthService.AuthenticateAsync(provider, request.IdToken);

            // Link the external account to existing user
            user.LinkExternalAccount(provider, externalUser.ExternalUserId!);

            // Update user
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Generate tokens
            var accessToken = _jwtGenerator.GenerateToken(user);
            var refreshTokenValue = _jwtGenerator.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(60);

            var refreshTokenEntity = new RefreshToken(
                user.Id,
                refreshTokenValue,
                DateTime.UtcNow.AddDays(7),
                "127.0.0.1" // TODO: Get actual IP from context
            );

            await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

            _logger.LogInformation("External account linked successfully for user {UserId} with provider {Provider}",
                request.UserId, request.Provider);

            return new ExternalAuthResponse(
                user.Id,
                user.UserName!,
                user.Email!,
                accessToken,
                refreshTokenValue,
                expiresAt,
                false // isNewUser should be false since we're linking to existing account
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking external account for user {UserId}", request.UserId);
            throw;
        }
    }
}

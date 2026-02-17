using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Shared.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.ExternalAuth.Commands.ValidateUnlinkAccount;

/// <summary>
/// Handler for ValidateUnlinkAccountCommand (AUTH-EXT-008)
/// 
/// Determines what flow the user needs to follow to unlink their OAuth provider:
/// 1. If no password → Requires password setup first (AUTH-PWD-001)
/// 2. If active provider → Requires email verification code
/// 3. Otherwise → Can use simple unlink (AUTH-EXT-006)
/// </summary>
public class ValidateUnlinkAccountCommandHandler : IRequestHandler<ValidateUnlinkAccountCommand, ValidateUnlinkAccountResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ValidateUnlinkAccountCommandHandler> _logger;

    public ValidateUnlinkAccountCommandHandler(
        IUserRepository userRepository,
        ILogger<ValidateUnlinkAccountCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ValidateUnlinkAccountResponse> Handle(ValidateUnlinkAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider
            if (!Enum.TryParse<ExternalAuthProvider>(request.Provider, true, out var provider))
            {
                throw new BadRequestException($"Unsupported provider: {request.Provider}");
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(request.UserId)
                ?? throw new NotFoundException("User not found.");

            // Check if user has this provider linked
            if (!user.IsExternalUser || user.ExternalAuthProvider != provider)
            {
                throw new BadRequestException($"You don't have a {request.Provider} account linked.");
            }

            var hasPassword = !string.IsNullOrEmpty(user.PasswordHash);
            var isActiveProvider = user.IsExternalUser && user.ExternalAuthProvider == provider;

            // Case 1: No password - must set password first
            if (!hasPassword)
            {
                _logger.LogInformation(
                    "User {UserId} needs to set password before unlinking {Provider}",
                    request.UserId, request.Provider);

                return new ValidateUnlinkAccountResponse(
                    CanUnlink: false,
                    HasPassword: false,
                    IsActiveProvider: isActiveProvider,
                    RequiresPasswordSetup: true,
                    RequiresEmailVerification: false,
                    Message: "You need to set a password before unlinking your OAuth account. " +
                             "This ensures you won't lose access to your account."
                );
            }

            // Case 2: Has password but is active provider - requires email verification
            if (isActiveProvider)
            {
                _logger.LogInformation(
                    "User {UserId} can unlink active provider {Provider} with email verification",
                    request.UserId, request.Provider);

                return new ValidateUnlinkAccountResponse(
                    CanUnlink: true,
                    HasPassword: true,
                    IsActiveProvider: true,
                    RequiresPasswordSetup: false,
                    RequiresEmailVerification: true,
                    Message: "To unlink your active login provider, we'll send a verification code to your email. " +
                             "All your sessions will be closed after unlinking."
                );
            }

            // Case 3: Has password and not active provider - simple unlink
            _logger.LogInformation(
                "User {UserId} can unlink {Provider} without additional verification",
                request.UserId, request.Provider);

            return new ValidateUnlinkAccountResponse(
                CanUnlink: true,
                HasPassword: true,
                IsActiveProvider: false,
                RequiresPasswordSetup: false,
                RequiresEmailVerification: false,
                Message: "You can unlink this account. You'll continue using your current login method."
            );
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating unlink account for user {UserId}", request.UserId);
            throw new ApplicationException("An error occurred while validating the request. Please try again.", ex);
        }
    }
}

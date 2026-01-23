using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Shared.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.ExternalAuth.Commands.UnlinkExternalAccount;

/// <summary>
/// Handler for UnlinkExternalAccountCommand (AUTH-EXT-006)
/// 
/// Security considerations:
/// - User must have a password set before unlinking (otherwise they'll be locked out)
/// - Only the linked provider can be unlinked
/// - Audit logging for security tracking
/// </summary>
public class UnlinkExternalAccountCommandHandler : IRequestHandler<UnlinkExternalAccountCommand, UnlinkExternalAccountResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UnlinkExternalAccountCommandHandler> _logger;

    public UnlinkExternalAccountCommandHandler(
        IUserRepository userRepository,
        ILogger<UnlinkExternalAccountCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UnlinkExternalAccountResponse> Handle(UnlinkExternalAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider enum
            if (!Enum.TryParse<ExternalAuthProvider>(request.Provider, true, out var provider))
                throw new BadRequestException($"Unsupported provider: {request.Provider}");

            // Get user
            var user = await _userRepository.GetByIdAsync(request.UserId)
                ?? throw new NotFoundException("User not found.");

            // Check if user has an external account from this provider
            if (!user.IsExternalUser || user.ExternalAuthProvider != provider)
                throw new BadRequestException($"User does not have a linked {request.Provider} account.");

            // SECURITY CHECK: Verify user has password set before unlinking
            // If user only has external auth and no password, they would be locked out
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new BadRequestException(
                    "Cannot unlink external account: You must set a password first. " +
                    "Go to Security Settings and set a password before unlinking your external account.");
            }

            // Store provider info for response before unlinking
            var unlinkedProvider = user.ExternalAuthProvider!.Value.ToString();
            var unlinkedExternalUserId = user.ExternalUserId;

            // Unlink the external account using the entity method
            user.UnlinkExternalAccount();

            // Update user
            await _userRepository.UpdateAsync(user, cancellationToken);

            _logger.LogInformation(
                "External account unlinked successfully. UserId: {UserId}, Provider: {Provider}, ExternalUserId: {ExternalUserId}",
                request.UserId, request.Provider, unlinkedExternalUserId);

            // TODO: Publish domain event for audit (ExternalAccountUnlinkedEvent)
            // This should trigger:
            // 1. Security alert email to user
            // 2. Audit log entry
            // 3. Analytics tracking

            return new UnlinkExternalAccountResponse(
                Success: true,
                Message: $"Successfully unlinked {unlinkedProvider} account",
                Provider: unlinkedProvider,
                UnlinkedAt: DateTime.UtcNow
            );
        }
        catch (BadRequestException)
        {
            throw; // Re-throw validation errors as-is
        }
        catch (NotFoundException)
        {
            throw; // Re-throw not found errors as-is
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking external account for user {UserId}", request.UserId);
            throw new ApplicationException("An error occurred while unlinking the external account. Please try again.", ex);
        }
    }
}

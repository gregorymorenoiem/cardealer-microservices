using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Shared.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.ExternalAuth.Commands.UnlinkExternalAccount;

public class UnlinkExternalAccountCommandHandler : IRequestHandler<UnlinkExternalAccountCommand, Unit>
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

    public async Task<Unit> Handle(UnlinkExternalAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider
            if (!Enum.TryParse<ExternalAuthProvider>(request.Provider, true, out var provider))
                throw new BadRequestException($"Unsupported provider: {request.Provider}");

            // Get user
            var user = await _userRepository.GetByIdAsync(request.UserId)
                ?? throw new NotFoundException("User not found.");

            // Check if user has an external account from this provider
            if (!user.IsExternalUser || user.ExternalAuthProvider != provider)
                throw new BadRequestException($"User does not have a linked {request.Provider} account.");

            // Unlink the external account using the entity method
            user.UnlinkExternalAccount();

            // Update user
            await _userRepository.UpdateAsync(user, cancellationToken);

            _logger.LogInformation("External account unlinked successfully for user {UserId} with provider {Provider}",
                request.UserId, request.Provider);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking external account for user {UserId}", request.UserId);
            throw;
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UserService.Domain.Entities.Privacy;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.Privacy.CancelAccountDeletion;

/// <summary>
/// Command para cancelar solicitud de eliminación de cuenta
/// </summary>
public record CancelAccountDeletionCommand(
    Guid UserId,
    string? Email = null
) : IRequest<bool>;

/// <summary>
/// Handler para CancelAccountDeletionCommand
/// </summary>
public class CancelAccountDeletionCommandHandler : IRequestHandler<CancelAccountDeletionCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPrivacyRequestRepository _privacyRepository;

    public CancelAccountDeletionCommandHandler(
        IUserRepository userRepository,
        IPrivacyRequestRepository privacyRepository)
    {
        _userRepository = userRepository;
        _privacyRepository = privacyRepository;
    }

    public async Task<bool> Handle(CancelAccountDeletionCommand request, CancellationToken cancellationToken)
    {
        // 1. Resolve user
        Domain.Entities.User? user = null;
        if (!string.IsNullOrEmpty(request.Email))
            user = await _userRepository.GetByEmailAsync(request.Email);
        user ??= await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado.");

        // 2. Find active deletion request (Pending or Processing = confirmed but in grace period)
        var privacyRequest = await _privacyRepository.GetPendingDeletionRequestAsync(user.Id);

        if (privacyRequest == null)
            return false; // No deletion request to cancel — idempotent

        // 3. Ensure still within cancellable window (grace period has not ended)
        if (privacyRequest.GracePeriodEndsAt.HasValue && privacyRequest.GracePeriodEndsAt.Value < DateTime.UtcNow)
            throw new InvalidOperationException("El período de gracia ha finalizado. La eliminación ya no puede ser cancelada.");

        // 4. Cancel the request
        privacyRequest.Status = PrivacyRequestStatus.Cancelled;
        privacyRequest.CompletedAt = DateTime.UtcNow;

        await _privacyRepository.UpdateAsync(privacyRequest);
        return true;
    }
}

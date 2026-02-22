using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UserService.Application.DTOs.Privacy;
using UserService.Domain.Entities.Privacy;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.Privacy.GetAccountDeletionStatus;

/// <summary>
/// Query para obtener estado de solicitud de eliminación
/// </summary>
public record GetAccountDeletionStatusQuery(
    Guid UserId,
    string? Email = null
) : IRequest<AccountDeletionStatusDto?>;

/// <summary>
/// Handler para GetAccountDeletionStatusQuery
/// </summary>
public class GetAccountDeletionStatusQueryHandler : IRequestHandler<GetAccountDeletionStatusQuery, AccountDeletionStatusDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly IPrivacyRequestRepository _privacyRepository;

    public GetAccountDeletionStatusQueryHandler(
        IUserRepository userRepository,
        IPrivacyRequestRepository privacyRepository)
    {
        _userRepository = userRepository;
        _privacyRepository = privacyRepository;
    }

    public async Task<AccountDeletionStatusDto?> Handle(GetAccountDeletionStatusQuery request, CancellationToken cancellationToken)
    {
        // 1. Resolve user
        Domain.Entities.User? user = null;
        if (!string.IsNullOrEmpty(request.Email))
            user = await _userRepository.GetByEmailAsync(request.Email);
        user ??= await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
            return null;

        // 2. Try to get the active deletion request (Pending or Processing)
        var privacyRequest = await _privacyRepository.GetPendingDeletionRequestAsync(user.Id);

        // 3. Fall back to most recent deletion request regardless of status
        if (privacyRequest == null)
        {
            var allRequests = await _privacyRepository.GetByUserIdAsync(user.Id);
            privacyRequest = allRequests
                .Where(r => r.Type == PrivacyRequestType.Cancellation)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();
        }

        if (privacyRequest == null)
            return null;

        var gracePeriodEndsAt = privacyRequest.GracePeriodEndsAt ?? DateTime.UtcNow.AddDays(15);
        var daysRemaining = Math.Max(0, (int)(gracePeriodEndsAt - DateTime.UtcNow).TotalDays);

        // CanCancel: still in grace period and not already cancelled/completed
        var canCancel = gracePeriodEndsAt > DateTime.UtcNow
            && privacyRequest.Status != PrivacyRequestStatus.Cancelled
            && privacyRequest.Status != PrivacyRequestStatus.Completed;

        return new AccountDeletionStatusDto(
            RequestId: privacyRequest.Id,
            Status: privacyRequest.Status.ToString(),
            RequestedAt: privacyRequest.CreatedAt,
            GracePeriodEndsAt: gracePeriodEndsAt,
            CanCancel: canCancel,
            DaysRemaining: daysRemaining,
            Reason: privacyRequest.DeletionReason
        );
    }
}

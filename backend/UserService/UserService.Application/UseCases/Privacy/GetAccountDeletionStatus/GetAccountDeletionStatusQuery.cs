using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UserService.Application.DTOs.Privacy;

namespace UserService.Application.UseCases.Privacy.GetAccountDeletionStatus;

/// <summary>
/// Query para obtener estado de solicitud de eliminación
/// </summary>
public record GetAccountDeletionStatusQuery(Guid UserId) : IRequest<AccountDeletionStatusDto?>;

/// <summary>
/// Handler para GetAccountDeletionStatusQuery
/// </summary>
public class GetAccountDeletionStatusQueryHandler : IRequestHandler<GetAccountDeletionStatusQuery, AccountDeletionStatusDto?>
{
    public async Task<AccountDeletionStatusDto?> Handle(GetAccountDeletionStatusQuery request, CancellationToken cancellationToken)
    {
        // TODO: Buscar solicitud pendiente en DB
        await Task.CompletedTask;
        
        // Retornar null si no hay solicitud pendiente
        // Por ahora retornamos ejemplo
        var gracePeriodEnds = DateTime.UtcNow.AddDays(10);
        var daysRemaining = (int)(gracePeriodEnds - DateTime.UtcNow).TotalDays;
        
        return new AccountDeletionStatusDto(
            RequestId: Guid.NewGuid(),
            Status: "Pending",
            RequestedAt: DateTime.UtcNow.AddDays(-5),
            GracePeriodEndsAt: gracePeriodEnds,
            CanCancel: true,
            DaysRemaining: daysRemaining,
            Reason: "NoLongerNeeded"
        );
    }
}

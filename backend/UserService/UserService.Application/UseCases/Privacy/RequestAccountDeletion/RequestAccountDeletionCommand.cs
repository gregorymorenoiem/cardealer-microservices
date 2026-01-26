using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UserService.Application.DTOs.Privacy;

namespace UserService.Application.UseCases.Privacy.RequestAccountDeletion;

/// <summary>
/// Command para solicitar eliminación de cuenta (Cancelación ARCO)
/// </summary>
public record RequestAccountDeletionCommand(
    Guid UserId,
    DeletionReason Reason,
    string? OtherReason,
    string? Feedback,
    string? IpAddress,
    string? UserAgent
) : IRequest<AccountDeletionResponseDto>;

/// <summary>
/// Handler para RequestAccountDeletionCommand
/// </summary>
public class RequestAccountDeletionCommandHandler : IRequestHandler<RequestAccountDeletionCommand, AccountDeletionResponseDto>
{
    public async Task<AccountDeletionResponseDto> Handle(RequestAccountDeletionCommand request, CancellationToken cancellationToken)
    {
        // TODO: Implementar lógica real
        await Task.CompletedTask;
        
        var requestId = Guid.NewGuid();
        var gracePeriodEnds = DateTime.UtcNow.AddDays(15); // 15 días de gracia según Ley 172-13
        
        // 1. Crear PrivacyRequest con Type=Cancellation
        // 2. Generar código de confirmación
        // 3. Enviar email de confirmación
        // 4. Programar job para ejecutar después del período de gracia
        
        return new AccountDeletionResponseDto(
            RequestId: requestId,
            Status: "PendingConfirmation",
            Message: "Hemos enviado un código de confirmación a tu email. Tu cuenta será eliminada en 15 días si no cancelas la solicitud.",
            GracePeriodEndsAt: gracePeriodEnds,
            ConfirmationEmailSentTo: "demo@okla.com.do" // TODO: Obtener email real
        );
    }
}

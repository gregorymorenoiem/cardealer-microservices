using MediatR;

namespace StripePaymentService.Application.Features.Subscription.Commands;

/// <summary>
/// Comando para cancelar una subscripción
/// </summary>
public class CancelSubscriptionCommand : IRequest<bool>
{
    public Guid SubscriptionId { get; set; }
    public string? CancellationReason { get; set; }

    public CancelSubscriptionCommand(Guid subscriptionId, string? cancellationReason = null)
    {
        SubscriptionId = subscriptionId;
        CancellationReason = cancellationReason;
    }
}

using MediatR;
using AzulPaymentService.Application.DTOs;

namespace AzulPaymentService.Application.Features.Subscription.Commands;

/// <summary>
/// Comando para crear una suscripción recurrente
/// </summary>
public class CreateSubscriptionCommand : IRequest<SubscriptionResponseDto>
{
    /// <summary>
    /// DTO con los datos de la suscripción
    /// </summary>
    public SubscriptionRequestDto SubscriptionRequest { get; set; } = null!;

    public CreateSubscriptionCommand(SubscriptionRequestDto subscriptionRequest)
    {
        SubscriptionRequest = subscriptionRequest;
    }
}

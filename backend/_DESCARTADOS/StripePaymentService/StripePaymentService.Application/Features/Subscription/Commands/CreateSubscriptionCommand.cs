using MediatR;
using StripePaymentService.Application.DTOs;

namespace StripePaymentService.Application.Features.Subscription.Commands;

/// <summary>
/// Comando para crear una subscripción en Stripe
/// </summary>
public class CreateSubscriptionCommand : IRequest<SubscriptionResponseDto>
{
    public CreateSubscriptionRequestDto Request { get; set; }

    public CreateSubscriptionCommand(CreateSubscriptionRequestDto request)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }
}

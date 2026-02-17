using MediatR;
using StripePaymentService.Application.DTOs;

namespace StripePaymentService.Application.Features.PaymentIntent.Queries;

/// <summary>
/// Query para obtener un Payment Intent por ID
/// </summary>
public class GetPaymentIntentQuery : IRequest<PaymentIntentResponseDto?>
{
    public Guid PaymentIntentId { get; set; }

    public GetPaymentIntentQuery(Guid paymentIntentId)
    {
        PaymentIntentId = paymentIntentId;
    }
}

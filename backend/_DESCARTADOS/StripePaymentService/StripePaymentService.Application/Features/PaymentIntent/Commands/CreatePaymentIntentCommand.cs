using MediatR;
using StripePaymentService.Application.DTOs;

namespace StripePaymentService.Application.Features.PaymentIntent.Commands;

/// <summary>
/// Comando para crear un Payment Intent
/// </summary>
public class CreatePaymentIntentCommand : IRequest<PaymentIntentResponseDto>
{
    public CreatePaymentIntentRequestDto Request { get; set; } = null!;

    public CreatePaymentIntentCommand(CreatePaymentIntentRequestDto request)
    {
        Request = request;
    }
}

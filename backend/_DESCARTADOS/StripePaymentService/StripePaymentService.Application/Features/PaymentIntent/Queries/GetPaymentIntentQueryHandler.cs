using MediatR;
using Microsoft.Extensions.Logging;
using StripePaymentService.Application.DTOs;
using StripePaymentService.Domain.Interfaces;

namespace StripePaymentService.Application.Features.PaymentIntent.Queries;

/// <summary>
/// Handler para obtener Payment Intent
/// </summary>
public class GetPaymentIntentQueryHandler : IRequestHandler<GetPaymentIntentQuery, PaymentIntentResponseDto?>
{
    private readonly IStripePaymentIntentRepository _repository;
    private readonly ILogger<GetPaymentIntentQueryHandler> _logger;

    public GetPaymentIntentQueryHandler(
        IStripePaymentIntentRepository repository,
        ILogger<GetPaymentIntentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PaymentIntentResponseDto?> Handle(GetPaymentIntentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo Payment Intent: {PaymentIntentId}", request.PaymentIntentId);

        try
        {
            var paymentIntent = await _repository.GetByIdAsync(request.PaymentIntentId, cancellationToken);
            if (paymentIntent == null)
            {
                _logger.LogWarning("Payment Intent no encontrado: {PaymentIntentId}", request.PaymentIntentId);
                return null;
            }

            return new PaymentIntentResponseDto
            {
                PaymentIntentId = paymentIntent.Id,
                StripePaymentIntentId = paymentIntent.StripePaymentIntentId,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency,
                ClientSecret = paymentIntent.ClientSecret,
                RequiresAction = paymentIntent.RequiresAction,
                CreatedAt = paymentIntent.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo Payment Intent: {PaymentIntentId}", request.PaymentIntentId);
            throw;
        }
    }
}

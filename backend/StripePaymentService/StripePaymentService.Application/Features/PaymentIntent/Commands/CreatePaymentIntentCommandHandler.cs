using MediatR;
using Microsoft.Extensions.Logging;
using StripePaymentService.Application.DTOs;
using StripePaymentService.Domain.Interfaces;
using StripePaymentService.Domain.Entities;

namespace StripePaymentService.Application.Features.PaymentIntent.Commands;

/// <summary>
/// Handler para crear Payment Intent
/// </summary>
public class CreatePaymentIntentCommandHandler : IRequestHandler<CreatePaymentIntentCommand, PaymentIntentResponseDto>
{
    private readonly IStripePaymentIntentRepository _repository;
    private readonly ILogger<CreatePaymentIntentCommandHandler> _logger;

    public CreatePaymentIntentCommandHandler(
        IStripePaymentIntentRepository repository,
        ILogger<CreatePaymentIntentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PaymentIntentResponseDto> Handle(CreatePaymentIntentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creando Payment Intent. Amount: {Amount}", request.Request.Amount);

        try
        {
            var paymentIntent = new StripePaymentIntent
            {
                Id = Guid.NewGuid(),
                Amount = request.Request.Amount,
                Currency = request.Request.Currency,
                Status = "requires_payment_method",
                Description = request.Request.Description,
                CustomerEmail = request.Request.CustomerEmail,
                CustomerName = request.Request.CustomerName,
                CustomerPhone = request.Request.CustomerPhone,
                Metadata = request.Request.Metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // TODO: Llamar a Stripe API
            paymentIntent.StripePaymentIntentId = Guid.NewGuid().ToString("N");
            paymentIntent.ClientSecret = $"pi_{Guid.NewGuid().ToString("N").Substring(0, 20)}_secret_{Guid.NewGuid().ToString("N").Substring(0, 20)}";

            await _repository.CreateAsync(paymentIntent, cancellationToken);

            return new PaymentIntentResponseDto
            {
                PaymentIntentId = paymentIntent.Id,
                StripePaymentIntentId = paymentIntent.StripePaymentIntentId,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency,
                ClientSecret = paymentIntent.ClientSecret,
                CreatedAt = paymentIntent.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando Payment Intent");
            throw;
        }
    }
}

using MediatR;
using Microsoft.Extensions.Logging;
using StripePaymentService.Application.DTOs;
using StripePaymentService.Domain.Interfaces;
using StripePaymentService.Domain.Entities;

namespace StripePaymentService.Application.Features.Subscription.Commands;

/// <summary>
/// Handler para crear subscripción
/// </summary>
public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionResponseDto>
{
    private readonly IStripeSubscriptionRepository _repository;
    private readonly IStripeCustomerRepository _customerRepository;
    private readonly ILogger<CreateSubscriptionCommandHandler> _logger;

    public CreateSubscriptionCommandHandler(
        IStripeSubscriptionRepository repository,
        IStripeCustomerRepository customerRepository,
        ILogger<CreateSubscriptionCommandHandler> logger)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<SubscriptionResponseDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creando Subscripción para Customer: {CustomerId}", request.Request.StripeCustomerId);

        try
        {
            // Obtener customer
            if (!Guid.TryParse(request.Request.StripeCustomerId, out var customerId))
            {
                _logger.LogWarning("ID de Customer inválido: {CustomerId}", request.Request.StripeCustomerId);
                throw new InvalidOperationException($"ID de Customer inválido: {request.Request.StripeCustomerId}");
            }

            var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
            if (customer == null)
            {
                _logger.LogWarning("Customer no encontrado: {CustomerId}", request.Request.StripeCustomerId);
                throw new InvalidOperationException($"Customer {request.Request.StripeCustomerId} no encontrado");
            }

            var subscription = new StripeSubscription
            {
                Id = Guid.NewGuid(),
                StripeCustomerId = customer.Id,
                StripePriceId = request.Request.StripePriceId,
                Currency = "usd",
                Status = "active",
                StartDate = request.Request.StartDate ?? DateTime.UtcNow,
                NextBillingDate = request.Request.StartDate ?? DateTime.UtcNow.AddMonths(1),
                Amount = 0, // TODO: Get from Stripe Price
                BillingInterval = "month",
                Metadata = request.Request.Metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // TODO: Llamar a Stripe API para crear subscription
            subscription.StripeSubscriptionId = Guid.NewGuid().ToString("N");

            await _repository.CreateAsync(subscription, cancellationToken);

            return new SubscriptionResponseDto
            {
                SubscriptionId = subscription.Id,
                StripeSubscriptionId = subscription.StripeSubscriptionId,
                StripeCustomerId = customer.Id.ToString(),
                Status = subscription.Status,
                Amount = subscription.Amount,
                Currency = subscription.Currency,
                BillingInterval = subscription.BillingInterval,
                StartDate = subscription.StartDate,
                NextBillingDate = subscription.NextBillingDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando subscripción");
            throw;
        }
    }
}

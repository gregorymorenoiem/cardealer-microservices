using MediatR;
using Serilog;
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
        _logger.Information("Creando Subscripción para Customer: {CustomerId}", request.Request.CustomerId);

        try
        {
            // Obtener customer
            var customer = await _customerRepository.GetByIdAsync(request.Request.CustomerId, cancellationToken);
            if (customer == null)
            {
                _logger.Warning("Customer no encontrado: {CustomerId}", request.Request.CustomerId);
                throw new InvalidOperationException($"Customer {request.Request.CustomerId} no encontrado");
            }

            var subscription = new StripeSubscription
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                Currency = request.Request.Currency ?? "USD",
                Status = "active",
                BillingCycleAnchor = request.Request.StartDate ?? DateTime.UtcNow.AddDays(request.Request.TrialDays ?? 0),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TrialDays = request.Request.TrialDays ?? 0
            };

            // TODO: Llamar a Stripe API para crear subscription
            subscription.StripeSubscriptionId = Guid.NewGuid().ToString("N");

            await _repository.CreateAsync(subscription, cancellationToken);

            return new SubscriptionResponseDto
            {
                SubscriptionId = subscription.Id,
                StripeSubscriptionId = subscription.StripeSubscriptionId,
                CustomerId = customer.Id,
                Status = subscription.Status,
                Currency = subscription.Currency,
                BillingCycleAnchor = subscription.BillingCycleAnchor,
                CreatedAt = subscription.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creando subscripción");
            throw;
        }
    }
}

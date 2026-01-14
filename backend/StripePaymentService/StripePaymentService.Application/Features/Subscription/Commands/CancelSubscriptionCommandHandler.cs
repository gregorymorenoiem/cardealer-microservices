using MediatR;
using Serilog;
using StripePaymentService.Domain.Interfaces;

namespace StripePaymentService.Application.Features.Subscription.Commands;

/// <summary>
/// Handler para cancelar subscripción
/// </summary>
public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, bool>
{
    private readonly IStripeSubscriptionRepository _repository;
    private readonly ILogger<CancelSubscriptionCommandHandler> _logger;

    public CancelSubscriptionCommandHandler(
        IStripeSubscriptionRepository repository,
        ILogger<CancelSubscriptionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("Cancelando subscripción: {SubscriptionId}, Razón: {Reason}",
            request.SubscriptionId, request.CancellationReason ?? "No especificada");

        try
        {
            var subscription = await _repository.GetByIdAsync(request.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.Warning("Subscripción no encontrada: {SubscriptionId}", request.SubscriptionId);
                throw new InvalidOperationException($"Subscripción {request.SubscriptionId} no encontrada");
            }

            // TODO: Llamar a Stripe API para cancelar
            subscription.Status = "canceled";
            subscription.CanceledAt = DateTime.UtcNow;
            subscription.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(subscription, cancellationToken);

            _logger.Information("Subscripción cancelada exitosamente: {SubscriptionId}", request.SubscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error cancelando subscripción: {SubscriptionId}", request.SubscriptionId);
            throw;
        }
    }
}

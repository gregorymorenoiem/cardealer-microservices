using MediatR;
using Serilog;
using StripePaymentService.Application.DTOs;
using StripePaymentService.Domain.Interfaces;

namespace StripePaymentService.Application.Features.Subscription.Queries;

/// <summary>
/// Handler para listar subscripciones
/// </summary>
public class ListSubscriptionsQueryHandler : IRequestHandler<ListSubscriptionsQuery, List<SubscriptionResponseDto>>
{
    private readonly IStripeSubscriptionRepository _repository;
    private readonly ILogger<ListSubscriptionsQueryHandler> _logger;

    public ListSubscriptionsQueryHandler(
        IStripeSubscriptionRepository repository,
        ILogger<ListSubscriptionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<SubscriptionResponseDto>> Handle(ListSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        _logger.Information("Listando subscripciones del customer: {CustomerId}, Page: {Page}, PageSize: {PageSize}",
            request.CustomerId, request.Page, request.PageSize);

        try
        {
            var subscriptions = await _repository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

            var results = subscriptions
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new SubscriptionResponseDto
                {
                    SubscriptionId = s.Id,
                    StripeSubscriptionId = s.StripeSubscriptionId,
                    CustomerId = s.CustomerId,
                    Status = s.Status,
                    Currency = s.Currency,
                    Amount = s.Amount,
                    BillingCycleAnchor = s.BillingCycleAnchor,
                    CreatedAt = s.CreatedAt,
                    CanceledAt = s.CanceledAt
                })
                .ToList();

            _logger.Information("Se retornaron {Count} subscripciones", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error listando subscripciones del customer: {CustomerId}", request.CustomerId);
            throw;
        }
    }
}

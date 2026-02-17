using MediatR;
using StripePaymentService.Application.DTOs;

namespace StripePaymentService.Application.Features.Subscription.Queries;

/// <summary>
/// Query para listar subscripciones de un customer
/// </summary>
public class ListSubscriptionsQuery : IRequest<List<SubscriptionResponseDto>>
{
    public Guid CustomerId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public ListSubscriptionsQuery(Guid customerId, int page = 1, int pageSize = 10)
    {
        CustomerId = customerId;
        Page = page;
        PageSize = pageSize;
    }
}

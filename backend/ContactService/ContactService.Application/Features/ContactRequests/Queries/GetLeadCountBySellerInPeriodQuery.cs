using ContactService.Domain.Interfaces;
using MediatR;

namespace ContactService.Application.Features.ContactRequests.Queries;

/// <summary>
/// Internal query: counts leads (contact requests) for a seller in a date range.
/// Used by NotificationService to detect upsell-eligible LIBRE-plan dealers (5+ leads in 30 days).
/// </summary>
public record GetLeadCountBySellerInPeriodQuery : IRequest<int>
{
    public Guid SellerId { get; init; }
    public DateTime From { get; init; }
    public DateTime To { get; init; }
}

public class GetLeadCountBySellerInPeriodQueryHandler
    : IRequestHandler<GetLeadCountBySellerInPeriodQuery, int>
{
    private readonly IContactRequestRepository _repository;

    public GetLeadCountBySellerInPeriodQueryHandler(IContactRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(GetLeadCountBySellerInPeriodQuery request, CancellationToken cancellationToken)
    {
        return await _repository.CountBySellerIdInPeriodAsync(
            request.SellerId, request.From, request.To, cancellationToken);
    }
}

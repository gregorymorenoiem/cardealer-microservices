using ContactService.Domain.Interfaces;
using MediatR;

namespace ContactService.Application.Features.ContactRequests.Queries;

/// <summary>
/// Internal query: checks if a buyer has ever contacted a specific seller.
/// Used by ReviewService to verify that only buyers who had real contact
/// with a dealer/seller can leave a review — prevents fake reviews.
/// </summary>
public record HasBuyerContactedSellerQuery : IRequest<bool>
{
    public Guid BuyerId { get; init; }
    public Guid SellerId { get; init; }
}

public class HasBuyerContactedSellerQueryHandler
    : IRequestHandler<HasBuyerContactedSellerQuery, bool>
{
    private readonly IContactRequestRepository _repository;

    public HasBuyerContactedSellerQueryHandler(IContactRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(HasBuyerContactedSellerQuery request, CancellationToken cancellationToken)
    {
        // Get all contact requests by this buyer, then check if any target this seller
        var buyerContacts = await _repository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        return buyerContacts.Any(c => c.SellerId == request.SellerId);
    }
}

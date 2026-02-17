using DealerManagementService.Application.DTOs;
using DealerManagementService.Domain.Entities;
using DealerManagementService.Domain.Interfaces;
using MediatR;

namespace DealerManagementService.Application.Features.Dealers.Queries;

public record GetDealerByUserIdQuery(Guid UserId) : IRequest<DealerDto?>;

public class GetDealerByUserIdQueryHandler : IRequestHandler<GetDealerByUserIdQuery, DealerDto?>
{
    private readonly IDealerRepository _dealerRepository;

    public GetDealerByUserIdQueryHandler(IDealerRepository dealerRepository)
    {
        _dealerRepository = dealerRepository;
    }

    public async Task<DealerDto?> Handle(GetDealerByUserIdQuery request, CancellationToken cancellationToken)
    {
        var dealer = await _dealerRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (dealer == null) return null;
        
        return MapToDto(dealer);
    }
    
    private static DealerDto MapToDto(Dealer dealer)
    {
        return new DealerDto(
            dealer.Id,
            dealer.UserId,
            dealer.BusinessName,
            dealer.RNC,
            dealer.LegalName,
            dealer.TradeName,
            dealer.Type.ToString(),
            dealer.Status.ToString(),
            dealer.VerificationStatus.ToString(),
            dealer.Email,
            dealer.Phone,
            dealer.MobilePhone,
            dealer.Website,
            dealer.Address,
            dealer.City,
            dealer.Province,
            dealer.ZipCode,
            dealer.Country,
            dealer.Description,
            dealer.LogoUrl,
            dealer.BannerUrl,
            dealer.EstablishedDate,
            dealer.EmployeeCount,
            dealer.CurrentPlan.ToString(),
            dealer.SubscriptionStartDate,
            dealer.SubscriptionEndDate,
            dealer.IsSubscriptionActive,
            dealer.MaxActiveListings,
            dealer.CurrentActiveListings,
            dealer.GetRemainingListings(),
            dealer.CreatedAt,
            dealer.UpdatedAt,
            dealer.VerifiedAt,
            null,
            null,
            dealer.FacebookUrl,
            dealer.InstagramUrl,
            dealer.WhatsAppNumber
        );
    }
}

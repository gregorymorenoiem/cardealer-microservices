using DealerManagementService.Application.DTOs;
using DealerManagementService.Domain.Entities;
using DealerManagementService.Domain.Interfaces;
using MediatR;

namespace DealerManagementService.Application.Features.Dealers.Queries;

public record GetDealersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? VerificationStatus = null,
    string? SearchTerm = null
) : IRequest<DealerListResponse>;

public class GetDealersQueryHandler : IRequestHandler<GetDealersQuery, DealerListResponse>
{
    private readonly IDealerRepository _dealerRepository;

    public GetDealersQueryHandler(IDealerRepository dealerRepository)
    {
        _dealerRepository = dealerRepository;
    }

    public async Task<DealerListResponse> Handle(GetDealersQuery request, CancellationToken cancellationToken)
    {
        DealerStatus? status = null;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<DealerStatus>(request.Status, true, out var s))
        {
            status = s;
        }
        
        VerificationStatus? verificationStatus = null;
        if (!string.IsNullOrEmpty(request.VerificationStatus) && Enum.TryParse<VerificationStatus>(request.VerificationStatus, true, out var vs))
        {
            verificationStatus = vs;
        }
        
        var (dealers, totalCount) = await _dealerRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            status,
            verificationStatus,
            request.SearchTerm,
            cancellationToken
        );
        
        var dealerDtos = dealers.Select(MapToDto).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        
        return new DealerListResponse(
            dealerDtos,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );
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
            null
        );
    }
}

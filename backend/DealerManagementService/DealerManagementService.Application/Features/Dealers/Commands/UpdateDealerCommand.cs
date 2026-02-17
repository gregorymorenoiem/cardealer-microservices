using DealerManagementService.Application.DTOs;
using DealerManagementService.Domain.Interfaces;
using MediatR;

namespace DealerManagementService.Application.Features.Dealers.Commands;

public record UpdateDealerCommand(Guid Id, UpdateDealerRequest Request) : IRequest<DealerDto>;

public class UpdateDealerCommandHandler : IRequestHandler<UpdateDealerCommand, DealerDto>
{
    private readonly IDealerRepository _dealerRepository;

    public UpdateDealerCommandHandler(IDealerRepository dealerRepository)
    {
        _dealerRepository = dealerRepository;
    }

    public async Task<DealerDto> Handle(UpdateDealerCommand request, CancellationToken cancellationToken)
    {
        var dealer = await _dealerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (dealer == null)
        {
            throw new KeyNotFoundException($"Dealer with ID {request.Id} not found");
        }
        
        var req = request.Request;
        
        // Update only non-null fields
        if (req.BusinessName != null) dealer.BusinessName = req.BusinessName;
        if (req.LegalName != null) dealer.LegalName = req.LegalName;
        if (req.TradeName != null) dealer.TradeName = req.TradeName;
        if (req.Email != null) dealer.Email = req.Email;
        if (req.Phone != null) dealer.Phone = req.Phone;
        if (req.MobilePhone != null) dealer.MobilePhone = req.MobilePhone;
        if (req.Website != null) dealer.Website = req.Website;
        if (req.Address != null) dealer.Address = req.Address;
        if (req.City != null) dealer.City = req.City;
        if (req.Province != null) dealer.Province = req.Province;
        if (req.ZipCode != null) dealer.ZipCode = req.ZipCode;
        if (req.Description != null) dealer.Description = req.Description;
        if (req.LogoUrl != null) dealer.LogoUrl = req.LogoUrl;
        if (req.BannerUrl != null) dealer.BannerUrl = req.BannerUrl;
        if (req.EstablishedDate.HasValue) dealer.EstablishedDate = req.EstablishedDate;
        if (req.EmployeeCount.HasValue) dealer.EmployeeCount = req.EmployeeCount;
        if (req.FacebookUrl != null) dealer.FacebookUrl = req.FacebookUrl;
        if (req.InstagramUrl != null) dealer.InstagramUrl = req.InstagramUrl;
        if (req.WhatsAppNumber != null) dealer.WhatsAppNumber = req.WhatsAppNumber;
        
        dealer.UpdatedAt = DateTime.UtcNow;
        
        await _dealerRepository.UpdateAsync(dealer, cancellationToken);
        
        return MapToDto(dealer);
    }
    
    private static DealerDto MapToDto(Domain.Entities.Dealer dealer)
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

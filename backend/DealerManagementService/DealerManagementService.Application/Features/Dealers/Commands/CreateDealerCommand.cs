using DealerManagementService.Application.DTOs;
using DealerManagementService.Domain.Entities;
using DealerManagementService.Domain.Interfaces;
using MediatR;

namespace DealerManagementService.Application.Features.Dealers.Commands;

public record CreateDealerCommand(CreateDealerRequest Request) : IRequest<DealerDto>;

public class CreateDealerCommandHandler : IRequestHandler<CreateDealerCommand, DealerDto>
{
    private readonly IDealerRepository _dealerRepository;

    public CreateDealerCommandHandler(IDealerRepository dealerRepository)
    {
        _dealerRepository = dealerRepository;
    }

    public async Task<DealerDto> Handle(CreateDealerCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        
        // Validate RNC doesn't exist
        if (await _dealerRepository.RNCExistsAsync(req.RNC, cancellationToken))
        {
            throw new InvalidOperationException($"RNC {req.RNC} already registered");
        }
        
        // Parse enum
        if (!Enum.TryParse<DealerType>(req.Type, true, out var dealerType))
        {
            throw new ArgumentException($"Invalid dealer type: {req.Type}");
        }
        
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = req.UserId,
            BusinessName = req.BusinessName,
            RNC = req.RNC,
            LegalName = req.LegalName,
            TradeName = req.TradeName,
            Type = dealerType,
            Status = DealerStatus.Pending,
            VerificationStatus = VerificationStatus.NotVerified,
            Email = req.Email,
            Phone = req.Phone,
            MobilePhone = req.MobilePhone,
            Website = req.Website,
            Address = req.Address,
            City = req.City,
            Province = req.Province,
            ZipCode = req.ZipCode,
            Description = req.Description,
            EstablishedDate = req.EstablishedDate,
            EmployeeCount = req.EmployeeCount,
            FacebookUrl = req.FacebookUrl,
            InstagramUrl = req.InstagramUrl,
            WhatsAppNumber = req.WhatsAppNumber,
            CurrentPlan = DealerPlan.Free,
            MaxActiveListings = 3, // Free plan limit
            CurrentActiveListings = 0,
            CreatedAt = DateTime.UtcNow
        };
        
        var created = await _dealerRepository.AddAsync(dealer, cancellationToken);
        
        return MapToDto(created);
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

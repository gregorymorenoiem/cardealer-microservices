using DealerManagementService.Application.DTOs;
using DealerManagementService.Domain.Entities;
using DealerManagementService.Domain.Interfaces;
using MediatR;

namespace DealerManagementService.Application.Features.Dealers.Queries;

public record GetDealerByIdQuery(Guid Id) : IRequest<DealerDto?>;

public class GetDealerByIdQueryHandler : IRequestHandler<GetDealerByIdQuery, DealerDto?>
{
    private readonly IDealerRepository _dealerRepository;
    private readonly IDealerDocumentRepository _documentRepository;
    private readonly IDealerLocationRepository _locationRepository;

    public GetDealerByIdQueryHandler(
        IDealerRepository dealerRepository,
        IDealerDocumentRepository documentRepository,
        IDealerLocationRepository locationRepository)
    {
        _dealerRepository = dealerRepository;
        _documentRepository = documentRepository;
        _locationRepository = locationRepository;
    }

    public async Task<DealerDto?> Handle(GetDealerByIdQuery request, CancellationToken cancellationToken)
    {
        var dealer = await _dealerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (dealer == null) return null;
        
        // Load related data
        var documents = await _documentRepository.GetByDealerIdAsync(request.Id, cancellationToken);
        var locations = await _locationRepository.GetByDealerIdAsync(request.Id, cancellationToken);
        
        return MapToDto(dealer, documents, locations);
    }
    
    private static DealerDto MapToDto(
        Dealer dealer,
        IEnumerable<DealerDocument> documents,
        IEnumerable<DealerLocation> locations)
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
            documents.Select(d => new DealerDocumentDto(
                d.Id,
                d.DealerId,
                d.Type.ToString(),
                d.FileName,
                d.FileUrl,
                d.FileSizeBytes,
                d.MimeType,
                d.VerificationStatus.ToString(),
                d.VerifiedAt,
                d.RejectionReason,
                d.ExpiryDate,
                d.IsExpired,
                d.UploadedAt
            )).ToList(),
            locations.Select(l => new DealerLocationDto(
                l.Id,
                l.DealerId,
                l.Name,
                l.Type.ToString(),
                l.Address,
                l.City,
                l.Province,
                l.ZipCode,
                l.Country,
                l.Phone,
                l.Email,
                l.WorkingHours,
                l.IsPrimary,
                l.IsActive,
                l.Latitude,
                l.Longitude,
                l.HasShowroom,
                l.HasServiceCenter,
                l.HasParking,
                l.ParkingSpaces,
                l.CreatedAt,
                l.UpdatedAt
            )).ToList()
        );
    }
}

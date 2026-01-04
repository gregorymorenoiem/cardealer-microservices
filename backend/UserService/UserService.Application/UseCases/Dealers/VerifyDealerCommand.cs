using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Application.UseCases.Dealers.VerifyDealer;

public record VerifyDealerCommand(Guid DealerId, VerifyDealerRequest Request) : IRequest<DealerDto>;

public class VerifyDealerCommandHandler : IRequestHandler<VerifyDealerCommand, DealerDto>
{
    private readonly IDealerRepository _dealerRepository;
    private readonly ILogger<VerifyDealerCommandHandler> _logger;

    public VerifyDealerCommandHandler(
        IDealerRepository dealerRepository,
        ILogger<VerifyDealerCommandHandler> logger)
    {
        _dealerRepository = dealerRepository;
        _logger = logger;
    }

    public async Task<DealerDto> Handle(VerifyDealerCommand command, CancellationToken cancellationToken)
    {
        var dealer = await _dealerRepository.GetByIdAsync(command.DealerId);
        if (dealer == null)
        {
            throw new NotFoundException($"Dealer {command.DealerId} not found");
        }

        var request = command.Request;

        if (request.IsVerified)
        {
            dealer.VerificationStatus = DealerVerificationStatus.Verified;
            dealer.VerifiedAt = DateTime.UtcNow;
            dealer.VerifiedByUserId = request.VerifiedByUserId;
            dealer.VerificationNotes = request.Notes;
            dealer.RejectionReason = null;
            
            _logger.LogInformation("Dealer {DealerId} verified by user {VerifiedByUserId}", 
                dealer.Id, request.VerifiedByUserId);
        }
        else
        {
            dealer.VerificationStatus = DealerVerificationStatus.Rejected;
            dealer.RejectionReason = request.Notes;
            dealer.VerifiedAt = null;
            dealer.VerifiedByUserId = null;
            
            _logger.LogInformation("Dealer {DealerId} rejected. Reason: {Reason}", 
                dealer.Id, request.Notes);
        }

        dealer.UpdatedAt = DateTime.UtcNow;

        await _dealerRepository.UpdateAsync(dealer);

        return new DealerDto
        {
            Id = dealer.Id,
            OwnerUserId = dealer.OwnerUserId,
            BusinessName = dealer.BusinessName,
            TradeName = dealer.TradeName,
            Description = dealer.Description,
            DealerType = dealer.DealerType,
            Email = dealer.Email,
            Phone = dealer.Phone,
            WhatsApp = dealer.WhatsApp,
            Website = dealer.Website,
            Address = dealer.Address,
            City = dealer.City,
            State = dealer.State,
            ZipCode = dealer.ZipCode,
            Country = dealer.Country,
            Latitude = dealer.Latitude,
            Longitude = dealer.Longitude,
            LogoUrl = dealer.LogoUrl,
            BannerUrl = dealer.BannerUrl,
            VerificationStatus = dealer.VerificationStatus,
            VerifiedAt = dealer.VerifiedAt,
            RejectionReason = dealer.RejectionReason,
            AverageRating = dealer.AverageRating,
            TotalReviews = dealer.TotalReviews,
            ActiveListings = dealer.ActiveListings,
            IsActive = dealer.IsActive,
            CreatedAt = dealer.CreatedAt,
            UpdatedAt = dealer.UpdatedAt
        };
    }
}

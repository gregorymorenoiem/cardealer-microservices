using DealerManagementService.Domain.Entities;
using DealerManagementService.Domain.Interfaces;
using MediatR;

namespace DealerManagementService.Application.Features.Dealers.Commands;

public record VerifyDealerCommand(Guid DealerId, bool Approved, string? RejectionReason, Guid VerifiedBy) : IRequest<bool>;

public class VerifyDealerCommandHandler : IRequestHandler<VerifyDealerCommand, bool>
{
    private readonly IDealerRepository _dealerRepository;

    public VerifyDealerCommandHandler(IDealerRepository dealerRepository)
    {
        _dealerRepository = dealerRepository;
    }

    public async Task<bool> Handle(VerifyDealerCommand request, CancellationToken cancellationToken)
    {
        var dealer = await _dealerRepository.GetByIdAsync(request.DealerId, cancellationToken);
        if (dealer == null)
        {
            throw new KeyNotFoundException($"Dealer with ID {request.DealerId} not found");
        }
        
        if (request.Approved)
        {
            dealer.VerificationStatus = VerificationStatus.Verified;
            dealer.Status = DealerStatus.Active;
            dealer.VerifiedAt = DateTime.UtcNow;
            dealer.VerifiedBy = request.VerifiedBy;
        }
        else
        {
            dealer.VerificationStatus = VerificationStatus.Rejected;
            dealer.Status = DealerStatus.Rejected;
            // Store rejection reason in description or a separate field
        }
        
        dealer.UpdatedAt = DateTime.UtcNow;
        
        await _dealerRepository.UpdateAsync(dealer, cancellationToken);
        
        return true;
    }
}

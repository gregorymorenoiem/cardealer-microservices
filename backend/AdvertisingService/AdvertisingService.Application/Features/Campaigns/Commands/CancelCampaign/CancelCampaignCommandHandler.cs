using AdvertisingService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Features.Campaigns.Commands.CancelCampaign;

public class CancelCampaignCommandHandler : IRequestHandler<CancelCampaignCommand, bool>
{
    private readonly IAdCampaignRepository _campaignRepo;
    private readonly ILogger<CancelCampaignCommandHandler> _logger;

    public CancelCampaignCommandHandler(IAdCampaignRepository campaignRepo, ILogger<CancelCampaignCommandHandler> logger)
    {
        _campaignRepo = campaignRepo;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelCampaignCommand request, CancellationToken ct)
    {
        var campaign = await _campaignRepo.GetByIdAsync(request.CampaignId, ct);
        if (campaign == null) throw new KeyNotFoundException($"Campaign {request.CampaignId} not found");
        if (campaign.OwnerId != request.RequesterId) throw new UnauthorizedAccessException("Not authorized");

        campaign.Cancel();
        await _campaignRepo.UpdateAsync(campaign, ct);

        _logger.LogInformation("Campaign {CampaignId} cancelled by {UserId}", request.CampaignId, request.RequesterId);
        return true;
    }
}

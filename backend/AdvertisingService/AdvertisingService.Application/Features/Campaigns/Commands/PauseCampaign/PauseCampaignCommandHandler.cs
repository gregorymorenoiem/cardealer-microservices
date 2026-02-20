using AdvertisingService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Features.Campaigns.Commands.PauseCampaign;

public class PauseCampaignCommandHandler : IRequestHandler<PauseCampaignCommand, bool>
{
    private readonly IAdCampaignRepository _campaignRepo;
    private readonly ILogger<PauseCampaignCommandHandler> _logger;

    public PauseCampaignCommandHandler(IAdCampaignRepository campaignRepo, ILogger<PauseCampaignCommandHandler> logger)
    {
        _campaignRepo = campaignRepo;
        _logger = logger;
    }

    public async Task<bool> Handle(PauseCampaignCommand request, CancellationToken ct)
    {
        var campaign = await _campaignRepo.GetByIdAsync(request.CampaignId, ct);
        if (campaign == null) throw new KeyNotFoundException($"Campaign {request.CampaignId} not found");
        if (campaign.OwnerId != request.RequesterId) throw new UnauthorizedAccessException("Not authorized");

        campaign.Pause();
        await _campaignRepo.UpdateAsync(campaign, ct);

        _logger.LogInformation("Campaign {CampaignId} paused by {UserId}", request.CampaignId, request.RequesterId);
        return true;
    }
}

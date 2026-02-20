using AdvertisingService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Features.Campaigns.Commands.ResumeCampaign;

public class ResumeCampaignCommandHandler : IRequestHandler<ResumeCampaignCommand, bool>
{
    private readonly IAdCampaignRepository _campaignRepo;
    private readonly ILogger<ResumeCampaignCommandHandler> _logger;

    public ResumeCampaignCommandHandler(IAdCampaignRepository campaignRepo, ILogger<ResumeCampaignCommandHandler> logger)
    {
        _campaignRepo = campaignRepo;
        _logger = logger;
    }

    public async Task<bool> Handle(ResumeCampaignCommand request, CancellationToken ct)
    {
        var campaign = await _campaignRepo.GetByIdAsync(request.CampaignId, ct);
        if (campaign == null) throw new KeyNotFoundException($"Campaign {request.CampaignId} not found");
        if (campaign.OwnerId != request.RequesterId) throw new UnauthorizedAccessException("Not authorized");

        campaign.Resume();
        await _campaignRepo.UpdateAsync(campaign, ct);

        _logger.LogInformation("Campaign {CampaignId} resumed by {UserId}", request.CampaignId, request.RequesterId);
        return true;
    }
}

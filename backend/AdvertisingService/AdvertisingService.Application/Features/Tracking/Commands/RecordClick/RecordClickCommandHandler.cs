using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Features.Tracking.Commands.RecordClick;

public class RecordClickCommandHandler : IRequestHandler<RecordClickCommand, bool>
{
    private readonly IAdCampaignRepository _campaignRepo;
    private readonly IAdClickRepository _clickRepo;
    private readonly ILogger<RecordClickCommandHandler> _logger;

    public RecordClickCommandHandler(
        IAdCampaignRepository campaignRepo,
        IAdClickRepository clickRepo,
        ILogger<RecordClickCommandHandler> logger)
    {
        _campaignRepo = campaignRepo;
        _clickRepo = clickRepo;
        _logger = logger;
    }

    public async Task<bool> Handle(RecordClickCommand request, CancellationToken ct)
    {
        var campaign = await _campaignRepo.GetByIdAsync(request.CampaignId, ct);
        if (campaign == null || !campaign.IsActive())
        {
            _logger.LogDebug("Skipping click for inactive/missing campaign {CampaignId}", request.CampaignId);
            return false;
        }

        var click = AdClick.Create(request.CampaignId, request.ImpressionId, request.UserId);
        await _clickRepo.AddAsync(click, ct);

        campaign.RecordClick();
        await _campaignRepo.UpdateAsync(campaign, ct);

        return true;
    }
}

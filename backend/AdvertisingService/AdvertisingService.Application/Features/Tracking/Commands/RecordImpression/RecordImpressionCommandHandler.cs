using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;
using AdvertisingService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AdvertisingService.Application.Features.Tracking.Commands.RecordImpression;

public class RecordImpressionCommandHandler : IRequestHandler<RecordImpressionCommand, bool>
{
    private readonly IAdCampaignRepository _campaignRepo;
    private readonly IAdImpressionRepository _impressionRepo;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RecordImpressionCommandHandler> _logger;

    // Dedup: one impression per campaign per session per 24h
    private static readonly TimeSpan DedupTtl = TimeSpan.FromHours(24);

    public RecordImpressionCommandHandler(
        IAdCampaignRepository campaignRepo,
        IAdImpressionRepository impressionRepo,
        IConnectionMultiplexer redis,
        ILogger<RecordImpressionCommandHandler> logger)
    {
        _campaignRepo = campaignRepo;
        _impressionRepo = impressionRepo;
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> Handle(RecordImpressionCommand request, CancellationToken ct)
    {
        var campaign = await _campaignRepo.GetByIdAsync(request.CampaignId, ct);
        if (campaign == null || !campaign.IsActive())
        {
            _logger.LogDebug("Skipping impression for inactive/missing campaign {CampaignId}", request.CampaignId);
            return false;
        }

        // Redis dedup: one impression per campaign per session (SETNX with TTL)
        var sessionKey = request.SessionId ?? request.IpHash ?? "anon";
        var dedupKey = $"ad:imp:dedup:{request.CampaignId}:{sessionKey}";

        var db = _redis.GetDatabase();
        var isNew = await db.StringSetAsync(dedupKey, "1", DedupTtl, When.NotExists);
        if (!isNew)
        {
            _logger.LogDebug("Duplicate impression skipped for campaign {CampaignId}, session {Session}",
                request.CampaignId, sessionKey);
            return true; // Already recorded for this session
        }

        // Record the impression
        var sectionInt = Enum.TryParse<AdPlacementType>(request.Section, true, out var sectionEnum)
            ? (int)sectionEnum
            : 0;

        var impression = AdImpression.Create(
            request.CampaignId,
            request.SessionId,
            request.UserId,
            request.IpHash,
            sectionInt,
            request.Position);

        await _impressionRepo.AddAsync(impression, ct);

        // Update campaign counters
        campaign.RecordView();
        await _campaignRepo.UpdateAsync(campaign, ct);

        return true;
    }
}

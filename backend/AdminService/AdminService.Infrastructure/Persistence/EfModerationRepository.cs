using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using AdminService.Infrastructure.External;
using Microsoft.Extensions.Logging;

namespace AdminService.Infrastructure.Persistence;

/// <summary>
/// Moderation repository backed by VehiclesSaleService HTTP calls.
/// Fetches pending vehicles from VehiclesSaleService and delegates
/// approve/reject actions to it.
/// </summary>
public class EfModerationRepository : IModerationRepository
{
    private readonly IVehicleServiceClient _vehicleServiceClient;
    private readonly ILogger<EfModerationRepository> _logger;

    public EfModerationRepository(
        IVehicleServiceClient vehicleServiceClient,
        ILogger<EfModerationRepository> logger)
    {
        _vehicleServiceClient = vehicleServiceClient;
        _logger = logger;
    }

    public async Task<IEnumerable<PendingListingInfo>> GetPendingListingsAsync(int limit)
    {
        _logger.LogDebug("Getting pending listings, limit: {Limit}", limit);
        var queueResult = await _vehicleServiceClient.GetModerationQueueAsync(1, limit);
        if (queueResult == null)
            return new List<PendingListingInfo>();

        return queueResult.Vehicles.Select(v => new PendingListingInfo
        {
            Id = v.Id,
            Title = v.Title,
            DealerName = v.SellerName,
            SubmittedAt = v.SubmittedAt ?? DateTime.UtcNow
        });
    }

    public async Task<IEnumerable<PendingReportInfo>> GetPendingReportsAsync(int limit)
    {
        _logger.LogDebug("Getting pending reports, limit: {Limit}", limit);
        await Task.CompletedTask;
        return new List<PendingReportInfo>();
    }

    public async Task<(IReadOnlyList<ModerationItem> Items, int Total)> GetModerationQueueAsync(
        string? type = null,
        string? priority = null,
        string? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting moderation queue via VehiclesSaleService: Type={Type}, Status={Status}, Page={Page}",
            type, status, page);

        // Only handle vehicle type (or null = all types)
        if (type != null && !string.Equals(type, "vehicle", StringComparison.OrdinalIgnoreCase))
        {
            return (new List<ModerationItem>().AsReadOnly(), 0);
        }

        // Only show pending items (moderation queue = pending items)
        if (status != null && !string.Equals(status, "pending", StringComparison.OrdinalIgnoreCase))
        {
            return (new List<ModerationItem>().AsReadOnly(), 0);
        }

        var result = await _vehicleServiceClient.GetModerationQueueAsync(page, pageSize, cancellationToken);
        if (result == null)
        {
            _logger.LogWarning("VehiclesSaleService returned null for moderation queue");
            return (new List<ModerationItem>().AsReadOnly(), 0);
        }

        _logger.LogInformation("Moderation queue fetched: {Count} vehicles pending (total={Total})",
            result.Vehicles.Count, result.TotalCount);

        var items = result.Vehicles.Select(v => new ModerationItem
        {
            Id = v.Id,
            Type = "vehicle",
            TargetId = v.Id.ToString(),
            Title = v.Title,
            Description = $"{v.Year} {v.Make} {v.Model} - {v.Condition} - {v.City ?? ""}{(string.IsNullOrEmpty(v.State) ? "" : ", " + v.State)}",
            Price = v.Price,
            Images = v.ImageUrl != null ? new List<string> { v.ImageUrl } : new List<string>(),
            SellerName = v.SellerName,
            SellerType = v.SellerType,
            SellerId = v.Id, // use vehicle Id as placeholder since seller Id not returned in queue endpoint
            SubmittedAt = v.SubmittedAt ?? DateTime.UtcNow,
            Priority = v.RejectionCount > 0 ? "high" : "normal",
            Status = "pending",
            ViewCount = v.ImageCount
        }).ToList();

        return (items.AsReadOnly(), result.TotalCount);
    }

    public async Task<ModerationItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting moderation item by id: {Id}", id);

        // Try to get it directly via GetVehicleByIdAsync
        var detail = await _vehicleServiceClient.GetVehicleByIdAsync(id, cancellationToken);
        if (detail == null)
            return null;

        // Only return if pending review
        if (!string.Equals(detail.StatusName, "pending", StringComparison.OrdinalIgnoreCase))
            return null;

        return new ModerationItem
        {
            Id = detail.Id,
            Type = "vehicle",
            TargetId = detail.Id.ToString(),
            Title = detail.Title,
            Description = $"{detail.Year} {detail.Make} {detail.Model}",
            Price = detail.Price,
            Images = detail.Images?.Select(i => i.Url).ToList() ?? new List<string>(),
            SellerName = detail.SellerName ?? "Unknown",
            SellerType = detail.SellerType ?? "seller",
            SellerId = detail.SellerId ?? Guid.Empty,
            SubmittedAt = detail.CreatedAt,
            Priority = "normal",
            Status = "pending"
        };
    }

    public async Task<ModerationStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting moderation stats from VehiclesSaleService");

        var result = await _vehicleServiceClient.GetModerationQueueAsync(1, 1, cancellationToken);
        var totalPending = result?.TotalCount ?? 0;

        // Count high priority (those with rejection count > 0)
        int highPriority = 0;
        if (totalPending > 0)
        {
            var fullResult = await _vehicleServiceClient.GetModerationQueueAsync(1, Math.Min(totalPending, 200), cancellationToken);
            highPriority = fullResult?.Vehicles?.Count(v => v.RejectionCount > 0) ?? 0;
        }

        return new ModerationStats
        {
            InQueue = totalPending,
            HighPriority = highPriority,
            ReviewedToday = 0,
            RejectedToday = 0,
            ApprovedToday = 0,
            EscalatedCount = 0,
            AvgReviewTimeMinutes = 0
        };
    }

    public async Task<bool> ProcessActionAsync(
        Guid itemId,
        string action,
        string reviewerId,
        string? reason = null,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing moderation action via VehiclesSaleService: ItemId={ItemId}, Action={Action}, ReviewerId={ReviewerId}",
            itemId, action, reviewerId);

        return action.ToLowerInvariant() switch
        {
            "approve" => await _vehicleServiceClient.ApproveVehicleAsync(itemId, reviewerId, notes, cancellationToken),
            "reject" => await _vehicleServiceClient.RejectVehicleAsync(itemId, reviewerId, reason ?? "Rejected by moderator", notes, cancellationToken),
            "escalate" => true, // escalate = keep in queue (no action to VehiclesSaleService)
            "skip" => true,     // skip = no-op, succeed silently
            _ => false
        };
    }

    public async Task<ModerationItem> AddAsync(ModerationItem item, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AddAsync called for moderation item: Type={Type}, TargetId={TargetId}",
            item.Type, item.TargetId);
        // VehiclesSaleService manages its own moderation queue
        await Task.CompletedTask;
        return item;
    }
}

using System.Globalization;
using System.Text.Json;
using ContactService.Domain.Entities;
using ContactService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ContactService.Infrastructure.Services;

/// <summary>
/// Redis-backed repository for conversation overage details.
/// Each overage conversation is stored as a member of a sorted set
/// (score = Unix timestamp) for ordered retrieval.
///
/// Redis key: okla:contact:overage:{dealerId}:{YYYY-MM}
///
/// CONTRA #5 / OVERAGE BILLING FIX
/// </summary>
public sealed class ConversationOverageRepository : IConversationOverageRepository
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<ConversationOverageRepository> _logger;
    private const string KeyPrefix = "okla:contact:overage";

    // In-memory fallback when Redis is unavailable
    private static readonly List<ConversationOverageDetail> InMemoryStore = new();
    private static readonly object StoreLock = new();

    public ConversationOverageRepository(
        ILogger<ConversationOverageRepository> logger,
        IConnectionMultiplexer? redis = null)
    {
        _logger = logger;
        _redis = redis;
    }

    public async Task<ConversationOverageDetail> CreateAsync(
        ConversationOverageDetail detail,
        CancellationToken ct = default)
    {
        if (_redis != null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = BuildKey(detail.DealerId, detail.BillingPeriod);
                var json = JsonSerializer.Serialize(detail);
                var score = new DateTimeOffset(detail.OccurredAtUtc).ToUnixTimeMilliseconds();

                await db.SortedSetAddAsync(key, json, score);

                // Set expiry: 90 days to cover billing + dispute window
                await db.KeyExpireAsync(key, TimeSpan.FromDays(90), ExpireWhen.HasNoExpiry);

                _logger.LogInformation(
                    "[ConversationOverageRepo] Persisted overage detail: Dealer={DealerId} Period={Period} ConvNo={ConvNo} Cost={Cost}",
                    detail.DealerId, detail.BillingPeriod, detail.ConversationNumber, detail.UnitCost);

                return detail;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[ConversationOverageRepo] Redis write failed, falling back to in-memory");
            }
        }

        // Fallback: in-memory store
        lock (StoreLock)
        {
            InMemoryStore.Add(detail);
        }

        return detail;
    }

    public async Task<List<ConversationOverageDetail>> GetByDealerAndPeriodAsync(
        Guid dealerId,
        string billingPeriod,
        CancellationToken ct = default)
    {
        if (_redis != null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = BuildKey(dealerId, billingPeriod);
                var entries = await db.SortedSetRangeByScoreAsync(key);

                var results = new List<ConversationOverageDetail>();
                foreach (var entry in entries)
                {
                    var detail = JsonSerializer.Deserialize<ConversationOverageDetail>(entry.ToString());
                    if (detail != null)
                    {
                        results.Add(detail);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[ConversationOverageRepo] Redis read failed, falling back to in-memory");
            }
        }

        // Fallback: in-memory
        lock (StoreLock)
        {
            return InMemoryStore
                .Where(d => d.DealerId == dealerId && d.BillingPeriod == billingPeriod)
                .OrderBy(d => d.OccurredAtUtc)
                .ToList();
        }
    }

    public async Task<int> GetOverageCountAsync(
        Guid dealerId,
        string billingPeriod,
        CancellationToken ct = default)
    {
        if (_redis != null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = BuildKey(dealerId, billingPeriod);
                return (int)await db.SortedSetLengthAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[ConversationOverageRepo] Redis count failed, falling back to in-memory");
            }
        }

        lock (StoreLock)
        {
            return InMemoryStore.Count(d => d.DealerId == dealerId && d.BillingPeriod == billingPeriod);
        }
    }

    public async Task<decimal> GetOverageTotalCostAsync(
        Guid dealerId,
        string billingPeriod,
        CancellationToken ct = default)
    {
        var details = await GetByDealerAndPeriodAsync(dealerId, billingPeriod, ct);
        return details.Sum(d => d.UnitCost);
    }

    private static string BuildKey(Guid dealerId, string billingPeriod)
    {
        return $"{KeyPrefix}:{dealerId:N}:{billingPeriod}";
    }
}

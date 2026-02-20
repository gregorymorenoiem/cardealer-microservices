using System.Diagnostics.Metrics;

namespace AdvertisingService.Infrastructure.Services;

public class AdvertisingMetrics
{
    private readonly Counter<long> _impressionsCounter;
    private readonly Counter<long> _clicksCounter;
    private readonly Counter<long> _campaignsCreatedCounter;
    private readonly Histogram<double> _rotationDuration;
    private readonly Counter<long> _cacheHitsCounter;
    private readonly Counter<long> _cacheMissesCounter;

    public AdvertisingMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("AdvertisingService", "1.0.0");

        _impressionsCounter = meter.CreateCounter<long>(
            "advertising.impressions.total",
            description: "Total ad impressions recorded");

        _clicksCounter = meter.CreateCounter<long>(
            "advertising.clicks.total",
            description: "Total ad clicks recorded");

        _campaignsCreatedCounter = meter.CreateCounter<long>(
            "advertising.campaigns.created.total",
            description: "Total campaigns created");

        _rotationDuration = meter.CreateHistogram<double>(
            "advertising.rotation.duration_ms",
            unit: "ms",
            description: "Rotation computation duration in milliseconds");

        _cacheHitsCounter = meter.CreateCounter<long>(
            "advertising.cache.hits.total",
            description: "Total rotation cache hits");

        _cacheMissesCounter = meter.CreateCounter<long>(
            "advertising.cache.misses.total",
            description: "Total rotation cache misses");
    }

    public void RecordImpression(string placement) =>
        _impressionsCounter.Add(1, new KeyValuePair<string, object?>("placement", placement));

    public void RecordClick(string placement) =>
        _clicksCounter.Add(1, new KeyValuePair<string, object?>("placement", placement));

    public void RecordCampaignCreated(string ownerType) =>
        _campaignsCreatedCounter.Add(1, new KeyValuePair<string, object?>("owner_type", ownerType));

    public void RecordRotationDuration(double durationMs, string section) =>
        _rotationDuration.Record(durationMs, new KeyValuePair<string, object?>("section", section));

    public void RecordCacheHit() => _cacheHitsCounter.Add(1);
    public void RecordCacheMiss() => _cacheMissesCounter.Add(1);
}

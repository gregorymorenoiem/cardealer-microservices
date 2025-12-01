namespace CacheService.Domain;

/// <summary>
/// Represents cache performance statistics
/// </summary>
public class CacheStatistics
{
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public long TotalSets { get; set; }
    public long TotalDeletes { get; set; }
    public long TotalKeys { get; set; }
    public long TotalSizeInBytes { get; set; }
    public DateTime LastResetAt { get; set; }
    public Dictionary<string, long> HitsByKey { get; set; } = new();
    public Dictionary<string, long> MissesByKey { get; set; } = new();

    /// <summary>
    /// Calculates the cache hit ratio
    /// </summary>
    public double GetHitRatio()
    {
        var total = TotalHits + TotalMisses;
        return total == 0 ? 0 : (double)TotalHits / total;
    }

    /// <summary>
    /// Gets the cache hit percentage
    /// </summary>
    public double GetHitPercentage()
    {
        return GetHitRatio() * 100;
    }

    /// <summary>
    /// Records a cache hit
    /// </summary>
    public void RecordHit(string key)
    {
        TotalHits++;
        if (!HitsByKey.ContainsKey(key))
            HitsByKey[key] = 0;
        HitsByKey[key]++;
    }

    /// <summary>
    /// Records a cache miss
    /// </summary>
    public void RecordMiss(string key)
    {
        TotalMisses++;
        if (!MissesByKey.ContainsKey(key))
            MissesByKey[key] = 0;
        MissesByKey[key]++;
    }

    /// <summary>
    /// Records a set operation
    /// </summary>
    public void RecordSet()
    {
        TotalSets++;
    }

    /// <summary>
    /// Records a delete operation
    /// </summary>
    public void RecordDelete()
    {
        TotalDeletes++;
    }

    /// <summary>
    /// Resets all statistics
    /// </summary>
    public void Reset()
    {
        TotalHits = 0;
        TotalMisses = 0;
        TotalSets = 0;
        TotalDeletes = 0;
        TotalKeys = 0;
        TotalSizeInBytes = 0;
        HitsByKey.Clear();
        MissesByKey.Clear();
        LastResetAt = DateTime.UtcNow;
    }
}

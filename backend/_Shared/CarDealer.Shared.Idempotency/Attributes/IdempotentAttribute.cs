namespace CarDealer.Shared.Idempotency.Attributes;

/// <summary>
/// Marks an endpoint as idempotent, enabling automatic idempotency handling
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class IdempotentAttribute : Attribute
{
    /// <summary>
    /// Whether to require an idempotency key
    /// </summary>
    public bool RequireKey { get; set; } = true;

    /// <summary>
    /// Custom header name for the idempotency key (overrides global setting)
    /// </summary>
    public string? HeaderName { get; set; }

    /// <summary>
    /// TTL in seconds for the idempotency record (0 = use default)
    /// </summary>
    public int TtlSeconds { get; set; } = 0;

    /// <summary>
    /// Whether to include request body in hash calculation
    /// </summary>
    public bool IncludeBodyInHash { get; set; } = true;

    /// <summary>
    /// Custom key prefix for this endpoint
    /// </summary>
    public string? KeyPrefix { get; set; }
}

/// <summary>
/// Marks an endpoint to skip idempotency checks
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class SkipIdempotencyAttribute : Attribute
{
}

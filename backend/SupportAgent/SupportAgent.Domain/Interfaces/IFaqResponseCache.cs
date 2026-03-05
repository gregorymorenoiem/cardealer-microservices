namespace SupportAgent.Domain.Interfaces;

/// <summary>
/// FAQ-level response cache for SupportAgent.
/// Caches Claude API responses keyed by a normalised hash of the user message.
/// Common questions like "¿Cómo me registro?" resolve in &lt;5 ms instead of 4-8 s.
/// </summary>
public interface IFaqResponseCache
{
    /// <summary>Try to get a cached response for the (normalised) user message.</summary>
    bool TryGet(string userMessage, out string cachedResponse);

    /// <summary>Store a response in cache for the normalised user message.</summary>
    void Set(string userMessage, string response, TimeSpan? ttl = null);
}

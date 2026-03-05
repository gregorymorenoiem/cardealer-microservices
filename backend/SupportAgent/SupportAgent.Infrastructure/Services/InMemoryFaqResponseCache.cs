using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SupportAgent.Domain.Interfaces;

namespace SupportAgent.Infrastructure.Services;

/// <summary>
/// In-memory implementation of FAQ response cache.
/// Common questions like "¿Cómo me registro?" resolve in &lt;5 ms instead of 4-8 s.
/// TTL is configurable (default 5 min to match Anthropic prompt-cache TTL).
/// </summary>
public sealed class InMemoryFaqResponseCache : IFaqResponseCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InMemoryFaqResponseCache> _logger;

    // Default TTL aligned with Anthropic prompt-cache ephemeral window (5 min).
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

    // Pre-compiled regex for normalisation (remove emojis, punctuation, extra spaces)
    private static readonly Regex NormaliseRegex = new(
        @"[^\p{L}\p{N}\s]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public InMemoryFaqResponseCache(IMemoryCache cache, ILogger<InMemoryFaqResponseCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public bool TryGet(string userMessage, out string cachedResponse)
    {
        var key = BuildKey(userMessage);
        if (_cache.TryGetValue(key, out string? value) && value is not null)
        {
            _logger.LogDebug("FAQ cache HIT for key {Key}", key[..12]);
            cachedResponse = value;
            return true;
        }

        cachedResponse = string.Empty;
        return false;
    }

    public void Set(string userMessage, string response, TimeSpan? ttl = null)
    {
        var key = BuildKey(userMessage);
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? DefaultTtl,
            Size = response.Length  // rough size limit for bounded cache
        };
        _cache.Set(key, response, options);
        _logger.LogDebug("FAQ cache SET for key {Key} (TTL={TTL})", key[..12], ttl ?? DefaultTtl);
    }

    /// <summary>
    /// Normalise the user message to a deterministic cache key.
    /// Steps: lowercase → remove emojis/punctuation → collapse whitespace → SHA256 truncated.
    /// </summary>
    private static string BuildKey(string message)
    {
        var normalised = message.Trim().ToLowerInvariant();
        normalised = NormaliseRegex.Replace(normalised, " ");
        normalised = Regex.Replace(normalised, @"\s+", " ").Trim();

        // SHA-256 hash truncated to 16 chars for compact key
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalised));
        return $"faq:{Convert.ToHexString(hash)[..16]}";
    }
}

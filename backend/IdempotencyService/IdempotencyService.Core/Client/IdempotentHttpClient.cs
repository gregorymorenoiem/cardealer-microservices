using System.Net.Http.Headers;

namespace IdempotencyService.Client;

/// <summary>
/// HTTP client for making idempotent requests
/// </summary>
public class IdempotentHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _headerName;

    public IdempotentHttpClient(HttpClient httpClient, string headerName = "X-Idempotency-Key")
    {
        _httpClient = httpClient;
        _headerName = headerName;
    }

    /// <summary>
    /// Sends an idempotent POST request
    /// </summary>
    public async Task<HttpResponseMessage> PostAsync(
        string requestUri,
        HttpContent content,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var key = idempotencyKey ?? Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content
        };
        request.Headers.Add(_headerName, key);

        return await _httpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sends an idempotent PUT request
    /// </summary>
    public async Task<HttpResponseMessage> PutAsync(
        string requestUri,
        HttpContent content,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var key = idempotencyKey ?? Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = content
        };
        request.Headers.Add(_headerName, key);

        return await _httpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sends an idempotent PATCH request
    /// </summary>
    public async Task<HttpResponseMessage> PatchAsync(
        string requestUri,
        HttpContent content,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var key = idempotencyKey ?? Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Patch, requestUri)
        {
            Content = content
        };
        request.Headers.Add(_headerName, key);

        return await _httpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sends an idempotent DELETE request
    /// </summary>
    public async Task<HttpResponseMessage> DeleteAsync(
        string requestUri,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var key = idempotencyKey ?? Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        request.Headers.Add(_headerName, key);

        return await _httpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Checks if the response was replayed from cache
    /// </summary>
    public static bool IsReplayed(HttpResponseMessage response)
    {
        return response.Headers.TryGetValues("X-Idempotency-Replayed", out var values)
               && values.Contains("true");
    }

    /// <summary>
    /// Generates a deterministic idempotency key based on request parameters
    /// </summary>
    public static string GenerateKey(string operation, params object[] parameters)
    {
        var keyParts = new List<string> { operation };
        keyParts.AddRange(parameters.Select(p => p?.ToString() ?? "null"));

        var combined = string.Join(":", keyParts);
        return $"{operation}:{GetStableHashCode(combined)}";
    }

    private static string GetStableHashCode(string str)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return $"{hash1 + (hash2 * 1566083941):X}";
        }
    }
}

/// <summary>
/// Extension methods for HttpClient
/// </summary>
public static class HttpClientIdempotencyExtensions
{
    /// <summary>
    /// Adds idempotency key to request headers
    /// </summary>
    public static HttpRequestMessage WithIdempotencyKey(
        this HttpRequestMessage request,
        string idempotencyKey,
        string headerName = "X-Idempotency-Key")
    {
        request.Headers.Add(headerName, idempotencyKey);
        return request;
    }

    /// <summary>
    /// Creates an idempotent HTTP client
    /// </summary>
    public static IdempotentHttpClient AsIdempotent(
        this HttpClient httpClient,
        string headerName = "X-Idempotency-Key")
    {
        return new IdempotentHttpClient(httpClient, headerName);
    }
}

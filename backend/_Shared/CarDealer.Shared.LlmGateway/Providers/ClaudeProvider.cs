// =============================================================================
// Claude Provider — Anthropic API Implementation
//
// Primary provider in the cascade. Uses the Messages API with prompt caching.
// Throws LlmProviderException on 429/500/503 to trigger fallback.
// =============================================================================

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CarDealer.Shared.LlmGateway.Abstractions;
using CarDealer.Shared.LlmGateway.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarDealer.Shared.LlmGateway.Providers;

/// <summary>
/// Anthropic Claude provider using the Messages API.
/// </summary>
public sealed class ClaudeProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeProviderOptions _options;
    private readonly ILogger<ClaudeProvider> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public LlmProviderType ProviderType => LlmProviderType.Claude;
    public string DisplayName => $"Claude ({_options.Model})";

    public ClaudeProvider(
        HttpClient httpClient,
        IOptions<LlmGatewayOptions> options,
        ILogger<ClaudeProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.Claude;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", _options.ApiVersion);

        if (_options.EnablePromptCaching)
        {
            _httpClient.DefaultRequestHeaders.Add("anthropic-beta", "prompt-caching-2024-07-31");
        }
    }

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        var messages = BuildMessages(request);
        var body = new ClaudeRequestBody
        {
            Model = _options.Model,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            System = string.IsNullOrEmpty(request.SystemPrompt)
                ? null
                : (_options.EnablePromptCaching
                    ? null // System prompt goes in system array for caching
                    : request.SystemPrompt),
            SystemArray = _options.EnablePromptCaching && !string.IsNullOrEmpty(request.SystemPrompt)
                ? [new SystemBlock { Type = "text", Text = request.SystemPrompt, CacheControl = new CacheControl { Type = "ephemeral" } }]
                : null,
            Messages = messages
        };

        var json = JsonSerializer.Serialize(body, JsonOpts);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var timeout = request.Timeout ?? _options.Timeout ?? TimeSpan.FromSeconds(30);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        HttpResponseMessage? httpResponse = null;
        try
        {
            httpResponse = await _httpClient.PostAsync("/v1/messages", content, cts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            sw.Stop();
            throw new LlmProviderException(
                LlmProviderType.Claude,
                $"Claude timeout after {sw.ElapsedMilliseconds}ms",
                httpStatusCode: 408,
                isTransient: true);
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            var statusCode = (int)httpResponse.StatusCode;
            var errorBody = await httpResponse.Content.ReadAsStringAsync(ct);
            sw.Stop();

            var isTransient = statusCode is 429 or 500 or 503 or 529;

            _logger.LogWarning(
                "Claude API error {StatusCode}: {Error} (latency: {Latency}ms)",
                statusCode, errorBody.Length > 200 ? errorBody[..200] : errorBody, sw.ElapsedMilliseconds);

            throw new LlmProviderException(
                LlmProviderType.Claude,
                $"Claude API returned {statusCode}",
                httpStatusCode: statusCode,
                isTransient: isTransient);
        }

        var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);
        var claudeResponse = JsonSerializer.Deserialize<ClaudeResponseBody>(responseJson, JsonOpts);
        sw.Stop();

        var textContent = claudeResponse?.Content?.FirstOrDefault(c => c.Type == "text")?.Text
                          ?? string.Empty;

        return new LlmResponse
        {
            Content = textContent,
            Provider = LlmProviderType.Claude,
            ModelId = claudeResponse?.Model ?? _options.Model,
            FallbackLevel = 0,
            ProviderLatency = sw.Elapsed,
            TotalLatency = sw.Elapsed,
            InputTokens = claudeResponse?.Usage?.InputTokens ?? 0,
            OutputTokens = claudeResponse?.Usage?.OutputTokens ?? 0,
            StopReason = claudeResponse?.StopReason
        };
    }

    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            var request = new LlmRequest
            {
                UserMessage = "Hi",
                MaxTokens = 5,
                Temperature = 0,
                CallerAgent = "HealthCheck"
            };

            var response = await CompleteAsync(request, ct);
            return !string.IsNullOrEmpty(response.Content);
        }
        catch
        {
            return false;
        }
    }

    private static List<ClaudeMessage> BuildMessages(LlmRequest request)
    {
        var messages = new List<ClaudeMessage>();

        foreach (var msg in request.History)
        {
            messages.Add(new ClaudeMessage { Role = msg.Role, Content = msg.Content });
        }

        messages.Add(new ClaudeMessage { Role = "user", Content = request.UserMessage });
        return messages;
    }

    // =========================================================================
    // Internal DTOs for Anthropic API
    // =========================================================================

    private sealed class ClaudeRequestBody
    {
        public string Model { get; set; } = null!;
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
        public string? System { get; set; }

        [JsonPropertyName("system")]
        public List<SystemBlock>? SystemArray { get; set; }

        public List<ClaudeMessage> Messages { get; set; } = [];
    }

    private sealed class SystemBlock
    {
        public string Type { get; set; } = "text";
        public string Text { get; set; } = null!;
        public CacheControl? CacheControl { get; set; }
    }

    private sealed class CacheControl
    {
        public string Type { get; set; } = "ephemeral";
    }

    private sealed class ClaudeMessage
    {
        public string Role { get; set; } = null!;
        public string Content { get; set; } = null!;
    }

    private sealed class ClaudeResponseBody
    {
        public string? Id { get; set; }
        public string? Model { get; set; }
        public List<ContentBlock>? Content { get; set; }
        public string? StopReason { get; set; }
        public UsageInfo? Usage { get; set; }
    }

    private sealed class ContentBlock
    {
        public string Type { get; set; } = null!;
        public string? Text { get; set; }
    }

    private sealed class UsageInfo
    {
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
    }
}

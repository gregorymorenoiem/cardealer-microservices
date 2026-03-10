// =============================================================================
// Gemini Provider — Google Generative Language API
//
// First fallback in the cascade when Claude fails (429/500/503).
// Uses Gemini 1.5 Flash for low-latency responses.
// Must respond within 500ms or trigger cascade to Llama.
// =============================================================================

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CarDealer.Shared.LlmGateway.Abstractions;
using CarDealer.Shared.LlmGateway.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarDealer.Shared.LlmGateway.Providers;

/// <summary>
/// Google Gemini provider using the generateContent API.
/// </summary>
public sealed class GeminiProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiProviderOptions _options;
    private readonly ILogger<GeminiProvider> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public LlmProviderType ProviderType => LlmProviderType.Gemini;
    public string DisplayName => $"Gemini ({_options.Model})";

    public GeminiProvider(
        HttpClient httpClient,
        IOptions<LlmGatewayOptions> options,
        ILogger<GeminiProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.Gemini;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        var contents = BuildContents(request);
        var body = new GeminiRequestBody
        {
            Contents = contents,
            SystemInstruction = string.IsNullOrEmpty(request.SystemPrompt)
                ? null
                : new GeminiContent
                {
                    Parts = [new GeminiPart { Text = request.SystemPrompt }]
                },
            GenerationConfig = new GeminiGenerationConfig
            {
                MaxOutputTokens = request.MaxTokens,
                Temperature = (float)request.Temperature
            }
        };

        var json = JsonSerializer.Serialize(body, JsonOpts);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var timeout = request.Timeout ?? _options.Timeout ?? TimeSpan.FromMilliseconds(500);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        // Gemini URL: /models/{model}:generateContent?key={apiKey}
        var url = $"/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";

        HttpResponseMessage? httpResponse = null;
        try
        {
            httpResponse = await _httpClient.PostAsync(url, content, cts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            sw.Stop();
            throw new LlmProviderException(
                LlmProviderType.Gemini,
                $"Gemini timeout after {sw.ElapsedMilliseconds}ms",
                httpStatusCode: 408,
                isTransient: true);
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            var statusCode = (int)httpResponse.StatusCode;
            var errorBody = await httpResponse.Content.ReadAsStringAsync(ct);
            sw.Stop();

            var isTransient = statusCode is 429 or 500 or 503;

            _logger.LogWarning(
                "Gemini API error {StatusCode}: {Error} (latency: {Latency}ms)",
                statusCode, errorBody.Length > 200 ? errorBody[..200] : errorBody, sw.ElapsedMilliseconds);

            throw new LlmProviderException(
                LlmProviderType.Gemini,
                $"Gemini API returned {statusCode}",
                httpStatusCode: statusCode,
                isTransient: isTransient);
        }

        var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);
        var geminiResponse = JsonSerializer.Deserialize<GeminiResponseBody>(responseJson, JsonOpts);
        sw.Stop();

        var textContent = geminiResponse?.Candidates?.FirstOrDefault()
            ?.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;

        var usage = geminiResponse?.UsageMetadata;

        return new LlmResponse
        {
            Content = textContent,
            Provider = LlmProviderType.Gemini,
            ModelId = _options.Model,
            FallbackLevel = 1,
            ProviderLatency = sw.Elapsed,
            TotalLatency = sw.Elapsed,
            InputTokens = usage?.PromptTokenCount ?? 0,
            OutputTokens = usage?.CandidatesTokenCount ?? 0,
            StopReason = geminiResponse?.Candidates?.FirstOrDefault()?.FinishReason
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

    private static List<GeminiContent> BuildContents(LlmRequest request)
    {
        var contents = new List<GeminiContent>();

        foreach (var msg in request.History)
        {
            contents.Add(new GeminiContent
            {
                Role = msg.Role == "assistant" ? "model" : "user",
                Parts = [new GeminiPart { Text = msg.Content }]
            });
        }

        contents.Add(new GeminiContent
        {
            Role = "user",
            Parts = [new GeminiPart { Text = request.UserMessage }]
        });

        return contents;
    }

    // =========================================================================
    // Internal DTOs for Google Gemini API
    // =========================================================================

    private sealed class GeminiRequestBody
    {
        public List<GeminiContent> Contents { get; set; } = [];
        public GeminiContent? SystemInstruction { get; set; }
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    private sealed class GeminiContent
    {
        public string? Role { get; set; }
        public List<GeminiPart> Parts { get; set; } = [];
    }

    private sealed class GeminiPart
    {
        public string? Text { get; set; }
    }

    private sealed class GeminiGenerationConfig
    {
        public int MaxOutputTokens { get; set; }
        public float Temperature { get; set; }
    }

    private sealed class GeminiResponseBody
    {
        public List<GeminiCandidate>? Candidates { get; set; }
        public GeminiUsageMetadata? UsageMetadata { get; set; }
    }

    private sealed class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
        public string? FinishReason { get; set; }
    }

    private sealed class GeminiUsageMetadata
    {
        public int PromptTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
        public int TotalTokenCount { get; set; }
    }
}

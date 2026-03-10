// =============================================================================
// Llama Provider — Self-hosted Llama 3.1 70B on DigitalOcean GPU
//
// Second fallback (level 2) when both Claude and Gemini fail.
// Uses OpenAI-compatible API format served by llama.cpp/vLLM.
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
/// Self-hosted Llama provider using OpenAI-compatible chat completions API.
/// </summary>
public sealed class LlamaProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly LlamaProviderOptions _options;
    private readonly ILogger<LlamaProvider> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public LlmProviderType ProviderType => LlmProviderType.Llama;
    public string DisplayName => $"Llama ({_options.Model})";

    public LlamaProvider(
        HttpClient httpClient,
        IOptions<LlmGatewayOptions> options,
        ILogger<LlamaProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.Llama;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);

        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        }
    }

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        var messages = BuildMessages(request);
        var body = new OpenAiChatRequest
        {
            Model = _options.Model,
            Messages = messages,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature
        };

        var json = JsonSerializer.Serialize(body, JsonOpts);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var timeout = request.Timeout ?? _options.Timeout ?? TimeSpan.FromSeconds(15);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        HttpResponseMessage? httpResponse = null;
        try
        {
            httpResponse = await _httpClient.PostAsync("/v1/chat/completions", content, cts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            sw.Stop();
            throw new LlmProviderException(
                LlmProviderType.Llama,
                $"Llama timeout after {sw.ElapsedMilliseconds}ms",
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
                "Llama API error {StatusCode}: {Error} (latency: {Latency}ms)",
                statusCode, errorBody.Length > 200 ? errorBody[..200] : errorBody, sw.ElapsedMilliseconds);

            throw new LlmProviderException(
                LlmProviderType.Llama,
                $"Llama API returned {statusCode}",
                httpStatusCode: statusCode,
                isTransient: isTransient);
        }

        var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);
        var llamaResponse = JsonSerializer.Deserialize<OpenAiChatResponse>(responseJson, JsonOpts);
        sw.Stop();

        var choice = llamaResponse?.Choices?.FirstOrDefault();
        var textContent = choice?.Message?.Content ?? string.Empty;

        return new LlmResponse
        {
            Content = textContent,
            Provider = LlmProviderType.Llama,
            ModelId = llamaResponse?.Model ?? _options.Model,
            FallbackLevel = 2,
            ProviderLatency = sw.Elapsed,
            TotalLatency = sw.Elapsed,
            InputTokens = llamaResponse?.Usage?.PromptTokens ?? 0,
            OutputTokens = llamaResponse?.Usage?.CompletionTokens ?? 0,
            StopReason = choice?.FinishReason
        };
    }

    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            // Simple health check — just hit the models endpoint
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await _httpClient.GetAsync("/v1/models", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static List<OpenAiMessage> BuildMessages(LlmRequest request)
    {
        var messages = new List<OpenAiMessage>();

        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            messages.Add(new OpenAiMessage { Role = "system", Content = request.SystemPrompt });
        }

        foreach (var msg in request.History)
        {
            messages.Add(new OpenAiMessage { Role = msg.Role, Content = msg.Content });
        }

        messages.Add(new OpenAiMessage { Role = "user", Content = request.UserMessage });
        return messages;
    }

    // =========================================================================
    // Internal DTOs for OpenAI-compatible API
    // =========================================================================

    private sealed class OpenAiChatRequest
    {
        public string Model { get; set; } = null!;
        public List<OpenAiMessage> Messages { get; set; } = [];
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
    }

    private sealed class OpenAiMessage
    {
        public string Role { get; set; } = null!;
        public string Content { get; set; } = null!;
    }

    private sealed class OpenAiChatResponse
    {
        public string? Id { get; set; }
        public string? Model { get; set; }
        public List<OpenAiChoice>? Choices { get; set; }
        public OpenAiUsage? Usage { get; set; }
    }

    private sealed class OpenAiChoice
    {
        public OpenAiMessage? Message { get; set; }
        public string? FinishReason { get; set; }
    }

    private sealed class OpenAiUsage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}

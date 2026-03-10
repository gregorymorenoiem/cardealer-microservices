// =============================================================================
// LLM Response Parser — Shared Utility
//
// Centralizes response parsing across all agents for multi-model compatibility:
//   1. Markdown code block stripping (```json ... ```)
//   2. JSON brace extraction ({...} from surrounding text)
//   3. Regex field extraction from truncated JSON
//   4. Raw text fallback with JSON prefix stripping
//
// Used by: SearchAgent, RecoAgent, DealerChatAgent, SupportAgent
// =============================================================================

using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.LlmGateway.Services;

/// <summary>
/// Shared utility for parsing LLM responses across different models.
/// Handles format differences between Claude, Gemini, and Llama outputs.
/// </summary>
public static partial class LlmResponseParser
{
    /// <summary>
    /// Standard JSON options using snake_case (OKLA convention for all agents).
    /// </summary>
    public static readonly JsonSerializerOptions SnakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// Parse an LLM response into a strongly-typed object using the 4-tier strategy:
    /// 1. Direct JSON deserialization
    /// 2. Code block extraction (```json...```) then deserialize
    /// 3. Brace extraction ({...}) then deserialize
    /// 4. Return default(T) on failure
    /// </summary>
    public static T? ParseJsonResponse<T>(string rawResponse, ILogger? logger = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
            return null;

        // Tier 1: Try direct deserialization (response is pure JSON)
        try
        {
            var result = JsonSerializer.Deserialize<T>(rawResponse.Trim(), SnakeCaseOptions);
            if (result != null) return result;
        }
        catch (JsonException)
        {
            // Expected — response may contain non-JSON wrapper text
        }

        // Tier 2: Strip markdown code blocks
        var stripped = StripCodeBlocks(rawResponse);
        if (stripped != rawResponse)
        {
            try
            {
                var result = JsonSerializer.Deserialize<T>(stripped, SnakeCaseOptions);
                if (result != null) return result;
            }
            catch (JsonException)
            {
                // Continue to next tier
            }
        }

        // Tier 3: Extract JSON from braces
        var extracted = ExtractJsonFromBraces(rawResponse);
        if (extracted != null)
        {
            try
            {
                var result = JsonSerializer.Deserialize<T>(extracted, SnakeCaseOptions);
                if (result != null) return result;
            }
            catch (JsonException)
            {
                // Continue to next tier
            }
        }

        // Tier 4: Extract JSON array if T is a collection type
        var arrayExtracted = ExtractJsonArray(rawResponse);
        if (arrayExtracted != null)
        {
            try
            {
                var result = JsonSerializer.Deserialize<T>(arrayExtracted, SnakeCaseOptions);
                if (result != null) return result;
            }
            catch (JsonException)
            {
                // All tiers exhausted
            }
        }

        logger?.LogWarning(
            "[LlmResponseParser] Failed to parse response as {Type}. Raw length={Length}, first 200 chars: {Preview}",
            typeof(T).Name,
            rawResponse.Length,
            rawResponse.Length > 200 ? rawResponse[..200] : rawResponse);

        return null;
    }

    /// <summary>
    /// Extract a specific string field from a JSON response using regex.
    /// Useful when JSON is truncated but the field of interest is present.
    /// </summary>
    public static string? ExtractField(string rawResponse, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(rawResponse)) return null;

        // Pattern: "field_name": "value" or "fieldName": "value"
        var pattern = $@"""{fieldName}""\s*:\s*""((?:[^""\\]|\\.)*)""";
        var match = Regex.Match(rawResponse, pattern, RegexOptions.Singleline);

        return match.Success ? UnescapeJsonString(match.Groups[1].Value) : null;
    }

    /// <summary>
    /// Strip markdown code block wrappers from LLM output.
    /// Handles: ```json\n...\n```, ```\n...\n```, ```JSON\n...\n```
    /// </summary>
    public static string StripCodeBlocks(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        var trimmed = text.Trim();

        // Pattern: ```json\n...\n``` or ```JSON\n...\n``` or ```\n...\n```
        var match = CodeBlockRegex().Match(trimmed);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return trimmed;
    }

    /// <summary>
    /// Extract JSON object by finding matching { } braces.
    /// Handles cases where the model wraps JSON in explanatory text.
    /// </summary>
    public static string? ExtractJsonFromBraces(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var firstBrace = text.IndexOf('{');
        if (firstBrace == -1) return null;

        var lastBrace = text.LastIndexOf('}');
        if (lastBrace <= firstBrace) return null;

        return text[firstBrace..(lastBrace + 1)];
    }

    /// <summary>
    /// Extract JSON array by finding matching [ ] brackets.
    /// </summary>
    public static string? ExtractJsonArray(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var firstBracket = text.IndexOf('[');
        if (firstBracket == -1) return null;

        var lastBracket = text.LastIndexOf(']');
        if (lastBracket <= firstBracket) return null;

        return text[firstBracket..(lastBracket + 1)];
    }

    /// <summary>
    /// Get plain text content from an LLM response, stripping any JSON/code formatting.
    /// </summary>
    public static string GetPlainText(string rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse)) return string.Empty;

        var text = StripCodeBlocks(rawResponse).Trim();

        // If it looks like JSON, try to extract a "response" or "text" field
        if (text.StartsWith('{'))
        {
            var responseField = ExtractField(text, "response")
                                ?? ExtractField(text, "text")
                                ?? ExtractField(text, "content")
                                ?? ExtractField(text, "message");

            if (responseField != null) return responseField;
        }

        return text;
    }

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    private static string UnescapeJsonString(string escaped)
    {
        return escaped
            .Replace("\\n", "\n")
            .Replace("\\t", "\t")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }

    [GeneratedRegex(@"^```(?:json|JSON)?\s*\n?(.*?)\n?```\s*$", RegexOptions.Singleline)]
    private static partial Regex CodeBlockRegex();
}

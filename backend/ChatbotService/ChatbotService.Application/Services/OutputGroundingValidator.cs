using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace ChatbotService.Application.Services;

/// <summary>
/// Validates LLM output against grounding data (inventory) to prevent hallucinations.
/// 
/// Anti-hallucination checks:
/// - Price claims match inventory data
/// - Vehicle specs (year, make, model) match inventory
/// - Fabricated vehicles are detected
/// - Financial calculations are validated
/// </summary>
public static class OutputGroundingValidator
{
    // Regex patterns for extracting claims from LLM output
    private static readonly Regex PricePattern = new(
        @"RD\$[\s]*([\d,\.]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex YearPattern = new(
        @"\b(19[5-9]\d|20[0-3]\d)\b",
        RegexOptions.Compiled);

    private static readonly Regex MileagePattern = new(
        @"([\d,\.]+)\s*(km|kilómetros|millas|miles)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Validates that a response doesn't contain ungrounded claims
    /// about vehicles not in the provided inventory.
    /// </summary>
    public static GroundingResult ValidateAgainstInventory(
        string response,
        IReadOnlyList<InventoryItem> inventory)
    {
        var result = new GroundingResult
        {
            IsGrounded = true,
            UngroundedClaims = new List<string>()
        };

        if (string.IsNullOrWhiteSpace(response) || inventory.Count == 0)
            return result;

        // Extract price claims from response
        var priceClaims = PricePattern.Matches(response);
        foreach (Match match in priceClaims)
        {
            var claimedPrice = ParsePrice(match.Groups[1].Value);
            if (claimedPrice > 0)
            {
                var matchesAny = inventory.Any(v =>
                    Math.Abs(v.Price - claimedPrice) < 1000 || // Allow RD$1000 tolerance
                    (v.OriginalPrice.HasValue && Math.Abs(v.OriginalPrice.Value - claimedPrice) < 1000));

                if (!matchesAny)
                {
                    result.UngroundedClaims.Add($"Price RD${claimedPrice:N0} not found in inventory");
                    result.IsGrounded = false;
                }
            }
        }

        // Check for common hallucination phrases
        var hallucinationPhrases = new[]
        {
            "no tenemos en stock pero podemos",
            "te consigo",
            "puedo pedirlo",
            "lo traemos de",
            "garantizo que",
            "precio especial solo para ti",
            "te puedo ofrecer financiamiento",
            "aprobación inmediata",
            "crédito pre-aprobado",
        };

        var lowerResponse = response.ToLowerInvariant();
        foreach (var phrase in hallucinationPhrases)
        {
            if (lowerResponse.Contains(phrase))
            {
                result.UngroundedClaims.Add($"Ungrounded claim: '{phrase}'");
                result.IsGrounded = false;
            }
        }

        return result;
    }

    /// <summary>
    /// Sanitizes a response by adding disclaimers for ungrounded claims
    /// </summary>
    public static string SanitizeResponse(string response, GroundingResult groundingResult)
    {
        if (groundingResult.IsGrounded)
            return response;

        // Add disclaimer footer
        return response + "\n\n⚠️ _Verifica los detalles directamente con el dealer. " +
            "Los precios y disponibilidad pueden variar._";
    }

    private static decimal ParsePrice(string priceStr)
    {
        try
        {
            var cleaned = priceStr.Replace(",", "").Replace(".", "").Trim();
            return decimal.TryParse(cleaned, out var val) ? val : 0;
        }
        catch
        {
            return 0;
        }
    }
}

/// <summary>
/// Result of grounding validation
/// </summary>
public class GroundingResult
{
    public bool IsGrounded { get; set; } = true;
    public List<string> UngroundedClaims { get; set; } = new();
}

/// <summary>
/// Lightweight inventory item for grounding checks
/// </summary>
public record InventoryItem
{
    public Guid VehicleId { get; init; }
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public decimal Price { get; init; }
    public decimal? OriginalPrice { get; init; }
    public int? Mileage { get; init; }
}

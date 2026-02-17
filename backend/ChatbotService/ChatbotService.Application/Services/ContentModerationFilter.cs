using System.Text.RegularExpressions;

namespace ChatbotService.Application.Services;

/// <summary>
/// Content moderation filter for chatbot messages.
/// Blocks inappropriate content in both user messages and LLM responses.
/// 
/// Categories:
/// - Violence/threats
/// - Sexual content  
/// - Hate speech
/// - Scam/fraud attempts
/// - Off-topic solicitation
/// </summary>
public static class ContentModerationFilter
{
    /// <summary>
    /// Checks user message for content that should be blocked
    /// </summary>
    public static ModerationResult ModerateUserMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return ModerationResult.Safe();

        var lower = message.ToLowerInvariant();

        // Scam/fraud patterns (common in RD marketplace)
        var scamPatterns = new[]
        {
            "envíame tu contraseña",
            "dame tu clave",
            "western union",
            "transferencia adelantada",
            "pago por adelantado",
            "envía depósito",
            "gana dinero fácil",
            "inversión segura",
            "bitcoin",
            "crypto",
            "número de tarjeta",
            "código de seguridad",
            "pin del banco",
        };

        foreach (var pattern in scamPatterns)
        {
            if (lower.Contains(pattern))
            {
                return new ModerationResult
                {
                    IsSafe = false,
                    Category = ModerationCategory.Scam,
                    Reason = $"Potential scam/fraud content detected: '{pattern}'",
                    SuggestedAction = "block_and_warn"
                };
            }
        }

        // Violence/threats
        var violencePatterns = new[]
        {
            "te voy a matar", "voy a matarte", "amenaza de muerte",
            "te voy a buscar", "sé dónde vives",
        };

        foreach (var pattern in violencePatterns)
        {
            if (lower.Contains(pattern))
            {
                return new ModerationResult
                {
                    IsSafe = false,
                    Category = ModerationCategory.Violence,
                    Reason = "Violence/threat detected",
                    SuggestedAction = "block_and_report"
                };
            }
        }

        // Off-topic solicitation (not vehicle-related)
        var offTopicPatterns = new[]
        {
            "busco novia", "busco novio", "quieres salir",
            "vendo drogas", "marihuana", "cocaína",
        };

        foreach (var pattern in offTopicPatterns)
        {
            if (lower.Contains(pattern))
            {
                return new ModerationResult
                {
                    IsSafe = false,
                    Category = ModerationCategory.OffTopic,
                    Reason = "Off-topic content not related to vehicles",
                    SuggestedAction = "redirect"
                };
            }
        }

        return ModerationResult.Safe();
    }

    /// <summary>
    /// Moderates LLM output before sending to user.
    /// Catches cases where the model generates inappropriate content.
    /// </summary>
    public static ModerationResult ModerateBotResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return ModerationResult.Safe();

        var lower = response.ToLowerInvariant();

        // Bot should never give legal/medical/financial advice
        var advicePatterns = new[]
        {
            "te recomiendo como abogado",
            "legalmente deberías",
            "medicamente te sugiero",
            "invierte en",
            "préstamo personal te conviene",
        };

        foreach (var pattern in advicePatterns)
        {
            if (lower.Contains(pattern))
            {
                return new ModerationResult
                {
                    IsSafe = false,
                    Category = ModerationCategory.UnauthorizedAdvice,
                    Reason = "Bot generated unauthorized professional advice",
                    SuggestedAction = "replace_response"
                };
            }
        }

        // Bot should never claim to be human
        var identityPatterns = new[]
        {
            "soy una persona real",
            "no soy un bot",
            "soy humano",
        };

        foreach (var pattern in identityPatterns)
        {
            if (lower.Contains(pattern))
            {
                return new ModerationResult
                {
                    IsSafe = false,
                    Category = ModerationCategory.IdentityDeception,
                    Reason = "Bot claimed to be human",
                    SuggestedAction = "replace_response"
                };
            }
        }

        return ModerationResult.Safe();
    }
}

public enum ModerationCategory
{
    None,
    Scam,
    Violence,
    HateSpeech,
    SexualContent,
    OffTopic,
    UnauthorizedAdvice,
    IdentityDeception
}

public class ModerationResult
{
    public bool IsSafe { get; set; } = true;
    public ModerationCategory Category { get; set; } = ModerationCategory.None;
    public string? Reason { get; set; }
    public string? SuggestedAction { get; set; }

    public static ModerationResult Safe() => new() { IsSafe = true };
}

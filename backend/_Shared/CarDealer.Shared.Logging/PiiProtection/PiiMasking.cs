namespace CarDealer.Shared.Logging.PiiProtection;

/// <summary>
/// Static utility for masking PII (Personally Identifiable Information) in log output.
/// Ley 172-13 compliance: ensures production logs never contain plaintext PII.
/// </summary>
public static class PiiMasking
{
    // ── Property name sets (case-insensitive matching) ───────────────────

    private static readonly HashSet<string> EmailProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email", "UserEmail", "BuyerEmail", "RecipientEmail",
        "ContactEmail", "RecipientAddress", "email"
    };

    private static readonly HashSet<string> PhoneProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Phone", "UserPhone", "BuyerPhone", "BusinessPhone",
        "WhatsApp", "phone", "From"
    };

    private static readonly HashSet<string> NameProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "FullName", "UserName", "BuyerName", "ProfileName",
        "FirstName", "LastName", "HandoffAgentName"
    };

    private static readonly HashSet<string> HighSensitivityProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "Secret", "ApiKey", "ApiSecret",
        "DocumentNumber", "Cedula", "CardNumber",
        "DataVaultToken", "WebhookSecret"
    };

    private static readonly HashSet<string> ContentProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Body", "Content", "Transcript", "UserPrompt",
        "AssistantResponse", "BotResponse"
    };

    private static readonly HashSet<string> IpProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "ClientIP", "IpAddress", "RemoteIp", "ip_address"
    };

    // ── Public API ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if the property name matches a known PII field.
    /// </summary>
    public static bool IsPiiProperty(string propertyName)
        => EmailProperties.Contains(propertyName)
        || PhoneProperties.Contains(propertyName)
        || NameProperties.Contains(propertyName)
        || HighSensitivityProperties.Contains(propertyName)
        || ContentProperties.Contains(propertyName)
        || IpProperties.Contains(propertyName);

    /// <summary>
    /// Masks a PII value based on the property name.
    /// </summary>
    public static string Mask(string propertyName, string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        if (EmailProperties.Contains(propertyName)) return MaskEmail(value);
        if (PhoneProperties.Contains(propertyName)) return MaskPhone(value);
        if (NameProperties.Contains(propertyName)) return MaskName(value);
        if (IpProperties.Contains(propertyName)) return MaskIp(value);
        if (ContentProperties.Contains(propertyName)) return MaskContent(value);
        if (HighSensitivityProperties.Contains(propertyName)) return MaskSensitive(value);

        return value;
    }

    // ── Masking strategies ──────────────────────────────────────────────

    /// <summary>"user@example.com" → "us***@***le.com"</summary>
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return email;
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0) return "***@***";
        var local = email[..atIndex];
        var domain = email[(atIndex + 1)..];
        var maskedLocal = local.Length <= 2 ? "***" : local[..2] + "***";
        var maskedDomain = domain.Length <= 4 ? "***" : "***" + domain[^4..];
        return $"{maskedLocal}@{maskedDomain}";
    }

    /// <summary>"809-555-1234" → "***1234"</summary>
    public static string MaskPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return phone;
        // Strip non-digit characters for consistent masking
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length <= 4) return "***";
        return "***" + digits[^4..];
    }

    /// <summary>"Juan Pérez" → "Ju***"</summary>
    public static string MaskName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (name.Length <= 2) return "***";
        return name[..2] + "***";
    }

    /// <summary>"192.168.1.100" → "192.168.*.*"</summary>
    public static string MaskIp(string ip)
    {
        if (string.IsNullOrEmpty(ip)) return ip;
        var parts = ip.Split('.');
        if (parts.Length == 4) return $"{parts[0]}.{parts[1]}.*.*";
        // IPv6 or other: mask entirely
        return ip.Length > 6 ? ip[..6] + "***" : "***";
    }

    /// <summary>"Hello, I want to buy..." → "[REDACTED 28 chars]"</summary>
    public static string MaskContent(string content)
    {
        if (string.IsNullOrEmpty(content)) return content;
        return $"[REDACTED {content.Length} chars]";
    }

    /// <summary>Any high-sensitivity value → "***MASKED***"</summary>
    public static string MaskSensitive(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return "***MASKED***";
    }
}

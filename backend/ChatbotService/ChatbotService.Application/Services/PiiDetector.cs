using System.Text.RegularExpressions;

namespace ChatbotService.Application.Services;

/// <summary>
/// PII (Personally Identifiable Information) detector and sanitizer.
/// Implements pre-LLM sanitization as designed in FASE_1_PROMPTS/10_pii_detection.md.
/// 
/// Uses regex-based detection (NOT prompt-based) to ensure PII never reaches the LLM.
/// Supports Dominican Republic specific formats: cédula, RNC, phone numbers.
/// </summary>
public static class PiiDetector
{
    // ── Dominican Republic Cédula (ID) ──────────────────────────────
    // Format: 001-1234567-8 or 00112345678 (11 digits)
    private static readonly Regex CedulaPattern = new(
        @"\b\d{3}[-\s]?\d{7}[-\s]?\d{1}\b",
        RegexOptions.Compiled);

    // ── RNC (Registro Nacional del Contribuyente) ───────────────────
    // Format: 1-01-12345-6 or 101123456 (9 digits) or 1-01-12345-67 (11 digits)
    private static readonly Regex RncPattern = new(
        @"\b\d{1}[-\s]?\d{2}[-\s]?\d{5}[-\s]?\d{1,2}\b",
        RegexOptions.Compiled);

    // ── Credit/Debit Card Numbers ───────────────────────────────────
    // Visa, MasterCard, AMEX — 13-19 digits with optional spaces/dashes
    private static readonly Regex CreditCardPattern = new(
        @"\b(?:\d{4}[-\s]?){3,4}\d{1,4}\b",
        RegexOptions.Compiled);

    // ── Dominican Phone Numbers ─────────────────────────────────────
    // Format: 809-555-1234, 829-555-1234, 849-555-1234, +1-809-...
    private static readonly Regex PhonePattern = new(
        @"(?:\+?1[-\s]?)?(?:809|829|849)[-\s]?\d{3}[-\s]?\d{4}\b",
        RegexOptions.Compiled);

    // ── Email Addresses ─────────────────────────────────────────────
    private static readonly Regex EmailPattern = new(
        @"\b[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}\b",
        RegexOptions.Compiled);

    // ── Bank Account Numbers (IBAN or local format) ─────────────────
    private static readonly Regex BankAccountPattern = new(
        @"\b(?:DO\d{2}\s?\d{4}\s?\d{4}\s?\d{4}\s?\d{4}\s?\d{4}|\d{10,20})\b",
        RegexOptions.Compiled);

    // ── SSN / Passport Numbers ──────────────────────────────────────
    private static readonly Regex PassportPattern = new(
        @"\b[A-Z]{1,2}\d{6,9}\b",
        RegexOptions.Compiled);

    // ── CVV / Security Codes ────────────────────────────────────────
    private static readonly Regex CvvPattern = new(
        @"\bcvv\s*[:=]?\s*\d{3,4}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Sanitizes PII from user input before sending to the LLM.
    /// Returns the sanitized message and detection results.
    /// </summary>
    public static PiiSanitizationResult Sanitize(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return new PiiSanitizationResult(message, new PiiDetectionInfo());

        var result = message;
        var info = new PiiDetectionInfo();

        // 1. Credit Card — HIGHEST PRIORITY (triggers agent transfer)
        if (CreditCardPattern.IsMatch(result))
        {
            // Additional validation: Luhn check for actual card numbers
            var matches = CreditCardPattern.Matches(result);
            foreach (Match match in matches)
            {
                var digits = new string(match.Value.Where(char.IsDigit).ToArray());
                if (digits.Length >= 13 && digits.Length <= 19 && PassesLuhnCheck(digits))
                {
                    info.HasCreditCard = true;
                    info.DetectedTypes.Add("credit_card");
                    result = result.Replace(match.Value, "[TARJETA_REDACTADA]");
                }
            }
        }

        // 2. CVV / Security code
        if (CvvPattern.IsMatch(result))
        {
            info.HasCreditCard = true; // CVV implies payment data
            info.DetectedTypes.Add("cvv");
            result = CvvPattern.Replace(result, "[CVV_REDACTADO]");
        }

        // 3. Dominican Cédula
        if (CedulaPattern.IsMatch(result))
        {
            var matches = CedulaPattern.Matches(result);
            foreach (Match match in matches)
            {
                var digits = new string(match.Value.Where(char.IsDigit).ToArray());
                if (digits.Length == 11) // Valid cédula length
                {
                    info.HasCedula = true;
                    info.DetectedTypes.Add("cedula");
                    result = result.Replace(match.Value, "[CÉDULA_REDACTADA]");
                }
            }
        }

        // 4. RNC
        if (RncPattern.IsMatch(result))
        {
            info.DetectedTypes.Add("rnc");
            result = RncPattern.Replace(result, "[RNC_REDACTADO]");
        }

        // 5. Email — only sanitize in user messages, keep for contact requests
        // We replace with a placeholder but note it was detected
        if (EmailPattern.IsMatch(result))
        {
            info.HasEmail = true;
            info.DetectedTypes.Add("email");
            // Don't sanitize emails — they're needed for contact requests
            // Just flag them for the handler to decide
        }

        // 6. Phone — same as email, flag but don't remove
        if (PhonePattern.IsMatch(result))
        {
            info.HasPhone = true;
            info.DetectedTypes.Add("phone");
            // Don't sanitize phones — needed for contact requests
        }

        // 7. Bank Account
        if (BankAccountPattern.IsMatch(result))
        {
            info.DetectedTypes.Add("bank_account");
            result = BankAccountPattern.Replace(result, "[CUENTA_REDACTADA]");
        }

        // 8. Passport
        if (PassportPattern.IsMatch(result))
        {
            info.DetectedTypes.Add("passport");
            result = PassportPattern.Replace(result, "[PASAPORTE_REDACTADO]");
        }

        info.WasSanitized = result != message;

        return new PiiSanitizationResult(result, info);
    }

    /// <summary>
    /// Sanitizes PII from LLM output before returning to the user.
    /// Ensures the model doesn't echo back any PII from the conversation.
    /// </summary>
    public static string SanitizeResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return response;

        var result = response;

        // Remove any credit card numbers from response
        result = CreditCardPattern.Replace(result, "[DATO_PROTEGIDO]");
        result = CvvPattern.Replace(result, "[DATO_PROTEGIDO]");

        // Remove any cédulas from response
        var cedulaMatches = CedulaPattern.Matches(result);
        foreach (Match match in cedulaMatches)
        {
            var digits = new string(match.Value.Where(char.IsDigit).ToArray());
            if (digits.Length == 11)
                result = result.Replace(match.Value, "[DATO_PROTEGIDO]");
        }

        // Remove bank accounts
        result = BankAccountPattern.Replace(result, "[DATO_PROTEGIDO]");

        return result;
    }

    /// <summary>
    /// Luhn algorithm to validate credit card numbers.
    /// </summary>
    private static bool PassesLuhnCheck(string digits)
    {
        int sum = 0;
        bool alternate = false;
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int n = digits[i] - '0';
            if (alternate)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }
            sum += n;
            alternate = !alternate;
        }
        return sum % 10 == 0;
    }
}

/// <summary>
/// Result of PII sanitization.
/// </summary>
public record PiiSanitizationResult(string SanitizedMessage, PiiDetectionInfo DetectionInfo);

/// <summary>
/// Information about detected PII types.
/// </summary>
public class PiiDetectionInfo
{
    public bool HasCreditCard { get; set; }
    public bool HasCedula { get; set; }
    public bool HasEmail { get; set; }
    public bool HasPhone { get; set; }
    public bool WasSanitized { get; set; }
    public List<string> DetectedTypes { get; set; } = new();

    /// <summary>
    /// Returns true if critical PII was detected that requires immediate agent transfer.
    /// </summary>
    public bool RequiresAgentTransfer => HasCreditCard;
}

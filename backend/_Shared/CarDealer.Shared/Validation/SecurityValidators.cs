using System.Text.RegularExpressions;

namespace CarDealer.Shared.Validation;

/// <summary>
/// Centralized security validation utilities for detecting SQL Injection and XSS patterns.
/// This is the SINGLE SOURCE OF TRUTH for security validation across all microservices.
/// 
/// Usage with FluentValidation (in each service's Application layer):
///   RuleFor(x => x.Email).Must(SecurityValidationHelper.IsSafeFromSqlInjection)
///                        .Must(SecurityValidationHelper.IsSafeFromXss);
/// 
/// Or use the FluentValidation extension methods from CarDealer.Shared.Validation.FluentValidation
/// (requires FluentValidation package reference in the consuming project):
///   RuleFor(x => x.Email).NoSqlInjection().NoXss();
/// </summary>
public static class SecurityValidationHelper
{
    /// <summary>
    /// SQL keywords and patterns that indicate potential SQL injection attempts.
    /// Comprehensive list covering OWASP Top 10 injection vectors.
    /// </summary>
    private static readonly string[] SqlKeywords = new[]
    {
        "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER",
        "EXEC", "EXECUTE", "UNION", "DECLARE", "CAST", "CONVERT",
        "--", "/*", "*/", "xp_", "sp_", "INFORMATION_SCHEMA", "SYSOBJECTS",
        "SYSCOLUMNS", "TABLE_NAME", "COLUMN_NAME", "OR 1=1", "OR '1'='1'",
        "WAITFOR DELAY", "BENCHMARK", "SLEEP("
    };

    /// <summary>
    /// XSS patterns covering script injection, event handlers, and dangerous protocols.
    /// </summary>
    private static readonly string[] XssPatterns = new[]
    {
        "<script", "</script>", "javascript:", "onerror=", "onload=", "onclick=",
        "onmouseover=", "onfocus=", "onblur=", "<iframe", "</iframe>", "<object",
        "</object>", "<embed", "</embed>", "eval(", "expression(", "vbscript:",
        "data:text/html", "<svg", "onanimationstart=", "onanimationend=",
        "ontransitionend=", "<img", "src=", "alert(", "confirm(", "prompt("
    };

    /// <summary>
    /// Pre-compiled regex patterns for advanced XSS detection.
    /// Compiled once at class load time for maximum performance.
    /// </summary>
    private static readonly Regex[] XssRegexPatterns = new[]
    {
        new Regex(@"<script[\s\S]*?>[\s\S]*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"javascript\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<iframe[\s\S]*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<object[\s\S]*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<embed[\s\S]*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<img[\s\S]*?onerror\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"data:text/html", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"eval\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"expression\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    };

    /// <summary>
    /// Checks if the input is safe from SQL injection patterns.
    /// Returns true if safe, false if injection detected.
    /// </summary>
    public static bool IsSafeFromSqlInjection(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        var upperInput = input.ToUpperInvariant();
        return !SqlKeywords.Any(keyword => upperInput.Contains(keyword));
    }

    /// <summary>
    /// Checks if the input is safe from XSS patterns.
    /// Returns true if safe, false if XSS detected.
    /// </summary>
    public static bool IsSafeFromXss(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        var lowerInput = input.ToLowerInvariant();
        return !XssPatterns.Any(pattern => lowerInput.Contains(pattern));
    }

    /// <summary>
    /// Advanced XSS check using pre-compiled regex patterns.
    /// Returns true if safe, false if XSS detected.
    /// </summary>
    public static bool IsSafeFromXssAdvanced(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        return !XssRegexPatterns.Any(regex => regex.IsMatch(input));
    }

    /// <summary>
    /// Combined check for both SQL injection and XSS.
    /// Returns true if safe from both threats.
    /// </summary>
    public static bool IsSafeFromSecurityThreats(string? input)
    {
        return IsSafeFromSqlInjection(input) && IsSafeFromXss(input);
    }
}

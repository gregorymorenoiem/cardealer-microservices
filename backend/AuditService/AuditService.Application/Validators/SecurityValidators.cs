using FluentValidation;

namespace AuditService.Application.Validators;

/// <summary>
/// Validadores de seguridad para detectar SQL Injection y XSS en inputs
/// </summary>
public static class SecurityValidators
{
    public static IRuleBuilderOptions<T, string> NoSqlInjection<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(value => !SqlInjectionValidator.ContainsSqlKeywords(value))
            .WithMessage("El valor contiene palabras clave SQL no permitidas");
    }

    public static IRuleBuilderOptions<T, string> NoXss<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(value => !XssValidator.ContainsXssPatterns(value))
            .WithMessage("El valor contiene patrones XSS no permitidos");
    }

    public static IRuleBuilderOptions<T, string> NoXssAdvanced<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(value => !XssValidator.ContainsXssPatternsAdvanced(value))
            .WithMessage("El valor contiene patrones XSS avanzados no permitidos");
    }

    public static IRuleBuilderOptions<T, string> NoSecurityThreats<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NoSqlInjection()
            .NoXss()
            .NoXssAdvanced();
    }
}

public static class SqlInjectionValidator
{
    private static readonly string[] SqlKeywords = new[]
    {
        "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER", "EXEC", "EXECUTE",
        "UNION", "WHERE", "OR", "AND", "--", "/*", "*/", "xp_", "sp_", "0x", "char(", "nchar(",
        "varchar(", "nvarchar(", "syscolumns", "sysobjects", "information_schema"
    };

    public static bool ContainsSqlKeywords(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var upperValue = value.ToUpperInvariant();
        return SqlKeywords.Any(keyword => upperValue.Contains(keyword.ToUpperInvariant()));
    }
}

public static class XssValidator
{
    private static readonly string[] XssPatterns = new[]
    {
        "<script", "</script>", "javascript:", "onerror=", "onload=", "onclick=", "onmouseover=",
        "eval(", "expression(", "vbscript:", "document.cookie", "document.write", "window.location",
        "innerHTML", "outerHTML", ".appendChild", ".createElement", "<iframe", "<object", "<embed",
        "<img", "src=", "onmessage=", "onbeforeunload="
    };

    public static bool ContainsXssPatterns(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var lowerValue = value.ToLowerInvariant();
        return XssPatterns.Any(pattern => lowerValue.Contains(pattern.ToLowerInvariant()));
    }

    public static bool ContainsXssPatternsAdvanced(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Regex patterns para XSS más sofisticados
        var patterns = new[]
        {
            @"<\s*script", // <script, < script
            @"on\w+\s*=", // onclick=, onerror=, etc.
            @"javascript\s*:", // javascript:
            @"<\s*iframe", // <iframe
            @"<\s*img[^>]+src\s*=" // <img src=
        };

        return patterns.Any(pattern => System.Text.RegularExpressions.Regex.IsMatch(value, pattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }
}

public static class SecurityValidator
{
    public static bool IsSecure(string? value)
    {
        return !SqlInjectionValidator.ContainsSqlKeywords(value) &&
               !XssValidator.ContainsXssPatterns(value) &&
               !XssValidator.ContainsXssPatternsAdvanced(value);
    }
}

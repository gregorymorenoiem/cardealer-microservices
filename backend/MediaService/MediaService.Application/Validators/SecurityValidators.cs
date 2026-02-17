using FluentValidation;
using System.Text.RegularExpressions;

namespace MediaService.Application.Validators;

public static class SecurityValidators
{
    private static readonly string[] SqlInjectionKeywords = new[]
    {
        "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER",
        "EXEC", "EXECUTE", "UNION", "DECLARE", "CAST", "CONVERT",
        "--", "/*", "*/", "xp_", "sp_", "INFORMATION_SCHEMA",
        "SYSOBJECTS", "SYSCOLUMNS", "TABLE_SCHEMA", "@@VERSION",
        "WAITFOR", "DELAY", "BENCHMARK", "SLEEP", "SHUTDOWN"
    };

    private static readonly string[] XssPatterns = new[]
    {
        "<script", "</script>", "javascript:", "onerror=", "onload=",
        "onclick=", "onmouseover=", "<iframe", "</iframe>", "<object",
        "</object>", "<embed", "</embed>", "<img", "src=",
        "eval(", "expression(", "vbscript:", "data:text/html",
        "base64", "<svg", "onanimationend=", "onanimationstart=",
        "ontransitionend=", "<link"
    };

    public static IRuleBuilderOptions<T, string> NoSqlInjection<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(value =>
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            var upperValue = value.ToUpperInvariant();
            return !SqlInjectionKeywords.Any(keyword => upperValue.Contains(keyword));
        })
        .WithMessage("Potential SQL injection detected")
        .WithErrorCode("SECURITY_SQL_INJECTION");
    }

    public static IRuleBuilderOptions<T, string> NoXss<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(value =>
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            var lowerValue = value.ToLowerInvariant();
            return !XssPatterns.Any(pattern => lowerValue.Contains(pattern));
        })
        .WithMessage("Potential XSS attack detected")
        .WithErrorCode("SECURITY_XSS");
    }

    public static IRuleBuilderOptions<T, string> SafeFileName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(value =>
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            // No path traversal
            if (value.Contains("..") || value.Contains("/") || value.Contains("\\"))
                return false;

            // No invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            if (value.Any(c => invalidChars.Contains(c)))
                return false;

            // No executable extensions
            var dangerousExtensions = new[] { ".exe", ".bat", ".cmd", ".sh", ".ps1", ".dll", ".so" };
            return !dangerousExtensions.Any(ext => value.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        })
        .WithMessage("Invalid or unsafe filename")
        .WithErrorCode("SECURITY_UNSAFE_FILENAME");
    }

    public static IRuleBuilderOptions<T, string> SafeMimeType<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        // SECURITY: SVG removed — it can contain <script> tags (stored XSS vector)
        var allowedMimeTypes = new[]
        {
            "image/jpeg", "image/png", "image/gif", "image/webp",
            "video/mp4", "video/mpeg", "video/webm",
            "application/pdf", "application/zip"
        };

        return ruleBuilder.Must(value => allowedMimeTypes.Contains(value?.ToLowerInvariant()))
            .WithMessage("MIME type not allowed")
            .WithErrorCode("SECURITY_INVALID_MIME_TYPE");
    }
}

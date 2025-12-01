using FluentValidation;

namespace NotificationService.Application.Validators;

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

    public static IRuleBuilderOptions<T, string> SafeEmailAddress<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .EmailAddress()
            .NoXss()
            .WithMessage("Invalid or unsafe email address")
            .WithErrorCode("SECURITY_UNSAFE_EMAIL");
    }

    public static IRuleBuilderOptions<T, string> SafePhoneNumber<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(value =>
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            // Only allow digits, +, -, (), and spaces
            return System.Text.RegularExpressions.Regex.IsMatch(value, @"^[\d\s\+\-\(\)]+$");
        })
        .WithMessage("Invalid phone number format")
        .WithErrorCode("SECURITY_INVALID_PHONE");
    }
}

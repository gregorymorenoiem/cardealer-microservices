using FluentValidation;
using AdminService.Application.DTOs;

namespace AdminService.Application.Validators;

// ============================================
// Security Validators (SQL Injection, XSS)
// ============================================
public static class SecurityValidators
{
    private static readonly string[] SqlKeywords = new[]
    {
        "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER",
        "EXEC", "EXECUTE", "XP_", "SP_", "UNION", "DECLARE", "CAST", "CONVERT",
        "--", "/*", "*/", "INFORMATION_SCHEMA", "SYSOBJECTS", "SYSCOLUMNS",
        "WAITFOR DELAY", "BENCHMARK", "SLEEP(", "OR 1=1", "OR '1'='1'"
    };

    private static readonly string[] XssPatterns = new[]
    {
        "<script", "</script>", "javascript:", "vbscript:", "onerror=", "onload=",
        "onclick=", "onmouseover=", "onfocus=", "onblur=", "<iframe", "</iframe>",
        "<object", "<embed", "<svg", "eval(", "expression(", "alert(", "confirm(",
        "prompt(", "data:text/html", "onanimationstart=", "onanimationend=", "ontransitionend="
    };

    public static IRuleBuilderOptions<T, string> NoSqlInjection<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(input =>
        {
            if (string.IsNullOrWhiteSpace(input)) return true;
            var upperInput = input.ToUpperInvariant();
            return !SqlKeywords.Any(keyword => upperInput.Contains(keyword));
        })
        .WithMessage("Input contains potential SQL injection patterns.");
    }

    public static IRuleBuilderOptions<T, string> NoXss<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(input =>
        {
            if (string.IsNullOrWhiteSpace(input)) return true;
            var lowerInput = input.ToLowerInvariant();
            return !XssPatterns.Any(pattern => lowerInput.Contains(pattern));
        })
        .WithMessage("Input contains potential XSS attack patterns.");
    }
}

// ============================================
// Moderation Action Validator
// ============================================
public class ModerationActionDtoValidator : AbstractValidator<ModerationActionDto>
{
    public ModerationActionDtoValidator()
    {
        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

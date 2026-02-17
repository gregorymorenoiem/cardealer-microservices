using FluentValidation;
using BillingService.Application.DTOs;

namespace BillingService.Application.Validators;

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
// Payment Validators
// ============================================
public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.Method)
            .NotEmpty().WithMessage("Payment method is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class RefundPaymentRequestValidator : AbstractValidator<RefundPaymentRequest>
{
    public RefundPaymentRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Refund reason is required.")
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss();
    }
}

public class AddPaymentMethodRequestValidator : AbstractValidator<AddPaymentMethodRequest>
{
    public AddPaymentMethodRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Payment method type is required.")
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Token)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Token));

        RuleFor(x => x.AccountType)
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.AccountType));
    }
}

// ============================================
// Invoice Validators
// ============================================
public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.LineItems)
            .MaximumLength(5000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.LineItems));
    }
}

// ============================================
// Subscription Validators
// ============================================
public class CreateSubscriptionRequestValidator : AbstractValidator<CreateSubscriptionRequest>
{
    public CreateSubscriptionRequestValidator()
    {
        RuleFor(x => x.Plan)
            .NotEmpty().WithMessage("Plan is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Cycle)
            .NotEmpty().WithMessage("Billing cycle is required.")
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Features)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Features));
    }
}

public class UpgradeSubscriptionRequestValidator : AbstractValidator<UpgradeSubscriptionRequest>
{
    public UpgradeSubscriptionRequestValidator()
    {
        RuleFor(x => x.Plan)
            .NotEmpty().WithMessage("Plan is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();
    }
}

public class ChangeBillingCycleRequestValidator : AbstractValidator<ChangeBillingCycleRequest>
{
    public ChangeBillingCycleRequestValidator()
    {
        RuleFor(x => x.Cycle)
            .NotEmpty().WithMessage("Billing cycle is required.")
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss();
    }
}

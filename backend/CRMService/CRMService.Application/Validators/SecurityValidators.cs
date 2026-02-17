using FluentValidation;
using CRMService.Application.DTOs;

namespace CRMService.Application.Validators;

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
// Lead Validators
// ============================================
public class CreateLeadRequestValidator : AbstractValidator<CreateLeadRequest>
{
    public CreateLeadRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Company)
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Company));

        RuleFor(x => x.JobTitle)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.JobTitle));

        RuleFor(x => x.Source)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();
    }
}

public class UpdateLeadRequestValidator : AbstractValidator<UpdateLeadRequest>
{
    public UpdateLeadRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Company)
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Company));

        RuleFor(x => x.JobTitle)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.JobTitle));

        RuleFor(x => x.Status)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Status));
    }
}

public class ConvertLeadRequestValidator : AbstractValidator<ConvertLeadRequest>
{
    public ConvertLeadRequestValidator()
    {
        RuleFor(x => x.DealTitle)
            .NotEmpty().WithMessage("Deal title is required.")
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss();
    }
}

// ============================================
// Deal Validators
// ============================================
public class CreateDealRequestValidator : AbstractValidator<CreateDealRequest>
{
    public CreateDealRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Currency)
            .MaximumLength(10)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.VIN)
            .MaximumLength(17)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.VIN));
    }
}

public class UpdateDealRequestValidator : AbstractValidator<UpdateDealRequest>
{
    public UpdateDealRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Currency)
            .MaximumLength(10)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.VIN)
            .MaximumLength(17)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.VIN));
    }
}

public class CloseDealRequestValidator : AbstractValidator<CloseDealRequest>
{
    public CloseDealRequestValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.LostReason)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.LostReason));
    }
}

// ============================================
// Activity Validators
// ============================================
public class CreateActivityRequestValidator : AbstractValidator<CreateActivityRequest>
{
    public CreateActivityRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Activity type is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Priority)
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss();
    }
}

public class UpdateActivityRequestValidator : AbstractValidator<UpdateActivityRequest>
{
    public UpdateActivityRequestValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Priority)
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss();
    }
}

public class CompleteActivityRequestValidator : AbstractValidator<CompleteActivityRequest>
{
    public CompleteActivityRequestValidator()
    {
        RuleFor(x => x.Outcome)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Outcome));
    }
}

// ============================================
// Pipeline Validators
// ============================================
public class CreatePipelineRequestValidator : AbstractValidator<CreatePipelineRequest>
{
    public CreatePipelineRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Pipeline name is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleForEach(x => x.Stages)
            .SetValidator(new CreateStageRequestValidator())
            .When(x => x.Stages != null && x.Stages.Count > 0);
    }
}

public class UpdatePipelineRequestValidator : AbstractValidator<UpdatePipelineRequest>
{
    public UpdatePipelineRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Pipeline name is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class CreateStageRequestValidator : AbstractValidator<CreateStageRequest>
{
    public CreateStageRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Stage name is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Color)
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Color));
    }
}

public class UpdateStageRequestValidator : AbstractValidator<UpdateStageRequest>
{
    public UpdateStageRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Stage name is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Color)
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Color));
    }
}

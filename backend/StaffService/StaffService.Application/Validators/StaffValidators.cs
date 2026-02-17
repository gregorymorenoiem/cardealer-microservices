using FluentValidation;
using StaffService.Application.Features.Invitations.Commands;
using StaffService.Application.Features.Staff.Commands;

namespace StaffService.Application.Validators;

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
// Create Staff From Invitation Validator
// ============================================
public class CreateStaffFromInvitationCommandValidator : AbstractValidator<CreateStaffFromInvitationCommand>
{
    public CreateStaffFromInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

// ============================================
// Create Invitation Validator
// ============================================
public class CreateInvitationCommandValidator : AbstractValidator<CreateInvitationCommand>
{
    public CreateInvitationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role specified.");

        RuleFor(x => x.Message)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Message));
    }
}

// ============================================
// Accept Invitation Validator
// ============================================
public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

// ============================================
// Update Staff Validator
// ============================================
public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Staff ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.EmployeeCode)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.EmployeeCode));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

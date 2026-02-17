using FluentValidation;
using AppointmentService.Application.DTOs;

namespace AppointmentService.Application.Validators;

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
// Appointment Validators
// ============================================
public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Customer email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Appointment type is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.")
            .MaximumLength(10)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.CustomerPhone)
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.CustomerPhone));

        RuleFor(x => x.VehicleDescription)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.VehicleDescription));

        RuleFor(x => x.AssignedToUserName)
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.AssignedToUserName));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Location)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Location));
    }
}

public class UpdateAppointmentRequestValidator : AbstractValidator<UpdateAppointmentRequest>
{
    public UpdateAppointmentRequestValidator()
    {
        RuleFor(x => x.VehicleDescription)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.VehicleDescription));

        RuleFor(x => x.AssignedToUserName)
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.AssignedToUserName));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Location)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Location));
    }
}

public class RescheduleAppointmentRequestValidator : AbstractValidator<RescheduleAppointmentRequest>
{
    public RescheduleAppointmentRequestValidator()
    {
        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.")
            .MaximumLength(10)
            .NoSqlInjection()
            .NoXss();
    }
}

// ============================================
// TimeSlot Validators
// ============================================
public class CreateTimeSlotRequestValidator : AbstractValidator<CreateTimeSlotRequest>
{
    public CreateTimeSlotRequestValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .NotEmpty().WithMessage("Day of week is required.")
            .MaximumLength(20)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.")
            .MaximumLength(10)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required.")
            .MaximumLength(10)
            .NoSqlInjection()
            .NoXss();
    }
}

public class UpdateTimeSlotRequestValidator : AbstractValidator<UpdateTimeSlotRequest>
{
    public UpdateTimeSlotRequestValidator()
    {
        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.")
            .MaximumLength(10)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required.")
            .MaximumLength(10)
            .NoSqlInjection()
            .NoXss();
    }
}

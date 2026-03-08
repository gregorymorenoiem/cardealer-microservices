using FluentValidation;
using AdminService.Application.UseCases.PlatformEmployees;

namespace AdminService.Application.Validators.PlatformEmployees;

/// <summary>
/// Validator for InvitePlatformEmployeeCommand.
/// Validates Email, Role, Permissions, Department, Notes with NoSqlInjection/NoXss.
/// </summary>
public class InvitePlatformEmployeeCommandValidator : AbstractValidator<InvitePlatformEmployeeCommand>
{
    private static readonly string[] ValidRoles = { "SuperAdmin", "Admin", "Moderator", "Support", "Viewer" };

    public InvitePlatformEmployeeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(254)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => ValidRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}.")
            .NoSqlInjection()
            .NoXss();

        RuleForEach(x => x.Permissions)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => x.Permissions != null && x.Permissions.Length > 0);

        RuleFor(x => x.Department)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Department));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.InvitedBy)
            .NotEmpty().WithMessage("InvitedBy is required.");
    }
}

/// <summary>
/// Validator for UpdatePlatformEmployeeCommand.
/// Validates Role, Permissions, Department, Notes, Status with NoSqlInjection/NoXss.
/// </summary>
public class UpdatePlatformEmployeeCommandValidator : AbstractValidator<UpdatePlatformEmployeeCommand>
{
    private static readonly string[] ValidRoles = { "SuperAdmin", "Admin", "Moderator", "Support", "Viewer" };
    private static readonly string[] ValidStatuses = { "Active", "Suspended", "Inactive" };

    public UpdatePlatformEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required.");

        RuleFor(x => x.Role!)
            .Must(role => string.IsNullOrEmpty(role) || ValidRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}.")
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Role));

        RuleForEach(x => x.Permissions)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => x.Permissions != null && x.Permissions.Length > 0);

        RuleFor(x => x.Department)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Department));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Status!)
            .Must(status => string.IsNullOrEmpty(status) || ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.")
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Status));
    }
}

/// <summary>
/// Validator for SuspendPlatformEmployeeCommand.
/// Validates Reason with NoSqlInjection/NoXss.
/// </summary>
public class SuspendPlatformEmployeeCommandValidator : AbstractValidator<SuspendPlatformEmployeeCommand>
{
    public SuspendPlatformEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}

/// <summary>
/// Validator for AcceptPlatformInvitationCommand.
/// ⚠️ CRITICAL: Contains Password field — full validation required.
/// </summary>
public class AcceptPlatformInvitationCommandValidator : AbstractValidator<AcceptPlatformInvitationCommand>
{
    public AcceptPlatformInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Invitation token is required.")
            .MinimumLength(20).WithMessage("Invalid token format.")
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

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .Matches(@"^\+?[\d\s\-()]{7,20}$").WithMessage("Invalid phone number format.")
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

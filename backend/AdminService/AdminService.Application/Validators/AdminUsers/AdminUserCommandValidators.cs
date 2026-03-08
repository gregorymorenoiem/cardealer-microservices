using FluentValidation;
using AdminService.Application.UseCases.AdminUsers;

namespace AdminService.Application.Validators.AdminUsers;

/// <summary>
/// Validator for UpdateAdminRoleCommand.
/// Validates Role and Permissions with NoSqlInjection/NoXss.
/// </summary>
public class UpdateAdminRoleCommandValidator : AbstractValidator<UpdateAdminRoleCommand>
{
    private static readonly string[] ValidRoles = { "SuperAdmin", "Admin", "Moderator", "Support", "Viewer" };

    public UpdateAdminRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

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
    }
}

/// <summary>
/// Validator for SuspendAdminUserCommand.
/// Validates Reason with NoSqlInjection/NoXss.
/// </summary>
public class SuspendAdminUserCommandValidator : AbstractValidator<SuspendAdminUserCommand>
{
    public SuspendAdminUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}

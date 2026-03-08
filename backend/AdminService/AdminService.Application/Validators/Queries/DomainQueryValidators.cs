using FluentValidation;
using AdminService.Application.UseCases.Dealers;
using AdminService.Application.UseCases.PlatformUsers;
using AdminService.Application.UseCases.AdminUsers;
using AdminService.Application.UseCases.PlatformEmployees;

namespace AdminService.Application.Validators.Queries;

// ─────────────────────────────────────────────────────
// DEALERS
// ─────────────────────────────────────────────────────

/// <summary>
/// Validator for GetDealersQuery.
/// Validates Search, Status, Plan filters with NoSqlInjection/NoXss.
/// </summary>
public class GetDealersQueryValidator : AbstractValidator<GetDealersQuery>
{
    public GetDealersQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Search));

        RuleFor(x => x.Status)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Plan)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Plan));

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

// ─────────────────────────────────────────────────────
// PLATFORM USERS
// ─────────────────────────────────────────────────────

/// <summary>
/// Validator for GetPlatformUsersQuery.
/// Validates Search, Type, Status filters with NoSqlInjection/NoXss.
/// </summary>
public class GetPlatformUsersQueryValidator : AbstractValidator<GetPlatformUsersQuery>
{
    public GetPlatformUsersQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Search));

        RuleFor(x => x.Type)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Type));

        RuleFor(x => x.Status)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

// ─────────────────────────────────────────────────────
// ADMIN USERS
// ─────────────────────────────────────────────────────

/// <summary>
/// Validator for GetAdminUsersQuery.
/// Validates Role filter with NoSqlInjection/NoXss.
/// </summary>
public class GetAdminUsersQueryValidator : AbstractValidator<GetAdminUsersQuery>
{
    public GetAdminUsersQueryValidator()
    {
        RuleFor(x => x.Role)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Role));

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

/// <summary>
/// Validator for GetAdminActivityQuery.
/// Validates Action filter with NoSqlInjection/NoXss.
/// </summary>
public class GetAdminActivityQueryValidator : AbstractValidator<GetAdminActivityQuery>
{
    public GetAdminActivityQueryValidator()
    {
        RuleFor(x => x.Action)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Action));

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

// ─────────────────────────────────────────────────────
// PLATFORM EMPLOYEES
// ─────────────────────────────────────────────────────

/// <summary>
/// Validator for GetPlatformEmployeesQuery.
/// Validates Status, Role, Department filters with NoSqlInjection/NoXss.
/// </summary>
public class GetPlatformEmployeesQueryValidator : AbstractValidator<GetPlatformEmployeesQuery>
{
    public GetPlatformEmployeesQueryValidator()
    {
        RuleFor(x => x.Status)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Role)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Role));

        RuleFor(x => x.Department)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Department));

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

/// <summary>
/// Validator for GetPlatformInvitationsQuery.
/// Validates Status filter with NoSqlInjection/NoXss.
/// </summary>
public class GetPlatformInvitationsQueryValidator : AbstractValidator<GetPlatformInvitationsQuery>
{
    public GetPlatformInvitationsQueryValidator()
    {
        RuleFor(x => x.Status)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Status));
    }
}

/// <summary>
/// Validator for ValidatePlatformInvitationQuery.
/// Validates Token with NoSqlInjection/NoXss.
/// </summary>
public class ValidatePlatformInvitationQueryValidator : AbstractValidator<ValidatePlatformInvitationQuery>
{
    public ValidatePlatformInvitationQueryValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.")
            .MinimumLength(20).WithMessage("Invalid token format.")
            .NoSqlInjection()
            .NoXss();
    }
}

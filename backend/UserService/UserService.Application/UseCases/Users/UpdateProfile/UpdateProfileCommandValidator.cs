using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Users.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.FirstName!)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName!)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Phone!)
            .MaximumLength(20)
            .NoSqlInjection()
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Bio!)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));

        RuleFor(x => x.Location!)
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.Location));

        RuleFor(x => x.City!)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.Province!)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.Province));

        RuleFor(x => x.PreferredLocale!)
            .MaximumLength(10)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.PreferredLocale));

        RuleFor(x => x.PreferredCurrency!)
            .MaximumLength(10)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.PreferredCurrency));
    }
}

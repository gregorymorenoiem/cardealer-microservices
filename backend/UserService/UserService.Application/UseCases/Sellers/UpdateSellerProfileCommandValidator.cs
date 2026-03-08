using FluentValidation;
using UserService.Application.DTOs;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Sellers.UpdateSellerProfile;

public class UpdateSellerProfileCommandValidator : AbstractValidator<UpdateSellerProfileCommand>
{
    public UpdateSellerProfileCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("SellerId is required");

        RuleFor(x => x.Request).NotNull().WithMessage("Request body is required");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.FullName!)
                .MaximumLength(200)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.FullName));

            RuleFor(x => x.Request.Bio!)
                .MaximumLength(1000)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Bio));

            RuleFor(x => x.Request.Nationality!)
                .MaximumLength(100)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Nationality));

            RuleFor(x => x.Request.AvatarUrl!)
                .MaximumLength(500)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.AvatarUrl));

            RuleFor(x => x.Request.WhatsApp!)
                .MaximumLength(20)
                .NoSqlInjection()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.WhatsApp));

            RuleFor(x => x.Request.Address!)
                .MaximumLength(500)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Address));

            RuleFor(x => x.Request.City!)
                .MaximumLength(100)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.City));

            RuleFor(x => x.Request.State!)
                .MaximumLength(100)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.State));

            RuleFor(x => x.Request.ZipCode!)
                .MaximumLength(20)
                .NoSqlInjection()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.ZipCode));

            RuleFor(x => x.Request.Country!)
                .MaximumLength(10)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Country));

            RuleFor(x => x.Request.PreferredContactMethod!)
                .MaximumLength(50)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.PreferredContactMethod));
        });
    }
}

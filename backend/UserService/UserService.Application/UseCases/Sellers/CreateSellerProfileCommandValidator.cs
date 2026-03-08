using FluentValidation;
using UserService.Application.DTOs;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Sellers.CreateSellerProfile;

public class CreateSellerProfileCommandValidator : AbstractValidator<CreateSellerProfileCommand>
{
    public CreateSellerProfileCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Request body is required");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(x => x.Request.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(200)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Request.Nationality!)
                .MaximumLength(100)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Nationality));

            RuleFor(x => x.Request.WhatsApp!)
                .MaximumLength(20)
                .NoSqlInjection()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.WhatsApp));

            RuleFor(x => x.Request.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(500)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Request.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Request.State)
                .NotEmpty().WithMessage("State is required")
                .MaximumLength(100)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Request.ZipCode!)
                .MaximumLength(20)
                .NoSqlInjection()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.ZipCode));

            RuleFor(x => x.Request.Country)
                .NotEmpty().WithMessage("Country is required")
                .MaximumLength(10)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Request.PreferredContactMethod!)
                .MaximumLength(50)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.PreferredContactMethod));
        });
    }
}

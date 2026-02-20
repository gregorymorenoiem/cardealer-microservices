using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Sellers.ConvertBuyerToSeller;

public class ConvertBuyerToSellerCommandValidator : AbstractValidator<ConvertBuyerToSellerCommand>
{
    public ConvertBuyerToSellerCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Request body is required.");

        RuleFor(x => x.Request.AcceptTerms)
            .Equal(true)
            .WithMessage("You must accept the terms and conditions to become a seller.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.PreferredContactMethod!)
                .MaximumLength(50)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.PreferredContactMethod));

            RuleFor(x => x.Request.Bio!)
                .MaximumLength(500)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Bio));
        });

        RuleFor(x => x.IdempotencyKey!)
            .MaximumLength(128)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.IdempotencyKey));
    }
}

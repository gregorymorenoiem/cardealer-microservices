using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Sellers.VerifySellerProfile;

public class VerifySellerProfileCommandValidator : AbstractValidator<VerifySellerProfileCommand>
{
    public VerifySellerProfileCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("SellerId is required");

        RuleFor(x => x.Request).NotNull().WithMessage("Request body is required");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.VerifiedByUserId)
                .NotEmpty().WithMessage("VerifiedByUserId is required");

            RuleFor(x => x.Request.Notes!)
                .MaximumLength(1000)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Notes));
        });
    }
}

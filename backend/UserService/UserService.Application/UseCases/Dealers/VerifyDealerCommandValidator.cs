using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Dealers.VerifyDealer;

public class VerifyDealerCommandValidator : AbstractValidator<VerifyDealerCommand>
{
    public VerifyDealerCommandValidator()
    {
        RuleFor(x => x.DealerId)
            .NotEmpty().WithMessage("DealerId is required");

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

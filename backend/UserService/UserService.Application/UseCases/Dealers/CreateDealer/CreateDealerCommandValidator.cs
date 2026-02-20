using FluentValidation;
using UserService.Application.DTOs;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Dealers.CreateDealer;

public class CreateDealerCommandValidator : AbstractValidator<CreateDealerCommand>
{
    public CreateDealerCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Request body is required");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.OwnerUserId)
                .NotEmpty().WithMessage("OwnerUserId is required");

            RuleFor(x => x.Request.BusinessName)
                .NotEmpty().WithMessage("Business name is required")
                .MaximumLength(150)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Request.TradeName)
                .MaximumLength(150)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.TradeName));

            RuleFor(x => x.Request.Description)
                .MaximumLength(2000)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Description));

            RuleFor(x => x.Request.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress()
                .MaximumLength(254)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Request.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .MaximumLength(20)
                .NoSqlInjection();

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

            RuleFor(x => x.Request.BusinessRegistrationNumber)
                .MaximumLength(50)
                .NoSqlInjection()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.BusinessRegistrationNumber));

            RuleFor(x => x.Request.TaxId)
                .MaximumLength(50)
                .NoSqlInjection()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.TaxId));

            RuleFor(x => x.Request.Website)
                .MaximumLength(500)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Website));

            RuleFor(x => x.Request.WhatsApp)
                .MaximumLength(20)
                .NoSqlInjection()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.WhatsApp));
        });
    }
}

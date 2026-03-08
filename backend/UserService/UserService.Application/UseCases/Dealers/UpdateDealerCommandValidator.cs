using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Dealers.UpdateDealer;

public class UpdateDealerCommandValidator : AbstractValidator<UpdateDealerCommand>
{
    public UpdateDealerCommandValidator()
    {
        RuleFor(x => x.DealerId)
            .NotEmpty().WithMessage("DealerId is required");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body is required");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.BusinessName!)
                .MaximumLength(150)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.BusinessName));

            RuleFor(x => x.Request.TradeName!)
                .MaximumLength(150)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.TradeName));

            RuleFor(x => x.Request.Description!)
                .MaximumLength(2000)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Description));

            RuleFor(x => x.Request.Email!)
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(254)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Email));

            RuleFor(x => x.Request.Phone!)
                .MaximumLength(20)
                .NoSqlInjection()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Phone));

            RuleFor(x => x.Request.WhatsApp!)
                .MaximumLength(20)
                .NoSqlInjection()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.WhatsApp));

            RuleFor(x => x.Request.Website!)
                .MaximumLength(500)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Website));

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

            RuleFor(x => x.Request.LogoUrl!)
                .MaximumLength(500)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.LogoUrl));

            RuleFor(x => x.Request.BannerUrl!)
                .MaximumLength(500)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.BannerUrl));

            RuleFor(x => x.Request.PrimaryColor!)
                .MaximumLength(20)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.PrimaryColor));

            RuleFor(x => x.Request.BusinessHours!)
                .MaximumLength(1000)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.BusinessHours));

            RuleFor(x => x.Request.SocialMediaLinks!)
                .MaximumLength(2000)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.SocialMediaLinks));
        });
    }
}

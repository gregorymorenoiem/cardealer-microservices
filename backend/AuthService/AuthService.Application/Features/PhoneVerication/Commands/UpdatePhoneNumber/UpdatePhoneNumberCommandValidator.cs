using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.PhoneVerification.Commands.UpdatePhoneNumber;

public class UpdatePhoneNumberCommandValidator : AbstractValidator<UpdatePhoneNumberCommand>
{
    public UpdatePhoneNumberCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.NewPhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.")
            .NoSqlInjection()
            .NoXss();
    }
}

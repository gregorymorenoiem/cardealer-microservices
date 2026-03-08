using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.TwoFactor.Commands.VerifyPhoneNumber;

public class VerifyPhoneNumberCommandValidator : AbstractValidator<VerifyPhoneNumberCommand>
{
    public VerifyPhoneNumberCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.VerificationCode)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 characters long.")
            .Matches(@"^\d+$").WithMessage("Verification code must contain only digits.")
            .NoSqlInjection()
            .NoXss();
    }
}

using FluentValidation;

namespace AuthService.Application.Features.PhoneVerification.Commands.UpdatePhoneNumber;

public class UpdatePhoneNumberCommandValidator : AbstractValidator<UpdatePhoneNumberCommand>
{
    public UpdatePhoneNumberCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.NewPhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.");
    }
}

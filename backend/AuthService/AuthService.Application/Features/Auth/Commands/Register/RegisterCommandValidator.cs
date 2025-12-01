// File: backend/AuthService/AuthService.Application/UseCases/Register/RegisterCommandValidator.cs
using FluentValidation;
using AuthService.Application.Features.Auth.Commands.Register;
using AuthService.Application.Validators;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("This field is required.")
            .NoXss() // ✅ NUEVO: Protección XSS
            .NoSqlInjection(); // ✅ NUEVO: Protección SQL Injection

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("This field is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                .WithMessage("Email must be in the format name@example.com.")
            .NoXss() // ✅ NUEVO
            .NoSqlInjection(); // ✅ NUEVO

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("This field is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"^(?=.*[A-Z])(?=.*\d).+$")
                .WithMessage("Password must include at least one uppercase letter and one number.");
    }
}

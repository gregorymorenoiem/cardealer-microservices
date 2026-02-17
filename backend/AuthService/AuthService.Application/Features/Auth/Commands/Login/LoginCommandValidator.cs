// File: backend/AuthService/AuthService.Application/UseCases/Login/LoginCommandValidator.cs
using FluentValidation;
using AuthService.Application.Features.Auth.Commands.Login;
using AuthService.Application.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                .WithMessage("El email debe tener el formato nombre@ejemplo.com")
            .MaximumLength(254).WithMessage("El email no puede exceder 254 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
            .MaximumLength(128).WithMessage("La contraseña no puede exceder 128 caracteres")
            .NoXss() // ✅ AGREGADO: Protección XSS
            .NoSqlInjection(); // ✅ AGREGADO: Protección SQL Injection

        // CaptchaToken validation (only if provided)
        RuleFor(x => x.CaptchaToken)
            .MaximumLength(1000).WithMessage("Token de captcha inválido")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.CaptchaToken));
    }
}

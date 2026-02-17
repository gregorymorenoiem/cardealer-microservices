// File: backend/AuthService/AuthService.Application/UseCases/Register/RegisterCommandValidator.cs
using FluentValidation;
using AuthService.Application.Features.Auth.Commands.Register;
using AuthService.Application.Validators;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        // UserName is now optional since we use FirstName/LastName
        RuleFor(x => x.UserName)
            .NoXss() // ✅ Protección XSS
            .NoSqlInjection() // ✅ Protección SQL Injection
            .When(x => !string.IsNullOrEmpty(x.UserName));

        // FirstName - Required if UserName not provided
        RuleFor(x => x.FirstName)
            .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.UserName))
                .WithMessage("El nombre es requerido")
            .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        // LastName - Required if UserName not provided
        RuleFor(x => x.LastName)
            .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.UserName))
                .WithMessage("El apellido es requerido")
            .MaximumLength(50).WithMessage("El apellido no puede exceder 50 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.LastName));

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
            .Matches(@"[A-Z]").WithMessage("La contraseña debe incluir al menos una mayúscula")
            .Matches(@"[a-z]").WithMessage("La contraseña debe incluir al menos una minúscula")
            .Matches(@"\d").WithMessage("La contraseña debe incluir al menos un número")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("La contraseña debe incluir al menos un carácter especial")
            .NoXss() // ✅ AGREGADO: Protección XSS
            .NoSqlInjection(); // ✅ AGREGADO: Protección SQL Injection

        // Phone - Optional but validated if provided
        RuleFor(x => x.Phone)
            .Matches(@"^[+]?[\d\s\-()]{7,20}$")
                .When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Formato de teléfono inválido")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.Phone));

        // AcceptTerms - Must be true for registration
        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("Debes aceptar los términos y condiciones");
    }
}

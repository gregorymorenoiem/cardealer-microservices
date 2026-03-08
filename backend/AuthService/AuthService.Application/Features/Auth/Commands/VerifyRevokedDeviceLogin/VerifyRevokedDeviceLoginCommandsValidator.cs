using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.Auth.Commands.VerifyRevokedDeviceLogin;

public class RequestRevokedDeviceLoginCommandValidator : AbstractValidator<RequestRevokedDeviceLoginCommand>
{
    public RequestRevokedDeviceLoginCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido")
            .MaximumLength(128).WithMessage("El ID de usuario no puede exceder 128 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido")
            .MaximumLength(254).WithMessage("El email no puede exceder 254 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.DeviceFingerprint)
            .NotEmpty().WithMessage("La huella del dispositivo es requerida")
            .MaximumLength(512).WithMessage("La huella del dispositivo no puede exceder 512 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.IpAddress)
            .MaximumLength(45).WithMessage("La dirección IP no puede exceder 45 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.IpAddress));

        RuleFor(x => x.UserAgent)
            .MaximumLength(512).WithMessage("El User-Agent no puede exceder 512 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.UserAgent));

        RuleFor(x => x.Browser)
            .MaximumLength(128).WithMessage("El nombre del navegador no puede exceder 128 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.Browser));

        RuleFor(x => x.OperatingSystem)
            .MaximumLength(128).WithMessage("El sistema operativo no puede exceder 128 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.OperatingSystem));
    }
}

public class VerifyRevokedDeviceLoginCommandValidator : AbstractValidator<VerifyRevokedDeviceLoginCommand>
{
    public VerifyRevokedDeviceLoginCommandValidator()
    {
        RuleFor(x => x.VerificationToken)
            .NotEmpty().WithMessage("El token de verificación es requerido")
            .MaximumLength(512).WithMessage("El token de verificación no puede exceder 512 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código de verificación es requerido")
            .Length(6).WithMessage("El código debe tener 6 dígitos")
            .Matches(@"^\d{6}$").WithMessage("El código debe contener solo dígitos")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.IpAddress)
            .MaximumLength(45).WithMessage("La dirección IP no puede exceder 45 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.IpAddress));
    }
}

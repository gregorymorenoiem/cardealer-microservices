using FluentValidation;
using RegulatoryAlertService.Application.DTOs;

namespace RegulatoryAlertService.Application.Validators;

public class CreateAlertValidator : AbstractValidator<CreateAlertDto>
{
    public CreateAlertValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(300).WithMessage("El título no puede exceder 300 caracteres");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es requerida");

        RuleFor(x => x.AlertType)
            .IsInEnum().WithMessage("Tipo de alerta inválido");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Prioridad inválida");

        RuleFor(x => x.RegulatoryBody)
            .IsInEnum().WithMessage("Ente regulador inválido");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Categoría inválida");
    }
}

public class CreateSubscriptionValidator : AbstractValidator<CreateSubscriptionDto>
{
    public CreateSubscriptionValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.Frequency)
            .IsInEnum().WithMessage("Frecuencia inválida");

        RuleFor(x => x.PreferredChannel)
            .IsInEnum().WithMessage("Canal de notificación inválido");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email inválido")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class CreateCalendarEntryValidator : AbstractValidator<CreateCalendarEntryDto>
{
    public CreateCalendarEntryValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es requerida");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("La fecha de vencimiento es requerida")
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha debe ser futura");

        RuleFor(x => x.RegulatoryBody)
            .IsInEnum().WithMessage("Ente regulador inválido");

        RuleFor(x => x.ReminderDaysBefore)
            .InclusiveBetween(1, 90).WithMessage("El recordatorio debe estar entre 1 y 90 días");
    }
}

public class CreateDeadlineValidator : AbstractValidator<CreateDeadlineDto>
{
    public CreateDeadlineValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("La fecha de vencimiento es requerida");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Prioridad inválida");
    }
}

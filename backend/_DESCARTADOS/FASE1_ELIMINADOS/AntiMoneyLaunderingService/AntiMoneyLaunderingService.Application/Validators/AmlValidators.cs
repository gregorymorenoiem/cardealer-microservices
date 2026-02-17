// =====================================================
// AntiMoneyLaunderingService - Validators
// Ley 155-17 Prevención de Lavado de Activos (PLD)
// =====================================================

using FluentValidation;
using AntiMoneyLaunderingService.Application.DTOs;

namespace AntiMoneyLaunderingService.Application.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId es requerido");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nombre completo es requerido")
            .MaximumLength(200).WithMessage("Nombre completo no puede exceder 200 caracteres");

        RuleFor(x => x.IdentificationType)
            .NotEmpty().WithMessage("Tipo de identificación es requerido")
            .Must(x => new[] { "Cedula", "Passport", "Rnc", "ForeignId" }.Contains(x))
            .WithMessage("Tipo de identificación no válido");

        RuleFor(x => x.IdentificationNumber)
            .NotEmpty().WithMessage("Número de identificación es requerido")
            .MaximumLength(50).WithMessage("Número de identificación no puede exceder 50 caracteres");

        RuleFor(x => x.EstimatedMonthlyIncome)
            .GreaterThanOrEqualTo(0).When(x => x.EstimatedMonthlyIncome.HasValue)
            .WithMessage("Ingreso mensual estimado debe ser mayor o igual a 0");
    }
}

public class CreateTransactionValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId es requerido");

        RuleFor(x => x.TransactionReference)
            .NotEmpty().WithMessage("Referencia de transacción es requerida")
            .MaximumLength(100).WithMessage("Referencia no puede exceder 100 caracteres");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Monto debe ser mayor a 0");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Moneda es requerida")
            .Length(3).WithMessage("Código de moneda debe tener 3 caracteres");

        RuleFor(x => x.TransactionType)
            .NotEmpty().WithMessage("Tipo de transacción es requerido");
    }
}

public class CreateSuspiciousActivityReportValidator : AbstractValidator<CreateSuspiciousActivityReportDto>
{
    public CreateSuspiciousActivityReportValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId es requerido");

        RuleFor(x => x.ReportType)
            .NotEmpty().WithMessage("Tipo de reporte es requerido");

        RuleFor(x => x.TransactionAmount)
            .GreaterThan(0).WithMessage("Monto debe ser mayor a 0");

        RuleFor(x => x.SuspicionIndicators)
            .NotEmpty().WithMessage("Indicadores de sospecha son requeridos")
            .MinimumLength(20).WithMessage("Debe proporcionar una descripción detallada de los indicadores");
    }
}

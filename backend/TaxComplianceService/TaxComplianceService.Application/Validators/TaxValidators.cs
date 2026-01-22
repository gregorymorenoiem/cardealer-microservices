// =====================================================
// TaxComplianceService - Validators
// Ley 11-92 Código Tributario de República Dominicana
// =====================================================

using FluentValidation;
using TaxComplianceService.Application.DTOs;

namespace TaxComplianceService.Application.Validators;

public class CreateTaxpayerValidator : AbstractValidator<CreateTaxpayerDto>
{
    public CreateTaxpayerValidator()
    {
        RuleFor(x => x.Rnc)
            .NotEmpty().WithMessage("RNC es requerido")
            .Matches(@"^\d{9}$|^\d{11}$").WithMessage("RNC debe tener 9 dígitos (persona física) o 11 dígitos (persona jurídica)");

        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("Razón social es requerida")
            .MaximumLength(200).WithMessage("Razón social no puede exceder 200 caracteres");

        RuleFor(x => x.TaxpayerType)
            .NotEmpty().WithMessage("Tipo de contribuyente es requerido")
            .Must(x => new[] { "Individual", "LegalEntity", "NonProfit", "Government" }.Contains(x))
            .WithMessage("Tipo de contribuyente no válido");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email no válido");
    }
}

public class CreateTaxDeclarationValidator : AbstractValidator<CreateTaxDeclarationDto>
{
    public CreateTaxDeclarationValidator()
    {
        RuleFor(x => x.TaxpayerId)
            .NotEmpty().WithMessage("TaxpayerId es requerido");

        RuleFor(x => x.DeclarationType)
            .NotEmpty().WithMessage("Tipo de declaración es requerido");

        RuleFor(x => x.Period)
            .NotEmpty().WithMessage("Período es requerido")
            .Matches(@"^\d{6}$").WithMessage("Período debe tener formato YYYYMM");

        RuleFor(x => x.GrossAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Monto bruto debe ser mayor o igual a 0");

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Monto de impuesto debe ser mayor o igual a 0");
    }
}

public class CreateNcfSequenceValidator : AbstractValidator<CreateNcfSequenceDto>
{
    public CreateNcfSequenceValidator()
    {
        RuleFor(x => x.TaxpayerId)
            .NotEmpty().WithMessage("TaxpayerId es requerido");

        RuleFor(x => x.NcfType)
            .NotEmpty().WithMessage("Tipo de NCF es requerido");

        RuleFor(x => x.Serie)
            .NotEmpty().WithMessage("Serie es requerida")
            .Length(1).WithMessage("Serie debe tener 1 carácter");

        RuleFor(x => x.StartNumber)
            .GreaterThan(0).WithMessage("Número inicial debe ser mayor a 0");

        RuleFor(x => x.EndNumber)
            .GreaterThan(x => x.StartNumber).WithMessage("Número final debe ser mayor al inicial");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Fecha de expiración debe ser futura");
    }
}

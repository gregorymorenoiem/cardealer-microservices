// =====================================================
// ConsumerProtectionService - Validators
// Ley 358-05 Derechos del Consumidor de RD
// =====================================================

using FluentValidation;
using ConsumerProtectionService.Application.DTOs;

namespace ConsumerProtectionService.Application.Validators;

public class CreateWarrantyValidator : AbstractValidator<CreateWarrantyDto>
{
    public CreateWarrantyValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId es requerido");

        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("SellerId es requerido");

        RuleFor(x => x.WarrantyType)
            .NotEmpty().WithMessage("Tipo de garantía es requerido")
            .Must(x => new[] { "Legal", "Extended", "Manufacturer", "Distributor", "Commercial" }.Contains(x))
            .WithMessage("Tipo de garantía no válido");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Fecha de inicio es requerida");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Fecha de fin es requerida")
            .GreaterThan(x => x.StartDate).WithMessage("Fecha de fin debe ser posterior a la de inicio");

        // Ley 358-05: Mínimo 6 meses para productos nuevos
        RuleFor(x => x)
            .Must(x => (x.EndDate - x.StartDate).TotalDays >= 180)
            .WithMessage("Según Ley 358-05, la garantía mínima para productos nuevos es 6 meses");
    }
}

public class CreateWarrantyClaimValidator : AbstractValidator<CreateWarrantyClaimDto>
{
    public CreateWarrantyClaimValidator()
    {
        RuleFor(x => x.WarrantyId)
            .NotEmpty().WithMessage("WarrantyId es requerido");

        RuleFor(x => x.ConsumerId)
            .NotEmpty().WithMessage("ConsumerId es requerido");

        RuleFor(x => x.IssueDescription)
            .NotEmpty().WithMessage("Descripción del problema es requerida")
            .MinimumLength(20).WithMessage("Descripción debe tener al menos 20 caracteres");
    }
}

public class CreateComplaintValidator : AbstractValidator<CreateComplaintDto>
{
    public CreateComplaintValidator()
    {
        RuleFor(x => x.ConsumerId)
            .NotEmpty().WithMessage("ConsumerId es requerido");

        RuleFor(x => x.ComplaintType)
            .NotEmpty().WithMessage("Tipo de reclamación es requerido")
            .Must(x => new[] { "ProductDefect", "ServiceFailure", "FalseAdvertising", "PriceIssue", "WarrantyDenied", "Fraud", "Other" }.Contains(x))
            .WithMessage("Tipo de reclamación no válido");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descripción es requerida")
            .MinimumLength(50).WithMessage("Descripción debe tener al menos 50 caracteres para una reclamación válida");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Prioridad es requerida")
            .Must(x => new[] { "Low", "Medium", "High", "Critical" }.Contains(x))
            .WithMessage("Prioridad no válida");
    }
}

public class CreateMediationValidator : AbstractValidator<CreateMediationDto>
{
    public CreateMediationValidator()
    {
        RuleFor(x => x.ComplaintId)
            .NotEmpty().WithMessage("ComplaintId es requerido");

        RuleFor(x => x.ScheduledDate)
            .NotEmpty().WithMessage("Fecha programada es requerida")
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de mediación debe ser futura");
    }
}

using FluentValidation;
using LegalDocumentService.Application.DTOs;

namespace LegalDocumentService.Application.Validators;

public class CreateLegalDocumentValidator : AbstractValidator<CreateLegalDocumentDto>
{
    public CreateLegalDocumentValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(500).WithMessage("El título no puede exceder 500 caracteres");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("El contenido es requerido");

        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage("Tipo de documento inválido");

        RuleFor(x => x.Jurisdiction)
            .IsInEnum().WithMessage("Jurisdicción inválida");

        RuleFor(x => x.Language)
            .IsInEnum().WithMessage("Idioma inválido");
    }
}

public class UpdateLegalDocumentValidator : AbstractValidator<UpdateLegalDocumentDto>
{
    public UpdateLegalDocumentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID es requerido");

        RuleFor(x => x.Title)
            .MaximumLength(500).WithMessage("El título no puede exceder 500 caracteres")
            .When(x => x.Title != null);
    }
}

public class CreateAcceptanceValidator : AbstractValidator<CreateAcceptanceDto>
{
    public CreateAcceptanceValidator()
    {
        RuleFor(x => x.LegalDocumentId)
            .NotEmpty().WithMessage("El ID del documento es requerido");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("Método de aceptación inválido");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("La dirección IP es requerida")
            .MaximumLength(45).WithMessage("Dirección IP inválida");

        RuleFor(x => x.UserAgent)
            .NotEmpty().WithMessage("El User Agent es requerido");
    }
}

public class CreateTemplateValidator : AbstractValidator<CreateTemplateDto>
{
    public CreateTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(50).WithMessage("El código no puede exceder 50 caracteres")
            .Matches("^[A-Z0-9_]+$").WithMessage("El código solo puede contener letras mayúsculas, números y guiones bajos");

        RuleFor(x => x.TemplateContent)
            .NotEmpty().WithMessage("El contenido de la plantilla es requerido");

        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage("Tipo de documento inválido");
    }
}

public class CreateComplianceRequirementValidator : AbstractValidator<CreateComplianceRequirementDto>
{
    public CreateComplianceRequirementValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(50).WithMessage("El código no puede exceder 50 caracteres");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es requerida");

        RuleFor(x => x.LegalBasis)
            .NotEmpty().WithMessage("La base legal es requerida");

        RuleFor(x => x.Jurisdiction)
            .IsInEnum().WithMessage("Jurisdicción inválida");
    }
}

// ContractService - FluentValidation Validators

namespace ContractService.Application.Validators;

using FluentValidation;
using ContractService.Domain.Entities;
using ContractService.Application.Commands;

#region Template Validators

public class CreateTemplateValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(50).WithMessage("El código no puede exceder 50 caracteres")
            .Matches(@"^[A-Z0-9\-_]+$").WithMessage("El código solo puede contener letras mayúsculas, números, guiones y guiones bajos");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de contrato inválido");

        RuleFor(x => x.ContentHtml)
            .NotEmpty().WithMessage("El contenido HTML es requerido");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("El idioma es requerido")
            .MaximumLength(10).WithMessage("El idioma no puede exceder 10 caracteres");

        RuleFor(x => x.MinimumSignatures)
            .GreaterThan(0).WithMessage("Debe requerir al menos una firma");

        RuleFor(x => x.ValidityDays)
            .GreaterThan(0).When(x => x.ValidityDays.HasValue)
            .WithMessage("Los días de validez deben ser mayor a cero");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El creador es requerido");
    }
}

public class UpdateTemplateValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de plantilla es requerido");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.ContentHtml)
            .NotEmpty().WithMessage("El contenido HTML es requerido");

        RuleFor(x => x.MinimumSignatures)
            .GreaterThan(0).WithMessage("Debe requerir al menos una firma");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("El modificador es requerido");
    }
}

#endregion

#region Contract Validators

public class CreateContractValidator : AbstractValidator<CreateContractCommand>
{
    public CreateContractValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de contrato inválido");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(500).WithMessage("El título no puede exceder 500 caracteres");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty().WithMessage("La fecha de vigencia es requerida")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("La fecha de vigencia no puede ser en el pasado");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(x => x.EffectiveDate)
            .When(x => x.ExpirationDate.HasValue)
            .WithMessage("La fecha de expiración debe ser posterior a la fecha de vigencia");

        RuleFor(x => x.SubjectType)
            .NotEmpty().WithMessage("El tipo de sujeto es requerido");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("El ID del sujeto es requerido");

        RuleFor(x => x.Currency)
            .MaximumLength(3).WithMessage("La moneda debe tener máximo 3 caracteres (ej: DOP, USD)");

        RuleFor(x => x.ContractValue)
            .GreaterThanOrEqualTo(0).When(x => x.ContractValue.HasValue)
            .WithMessage("El valor del contrato no puede ser negativo");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El creador es requerido");

        RuleFor(x => x.Parties)
            .Must(p => p == null || p.Count >= 2)
            .WithMessage("Un contrato debe tener al menos 2 partes");
    }
}

public class FinalizeContractValidator : AbstractValidator<FinalizeContractCommand>
{
    public FinalizeContractValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID del contrato es requerido");

        RuleFor(x => x.FinalizedBy)
            .NotEmpty().WithMessage("El finalizador es requerido");
    }
}

public class TerminateContractValidator : AbstractValidator<TerminateContractCommand>
{
    public TerminateContractValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID del contrato es requerido");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("La razón de terminación es requerida")
            .MaximumLength(1000).WithMessage("La razón no puede exceder 1000 caracteres");

        RuleFor(x => x.TerminatedBy)
            .NotEmpty().WithMessage("El terminador es requerido");
    }
}

#endregion

#region Signature Validators

public class RequestSignatureValidator : AbstractValidator<RequestSignatureCommand>
{
    public RequestSignatureValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEmpty().WithMessage("El ID del contrato es requerido");

        RuleFor(x => x.PartyId)
            .NotEmpty().WithMessage("El ID de la parte es requerido");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de firma inválido");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpiresAt.HasValue)
            .WithMessage("La fecha de expiración debe ser en el futuro");

        RuleFor(x => x.RequestedBy)
            .NotEmpty().WithMessage("El solicitante es requerido");
    }
}

public class SignContractValidator : AbstractValidator<SignContractCommand>
{
    public SignContractValidator()
    {
        RuleFor(x => x.SignatureId)
            .NotEmpty().WithMessage("El ID de firma es requerido");

        RuleFor(x => x.SignatureData)
            .NotEmpty().WithMessage("Los datos de firma son requeridos");

        RuleFor(x => x.IPAddress)
            .NotEmpty().WithMessage("La dirección IP es requerida");
    }
}

public class DeclineSignatureValidator : AbstractValidator<DeclineSignatureCommand>
{
    public DeclineSignatureValidator()
    {
        RuleFor(x => x.SignatureId)
            .NotEmpty().WithMessage("El ID de firma es requerido");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("La razón del rechazo es requerida")
            .MaximumLength(500).WithMessage("La razón no puede exceder 500 caracteres");

        RuleFor(x => x.DeclinedBy)
            .NotEmpty().WithMessage("El rechazador es requerido");
    }
}

#endregion

#region Clause Validators

public class UpdateClauseValidator : AbstractValidator<UpdateClauseCommand>
{
    public UpdateClauseValidator()
    {
        RuleFor(x => x.ClauseId)
            .NotEmpty().WithMessage("El ID de la cláusula es requerido");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("El contenido es requerido");

        RuleFor(x => x.ModificationReason)
            .NotEmpty().WithMessage("La razón de modificación es requerida")
            .MaximumLength(500).WithMessage("La razón no puede exceder 500 caracteres");

        RuleFor(x => x.ModifiedBy)
            .NotEmpty().WithMessage("El modificador es requerido");
    }
}

#endregion

#region Document Validators

public class UploadDocumentValidator : AbstractValidator<UploadDocumentCommand>
{
    private static readonly string[] AllowedContentTypes = 
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/gif",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public UploadDocumentValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEmpty().WithMessage("El ID del contrato es requerido");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("El tipo de documento es requerido");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("El nombre de archivo es requerido")
            .MaximumLength(255).WithMessage("El nombre de archivo no puede exceder 255 caracteres");

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Tipo de archivo no permitido. Use PDF, imágenes o documentos Word");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("El tamaño de archivo debe ser mayor a cero")
            .LessThanOrEqualTo(52428800).WithMessage("El archivo no puede exceder 50MB"); // 50MB max

        RuleFor(x => x.StoragePath)
            .NotEmpty().WithMessage("La ruta de almacenamiento es requerida");

        RuleFor(x => x.FileHash)
            .NotEmpty().WithMessage("El hash del archivo es requerido");

        RuleFor(x => x.UploadedBy)
            .NotEmpty().WithMessage("El cargador es requerido");
    }
}

#endregion

#region Terms Validators

public class AcceptTermsValidator : AbstractValidator<AcceptTermsCommand>
{
    public AcceptTermsValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEmpty().WithMessage("El ID del contrato es requerido");

        RuleFor(x => x.AcceptedTerms)
            .Equal(true).WithMessage("Debe aceptar los términos y condiciones");

        RuleFor(x => x.AcceptedPrivacyPolicy)
            .Equal(true).WithMessage("Debe aceptar la política de privacidad");

        RuleFor(x => x.AcceptedBy)
            .NotEmpty().WithMessage("El aceptante es requerido");

        RuleFor(x => x.IPAddress)
            .NotEmpty().WithMessage("La dirección IP es requerida");
    }
}

#endregion

#region DTO Validators

public class CreatePartyDtoValidator : AbstractValidator<CreatePartyDto>
{
    public CreatePartyDtoValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de parte inválido");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Rol de parte inválido");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("El tipo de documento es requerido");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("El número de documento es requerido")
            .MaximumLength(50).WithMessage("El número de documento no puede exceder 50 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.RNC)
            .Matches(@"^\d{9}$").When(x => !string.IsNullOrEmpty(x.RNC))
            .WithMessage("El RNC debe tener 9 dígitos");
    }
}

#endregion

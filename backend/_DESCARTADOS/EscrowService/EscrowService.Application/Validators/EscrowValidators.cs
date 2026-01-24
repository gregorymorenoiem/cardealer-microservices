// EscrowService - FluentValidation Validators
// Alineado con Ley 126-02 (Comercio Electrónico) y Ley 155-17

namespace EscrowService.Application.Validators;

using FluentValidation;
using EscrowService.Application.Commands;
using EscrowService.Domain.Entities;

#region EscrowAccount Validators

public class CreateEscrowAccountValidator : AbstractValidator<CreateEscrowAccountCommand>
{
    public CreateEscrowAccountValidator()
    {
        RuleFor(x => x.TransactionType)
            .IsInEnum()
            .WithMessage("Tipo de transacción inválido.");

        RuleFor(x => x.BuyerId)
            .NotEmpty()
            .WithMessage("El ID del comprador es requerido.");

        RuleFor(x => x.BuyerName)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("El nombre del comprador es requerido (máximo 200 caracteres).");

        RuleFor(x => x.BuyerEmail)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("El email del comprador debe ser válido.");

        RuleFor(x => x.SellerId)
            .NotEmpty()
            .WithMessage("El ID del vendedor es requerido.");

        RuleFor(x => x.SellerName)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("El nombre del vendedor es requerido (máximo 200 caracteres).");

        RuleFor(x => x.SellerEmail)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("El email del vendedor debe ser válido.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithMessage("El monto total debe ser mayor a cero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Must(x => x == "DOP" || x == "USD")
            .WithMessage("La moneda debe ser DOP o USD.");

        RuleFor(x => x.ExpirationDays)
            .GreaterThan(0)
            .When(x => x.ExpirationDays.HasValue)
            .WithMessage("Los días de expiración deben ser mayores a cero.");

        RuleFor(x => x.SubjectType)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("El tipo de sujeto es requerido.");

        RuleFor(x => x.SubjectId)
            .NotEmpty()
            .WithMessage("El ID del sujeto es requerido.");

        RuleFor(x => x.SubjectDescription)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("La descripción del sujeto es requerida (máximo 500 caracteres).");
    }
}

public class FundEscrowValidator : AbstractValidator<FundEscrowCommand>
{
    public FundEscrowValidator()
    {
        RuleFor(x => x.EscrowAccountId)
            .NotEmpty()
            .WithMessage("El ID de la cuenta escrow es requerido.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto a depositar debe ser mayor a cero.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("Método de pago inválido.");

        RuleFor(x => x.BankReference)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.PaymentMethod == PaymentMethod.BankTransfer)
            .WithMessage("La referencia bancaria es requerida para transferencias.");

        RuleFor(x => x.FundedBy)
            .NotEmpty()
            .WithMessage("El usuario que fondea es requerido.");
    }
}

public class ReleaseEscrowValidator : AbstractValidator<ReleaseEscrowCommand>
{
    public ReleaseEscrowValidator()
    {
        RuleFor(x => x.EscrowAccountId)
            .NotEmpty()
            .WithMessage("El ID de la cuenta escrow es requerido.");

        RuleFor(x => x.DestinationAccount)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("La cuenta de destino es requerida (máximo 100 caracteres).");

        RuleFor(x => x.BankName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("El nombre del banco es requerido (máximo 100 caracteres).");

        RuleFor(x => x.ReleasedBy)
            .NotEmpty()
            .WithMessage("El usuario que libera los fondos es requerido.");
    }
}

public class RefundEscrowValidator : AbstractValidator<RefundEscrowCommand>
{
    public RefundEscrowValidator()
    {
        RuleFor(x => x.EscrowAccountId)
            .NotEmpty()
            .WithMessage("El ID de la cuenta escrow es requerido.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .When(x => x.Amount.HasValue)
            .WithMessage("El monto a reembolsar debe ser mayor a cero.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("El motivo del reembolso es requerido (máximo 500 caracteres).");

        RuleFor(x => x.RefundedBy)
            .NotEmpty()
            .WithMessage("El usuario que inicia el reembolso es requerido.");
    }
}

public class ApproveReleaseValidator : AbstractValidator<ApproveReleaseCommand>
{
    public ApproveReleaseValidator()
    {
        RuleFor(x => x.EscrowAccountId)
            .NotEmpty()
            .WithMessage("El ID de la cuenta escrow es requerido.");

        RuleFor(x => x.ApprovedBy)
            .NotEmpty()
            .WithMessage("El usuario que aprueba es requerido.");

        RuleFor(x => x.ApproverType)
            .Must(x => x == "Buyer" || x == "Seller")
            .WithMessage("El tipo de aprobador debe ser 'Buyer' o 'Seller'.");
    }
}

public class CancelEscrowValidator : AbstractValidator<CancelEscrowCommand>
{
    public CancelEscrowValidator()
    {
        RuleFor(x => x.EscrowAccountId)
            .NotEmpty()
            .WithMessage("El ID de la cuenta escrow es requerido.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("El motivo de cancelación es requerido (máximo 500 caracteres).");

        RuleFor(x => x.CancelledBy)
            .NotEmpty()
            .WithMessage("El usuario que cancela es requerido.");
    }
}

public class ExtendEscrowExpirationValidator : AbstractValidator<ExtendEscrowExpirationCommand>
{
    public ExtendEscrowExpirationValidator()
    {
        RuleFor(x => x.EscrowAccountId)
            .NotEmpty()
            .WithMessage("El ID de la cuenta escrow es requerido.");

        RuleFor(x => x.AdditionalDays)
            .GreaterThan(0)
            .WithMessage("Los días adicionales deben ser mayores a cero.");

        RuleFor(x => x.ExtendedBy)
            .NotEmpty()
            .WithMessage("El usuario que extiende es requerido.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("El motivo de la extensión es requerido (máximo 500 caracteres).");
    }
}

#endregion

#region Condition Validators

public class AddConditionValidator : AbstractValidator<AddConditionCommand>
{
    public AddConditionValidator()
    {
        RuleFor(x => x.EscrowAccountId)
            .NotEmpty()
            .WithMessage("El ID de la cuenta escrow es requerido.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Tipo de condición inválido.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("El nombre de la condición es requerido (máximo 100 caracteres).");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("La descripción no puede exceder 500 caracteres.");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(1)
            .WithMessage("El orden debe ser mayor o igual a 1.");
    }
}

public class MarkConditionMetValidator : AbstractValidator<MarkConditionMetCommand>
{
    public MarkConditionMetValidator()
    {
        RuleFor(x => x.ConditionId)
            .NotEmpty()
            .WithMessage("El ID de la condición es requerido.");

        RuleFor(x => x.VerifiedBy)
            .NotEmpty()
            .WithMessage("El verificador es requerido.");

        RuleFor(x => x.VerificationNotes)
            .MaximumLength(500)
            .WithMessage("Las notas no pueden exceder 500 caracteres.");
    }
}

public class MarkConditionFailedValidator : AbstractValidator<MarkConditionFailedCommand>
{
    public MarkConditionFailedValidator()
    {
        RuleFor(x => x.ConditionId)
            .NotEmpty()
            .WithMessage("El ID de la condición es requerido.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("El motivo del fallo es requerido (máximo 500 caracteres).");

        RuleFor(x => x.MarkedBy)
            .NotEmpty()
            .WithMessage("El usuario es requerido.");
    }
}

public class WaiveConditionValidator : AbstractValidator<WaiveConditionCommand>
{
    public WaiveConditionValidator()
    {
        RuleFor(x => x.ConditionId)
            .NotEmpty()
            .WithMessage("El ID de la condición es requerido.");

        RuleFor(x => x.WaivedBy)
            .NotEmpty()
            .WithMessage("El usuario que exonera es requerido.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("El motivo de la exoneración es requerido (máximo 500 caracteres).");
    }
}

#endregion

#region Document Validators

public class UploadDocumentValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentValidator()
    {
        RuleFor(x => x.EscrowAccountId)
            .NotEmpty()
            .WithMessage("El ID de la cuenta escrow es requerido.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("El nombre del documento es requerido (máximo 200 caracteres).");

        RuleFor(x => x.DocumentType)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("El tipo de documento es requerido (máximo 50 caracteres).");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("El nombre del archivo es requerido (máximo 255 caracteres).");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("El tipo de contenido es requerido.");

        RuleFor(x => x.UploadedBy)
            .NotEmpty()
            .WithMessage("El usuario que sube el documento es requerido.");
    }
}

public class VerifyDocumentValidator : AbstractValidator<VerifyDocumentCommand>
{
    public VerifyDocumentValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("El ID del documento es requerido.");

        RuleFor(x => x.VerifiedBy)
            .NotEmpty()
            .WithMessage("El verificador es requerido.");
    }
}

public class DeleteDocumentValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("El ID del documento es requerido.");

        RuleFor(x => x.DeletedBy)
            .NotEmpty()
            .WithMessage("El usuario que elimina es requerido.");
    }
}

#endregion

#region Dispute Validators

public class FileDisputeValidator : AbstractValidator<FileDisputeCommand>
{
    public FileDisputeValidator()
    {
        RuleFor(x => x.EscrowAccountId)
            .NotEmpty()
            .WithMessage("El ID de la cuenta escrow es requerido.");

        RuleFor(x => x.FiledById)
            .NotEmpty()
            .WithMessage("El ID del usuario que presenta la disputa es requerido.");

        RuleFor(x => x.FiledByName)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("El nombre del usuario es requerido (máximo 200 caracteres).");

        RuleFor(x => x.FiledByType)
            .Must(x => x == "Buyer" || x == "Seller")
            .WithMessage("El tipo debe ser 'Buyer' o 'Seller'.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("El motivo es requerido (máximo 100 caracteres).");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("La descripción es requerida (máximo 2000 caracteres).");
    }
}

public class AssignDisputeValidator : AbstractValidator<AssignDisputeCommand>
{
    public AssignDisputeValidator()
    {
        RuleFor(x => x.DisputeId)
            .NotEmpty()
            .WithMessage("El ID de la disputa es requerido.");

        RuleFor(x => x.AssignedTo)
            .NotEmpty()
            .WithMessage("El usuario asignado es requerido.");
    }
}

public class EscalateDisputeValidator : AbstractValidator<EscalateDisputeCommand>
{
    public EscalateDisputeValidator()
    {
        RuleFor(x => x.DisputeId)
            .NotEmpty()
            .WithMessage("El ID de la disputa es requerido.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("El motivo de escalación es requerido (máximo 500 caracteres).");

        RuleFor(x => x.EscalatedBy)
            .NotEmpty()
            .WithMessage("El usuario que escala es requerido.");
    }
}

public class ResolveDisputeValidator : AbstractValidator<ResolveDisputeCommand>
{
    public ResolveDisputeValidator()
    {
        RuleFor(x => x.DisputeId)
            .NotEmpty()
            .WithMessage("El ID de la disputa es requerido.");

        RuleFor(x => x.Resolution)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("La resolución es requerida (máximo 2000 caracteres).");

        RuleFor(x => x.ResolvedBuyerAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El monto para el comprador no puede ser negativo.");

        RuleFor(x => x.ResolvedSellerAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El monto para el vendedor no puede ser negativo.");

        RuleFor(x => x.ResolvedBy)
            .NotEmpty()
            .WithMessage("El usuario que resuelve es requerido.");
    }
}

public class CloseDisputeValidator : AbstractValidator<CloseDisputeCommand>
{
    public CloseDisputeValidator()
    {
        RuleFor(x => x.DisputeId)
            .NotEmpty()
            .WithMessage("El ID de la disputa es requerido.");

        RuleFor(x => x.ClosedBy)
            .NotEmpty()
            .WithMessage("El usuario que cierra es requerido.");
    }
}

#endregion

#region Fee Configuration Validators

public class CreateFeeConfigurationValidator : AbstractValidator<CreateFeeConfigurationCommand>
{
    public CreateFeeConfigurationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("El nombre es requerido (máximo 100 caracteres).");

        RuleFor(x => x.TransactionType)
            .IsInEnum()
            .WithMessage("Tipo de transacción inválido.");

        RuleFor(x => x.MinAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El monto mínimo no puede ser negativo.");

        RuleFor(x => x.MaxAmount)
            .GreaterThan(x => x.MinAmount)
            .WithMessage("El monto máximo debe ser mayor que el mínimo.");

        RuleFor(x => x.FeePercentage)
            .InclusiveBetween(0, 100)
            .WithMessage("El porcentaje de comisión debe estar entre 0 y 100.");

        RuleFor(x => x.MinFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La comisión mínima no puede ser negativa.");

        RuleFor(x => x.MaxFee)
            .GreaterThan(x => x.MinFee)
            .WithMessage("La comisión máxima debe ser mayor que la mínima.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty()
            .WithMessage("La fecha de inicio es requerida.");

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("El usuario que crea es requerido.");
    }
}

public class UpdateFeeConfigurationValidator : AbstractValidator<UpdateFeeConfigurationCommand>
{
    public UpdateFeeConfigurationValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID de la configuración es requerido.");

        RuleFor(x => x.FeePercentage)
            .InclusiveBetween(0, 100)
            .WithMessage("El porcentaje de comisión debe estar entre 0 y 100.");

        RuleFor(x => x.MinFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La comisión mínima no puede ser negativa.");

        RuleFor(x => x.MaxFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La comisión máxima no puede ser negativa.");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty()
            .WithMessage("El usuario que actualiza es requerido.");
    }
}

#endregion

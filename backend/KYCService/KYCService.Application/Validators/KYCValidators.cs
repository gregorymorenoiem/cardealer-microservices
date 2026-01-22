using FluentValidation;
using KYCService.Application.Commands;

namespace KYCService.Application.Validators;

/// <summary>
/// Validador para crear perfil KYC
/// </summary>
public class CreateKYCProfileValidator : AbstractValidator<CreateKYCProfileCommand>
{
    public CreateKYCProfileValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required")
            .MaximumLength(200)
            .WithMessage("Full name cannot exceed 200 characters");

        RuleFor(x => x.PrimaryDocumentNumber)
            .NotEmpty()
            .When(x => x.EntityType == Domain.Entities.EntityType.Individual)
            .WithMessage("Document number is required for individuals");

        RuleFor(x => x.RNC)
            .NotEmpty()
            .When(x => x.EntityType == Domain.Entities.EntityType.Business)
            .WithMessage("RNC is required for businesses")
            .Matches(@"^\d{9,11}$")
            .When(x => !string.IsNullOrEmpty(x.RNC))
            .WithMessage("RNC must be 9-11 digits");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");

        RuleFor(x => x.Phone)
            .Matches(@"^[\d\s\+\-\(\)]+$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Invalid phone format");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Now.AddYears(-18))
            .When(x => x.DateOfBirth.HasValue && x.EntityType == Domain.Entities.EntityType.Individual)
            .WithMessage("Person must be at least 18 years old");

        RuleFor(x => x.BusinessName)
            .NotEmpty()
            .When(x => x.EntityType == Domain.Entities.EntityType.Business)
            .WithMessage("Business name is required for businesses");

        RuleFor(x => x.PEPPosition)
            .NotEmpty()
            .When(x => x.IsPEP)
            .WithMessage("PEP position is required when IsPEP is true");
    }
}

/// <summary>
/// Validador para actualizar perfil KYC
/// </summary>
public class UpdateKYCProfileValidator : AbstractValidator<UpdateKYCProfileCommand>
{
    public UpdateKYCProfileValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Profile ID is required");

        RuleFor(x => x.FullName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.FullName))
            .WithMessage("Full name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");
    }
}

/// <summary>
/// Validador para aprobar perfil KYC
/// </summary>
public class ApproveKYCProfileValidator : AbstractValidator<ApproveKYCProfileCommand>
{
    public ApproveKYCProfileValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Profile ID is required");

        RuleFor(x => x.ApprovedBy)
            .NotEmpty()
            .WithMessage("ApprovedBy is required");

        RuleFor(x => x.ValidityDays)
            .InclusiveBetween(30, 730)
            .WithMessage("Validity days must be between 30 and 730 (2 years)");
    }
}

/// <summary>
/// Validador para rechazar perfil KYC
/// </summary>
public class RejectKYCProfileValidator : AbstractValidator<RejectKYCProfileCommand>
{
    public RejectKYCProfileValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Profile ID is required");

        RuleFor(x => x.RejectedBy)
            .NotEmpty()
            .WithMessage("RejectedBy is required");

        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .WithMessage("Rejection reason is required")
            .MaximumLength(1000)
            .WithMessage("Rejection reason cannot exceed 1000 characters");
    }
}

/// <summary>
/// Validador para subir documento KYC
/// </summary>
public class UploadKYCDocumentValidator : AbstractValidator<UploadKYCDocumentCommand>
{
    public UploadKYCDocumentValidator()
    {
        RuleFor(x => x.KYCProfileId)
            .NotEmpty()
            .WithMessage("KYC Profile ID is required");

        RuleFor(x => x.DocumentName)
            .NotEmpty()
            .WithMessage("Document name is required")
            .MaximumLength(200)
            .WithMessage("Document name cannot exceed 200 characters");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required");

        RuleFor(x => x.FileUrl)
            .NotEmpty()
            .WithMessage("File URL is required");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("File size must be greater than 0")
            .LessThan(10 * 1024 * 1024) // 10 MB
            .WithMessage("File size cannot exceed 10 MB");

        RuleFor(x => x.UploadedBy)
            .NotEmpty()
            .WithMessage("UploadedBy is required");
    }
}

/// <summary>
/// Validador para verificar documento KYC
/// </summary>
public class VerifyKYCDocumentValidator : AbstractValidator<VerifyKYCDocumentCommand>
{
    public VerifyKYCDocumentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Document ID is required");

        RuleFor(x => x.VerifiedBy)
            .NotEmpty()
            .WithMessage("VerifiedBy is required");

        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .When(x => !x.Approved)
            .WithMessage("Rejection reason is required when document is rejected");
    }
}

/// <summary>
/// Validador para crear verificación KYC
/// </summary>
public class CreateKYCVerificationValidator : AbstractValidator<CreateKYCVerificationCommand>
{
    public CreateKYCVerificationValidator()
    {
        RuleFor(x => x.KYCProfileId)
            .NotEmpty()
            .WithMessage("KYC Profile ID is required");

        RuleFor(x => x.VerificationType)
            .NotEmpty()
            .WithMessage("Verification type is required")
            .Must(x => new[] { "identity", "address", "income", "pep", "sanctions" }.Contains(x.ToLower()))
            .WithMessage("Invalid verification type");

        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("Provider is required");

        RuleFor(x => x.ConfidenceScore)
            .InclusiveBetween(0, 100)
            .WithMessage("Confidence score must be between 0 and 100");

        RuleFor(x => x.FailureReason)
            .NotEmpty()
            .When(x => !x.Passed)
            .WithMessage("Failure reason is required when verification failed");
    }
}

/// <summary>
/// Validador para evaluar riesgo KYC
/// </summary>
public class AssessKYCRiskValidator : AbstractValidator<AssessKYCRiskCommand>
{
    public AssessKYCRiskValidator()
    {
        RuleFor(x => x.KYCProfileId)
            .NotEmpty()
            .WithMessage("KYC Profile ID is required");

        RuleFor(x => x.NewScore)
            .InclusiveBetween(0, 100)
            .WithMessage("Risk score must be between 0 and 100");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required")
            .MaximumLength(1000)
            .WithMessage("Reason cannot exceed 1000 characters");

        RuleFor(x => x.AssessedBy)
            .NotEmpty()
            .WithMessage("AssessedBy is required");

        RuleFor(x => x.Factors)
            .NotEmpty()
            .WithMessage("At least one risk factor is required");
    }
}

/// <summary>
/// Validador para crear reporte de transacción sospechosa
/// </summary>
public class CreateSTRValidator : AbstractValidator<CreateSuspiciousTransactionReportCommand>
{
    public CreateSTRValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.SuspiciousActivityType)
            .NotEmpty()
            .WithMessage("Suspicious activity type is required")
            .MaximumLength(200)
            .WithMessage("Suspicious activity type cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MinimumLength(50)
            .WithMessage("Description must be at least 50 characters")
            .MaximumLength(5000)
            .WithMessage("Description cannot exceed 5000 characters");

        RuleFor(x => x.RedFlags)
            .NotEmpty()
            .WithMessage("At least one red flag is required");

        RuleFor(x => x.DetectedAt)
            .NotEmpty()
            .WithMessage("Detection date is required")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Detection date cannot be in the future");

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("CreatedBy is required");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Must(x => new[] { "DOP", "USD", "EUR" }.Contains(x))
            .WithMessage("Currency must be DOP, USD, or EUR");
    }
}

/// <summary>
/// Validador para aprobar STR
/// </summary>
public class ApproveSTRValidator : AbstractValidator<ApproveSTRCommand>
{
    public ApproveSTRValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Report ID is required");

        RuleFor(x => x.ApprovedBy)
            .NotEmpty()
            .WithMessage("ApprovedBy is required");
    }
}

/// <summary>
/// Validador para enviar STR a UAF
/// </summary>
public class SendSTRToUAFValidator : AbstractValidator<SendSTRToUAFCommand>
{
    public SendSTRToUAFValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Report ID is required");

        RuleFor(x => x.SentBy)
            .NotEmpty()
            .WithMessage("SentBy is required");

        RuleFor(x => x.UAFReportNumber)
            .NotEmpty()
            .WithMessage("UAF report number is required")
            .MaximumLength(50)
            .WithMessage("UAF report number cannot exceed 50 characters");
    }
}

/// <summary>
/// Validador para agregar entrada a watchlist
/// </summary>
public class AddWatchlistEntryValidator : AbstractValidator<AddWatchlistEntryCommand>
{
    public AddWatchlistEntryValidator()
    {
        RuleFor(x => x.Source)
            .NotEmpty()
            .WithMessage("Source is required")
            .MaximumLength(100)
            .WithMessage("Source cannot exceed 100 characters");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required")
            .MaximumLength(300)
            .WithMessage("Full name cannot exceed 300 characters");
    }
}

/// <summary>
/// Validador para screening de watchlist
/// </summary>
public class ScreenWatchlistValidator : AbstractValidator<ScreenWatchlistCommand>
{
    public ScreenWatchlistValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required for screening")
            .MinimumLength(2)
            .WithMessage("Full name must be at least 2 characters");
    }
}

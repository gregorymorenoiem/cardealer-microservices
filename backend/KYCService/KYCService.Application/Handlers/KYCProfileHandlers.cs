using MediatR;
using KYCService.Domain.Entities;
using KYCService.Domain.Interfaces;
using KYCService.Application.Commands;
using KYCService.Application.DTOs;

namespace KYCService.Application.Handlers;

/// <summary>
/// Handler para crear perfil KYC
/// </summary>
public class CreateKYCProfileHandler : IRequestHandler<CreateKYCProfileCommand, KYCProfileDto>
{
    private readonly IKYCProfileRepository _repository;

    public CreateKYCProfileHandler(IKYCProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<KYCProfileDto> Handle(CreateKYCProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = new KYCProfile
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            EntityType = request.EntityType,
            Status = KYCStatus.Pending,
            RiskLevel = RiskLevel.Low,
            RiskScore = 0,
            FullName = request.FullName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            PlaceOfBirth = request.PlaceOfBirth,
            Nationality = request.Nationality,
            Gender = request.Gender,
            PrimaryDocumentType = request.PrimaryDocumentType,
            PrimaryDocumentNumber = request.PrimaryDocumentNumber,
            PrimaryDocumentExpiry = request.PrimaryDocumentExpiry,
            PrimaryDocumentCountry = request.PrimaryDocumentCountry,
            Email = request.Email,
            Phone = request.Phone,
            MobilePhone = request.MobilePhone,
            Address = request.Address,
            City = request.City,
            Province = request.Province,
            PostalCode = request.PostalCode,
            Country = request.Country ?? "DO",
            Occupation = request.Occupation,
            EmployerName = request.EmployerName,
            SourceOfFunds = request.SourceOfFunds,
            ExpectedTransactionVolume = request.ExpectedTransactionVolume,
            EstimatedAnnualIncome = request.EstimatedAnnualIncome,
            IsPEP = request.IsPEP,
            PEPPosition = request.PEPPosition,
            PEPRelationship = request.PEPRelationship,
            BusinessName = request.BusinessName,
            RNC = request.RNC,
            BusinessType = request.BusinessType,
            IncorporationDate = request.IncorporationDate,
            LegalRepresentative = request.LegalRepresentative,
            CreatedAt = DateTime.UtcNow
        };

        // Si es PEP, incrementar nivel de riesgo inicial
        if (profile.IsPEP)
        {
            profile.RiskLevel = RiskLevel.High;
            profile.RiskScore = 70;
            profile.RiskFactors.Add("PEP Status");
        }

        var created = await _repository.CreateAsync(profile, cancellationToken);
        return MapToDto(created);
    }

    private static KYCProfileDto MapToDto(KYCProfile p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        EntityType = p.EntityType,
        Status = p.Status,
        RiskLevel = p.RiskLevel,
        RiskScore = p.RiskScore,
        RiskFactors = p.RiskFactors,
        FullName = p.FullName,
        MiddleName = p.MiddleName,
        LastName = p.LastName,
        DateOfBirth = p.DateOfBirth,
        PlaceOfBirth = p.PlaceOfBirth,
        Nationality = p.Nationality,
        PrimaryDocumentType = p.PrimaryDocumentType,
        PrimaryDocumentNumber = p.PrimaryDocumentNumber,
        PrimaryDocumentExpiry = p.PrimaryDocumentExpiry,
        Email = p.Email,
        Phone = p.Phone,
        Address = p.Address,
        City = p.City,
        Province = p.Province,
        Country = p.Country,
        IsPEP = p.IsPEP,
        PEPPosition = p.PEPPosition,
        BusinessName = p.BusinessName,
        RNC = p.RNC,
        IdentityVerifiedAt = p.IdentityVerifiedAt,
        AddressVerifiedAt = p.AddressVerifiedAt,
        CreatedAt = p.CreatedAt,
        ApprovedAt = p.ApprovedAt,
        ExpiresAt = p.ExpiresAt,
        NextReviewAt = p.NextReviewAt
    };
}

/// <summary>
/// Handler para actualizar perfil KYC
/// </summary>
public class UpdateKYCProfileHandler : IRequestHandler<UpdateKYCProfileCommand, KYCProfileDto>
{
    private readonly IKYCProfileRepository _repository;

    public UpdateKYCProfileHandler(IKYCProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<KYCProfileDto> Handle(UpdateKYCProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"KYC Profile with ID {request.Id} not found");

        if (request.FullName != null) profile.FullName = request.FullName;
        if (request.Email != null) profile.Email = request.Email;
        if (request.Phone != null) profile.Phone = request.Phone;
        if (request.MobilePhone != null) profile.MobilePhone = request.MobilePhone;
        if (request.Address != null) profile.Address = request.Address;
        if (request.City != null) profile.City = request.City;
        if (request.Province != null) profile.Province = request.Province;
        if (request.Occupation != null) profile.Occupation = request.Occupation;
        if (request.EmployerName != null) profile.EmployerName = request.EmployerName;
        if (request.SourceOfFunds != null) profile.SourceOfFunds = request.SourceOfFunds;
        if (request.EstimatedAnnualIncome.HasValue) profile.EstimatedAnnualIncome = request.EstimatedAnnualIncome;

        profile.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(profile, cancellationToken);
        return MapToDto(updated);
    }

    private static KYCProfileDto MapToDto(KYCProfile p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        EntityType = p.EntityType,
        Status = p.Status,
        RiskLevel = p.RiskLevel,
        RiskScore = p.RiskScore,
        RiskFactors = p.RiskFactors,
        FullName = p.FullName,
        Email = p.Email,
        Phone = p.Phone,
        Address = p.Address,
        City = p.City,
        Province = p.Province,
        Country = p.Country,
        IsPEP = p.IsPEP,
        CreatedAt = p.CreatedAt,
        ApprovedAt = p.ApprovedAt,
        ExpiresAt = p.ExpiresAt,
        NextReviewAt = p.NextReviewAt
    };
}

/// <summary>
/// Handler para aprobar perfil KYC
/// </summary>
public class ApproveKYCProfileHandler : IRequestHandler<ApproveKYCProfileCommand, KYCProfileDto>
{
    private readonly IKYCProfileRepository _repository;

    public ApproveKYCProfileHandler(IKYCProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<KYCProfileDto> Handle(ApproveKYCProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"KYC Profile with ID {request.Id} not found");

        profile.Status = KYCStatus.Approved;
        profile.ApprovedAt = DateTime.UtcNow;
        profile.ApprovedBy = request.ApprovedBy;
        profile.ApprovalNotes = request.Notes;
        profile.UpdatedAt = DateTime.UtcNow;
        profile.ExpiresAt = DateTime.UtcNow.AddDays(request.ValidityDays);
        profile.NextReviewAt = DateTime.UtcNow.AddDays(request.ValidityDays - 30); // Revisar 30 días antes de expirar
        profile.LastReviewAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(profile, cancellationToken);
        return MapToDto(updated);
    }

    private static KYCProfileDto MapToDto(KYCProfile p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        EntityType = p.EntityType,
        Status = p.Status,
        RiskLevel = p.RiskLevel,
        RiskScore = p.RiskScore,
        FullName = p.FullName,
        IsPEP = p.IsPEP,
        CreatedAt = p.CreatedAt,
        ApprovedAt = p.ApprovedAt,
        ExpiresAt = p.ExpiresAt,
        NextReviewAt = p.NextReviewAt
    };
}

/// <summary>
/// Handler para rechazar perfil KYC
/// </summary>
public class RejectKYCProfileHandler : IRequestHandler<RejectKYCProfileCommand, KYCProfileDto>
{
    private readonly IKYCProfileRepository _repository;

    public RejectKYCProfileHandler(IKYCProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<KYCProfileDto> Handle(RejectKYCProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"KYC Profile with ID {request.Id} not found");

        profile.Status = KYCStatus.Rejected;
        profile.RejectedAt = DateTime.UtcNow;
        profile.RejectedBy = request.RejectedBy;
        profile.RejectionReason = request.RejectionReason;
        profile.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(profile, cancellationToken);
        return MapToDto(updated);
    }

    private static KYCProfileDto MapToDto(KYCProfile p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        EntityType = p.EntityType,
        Status = p.Status,
        RiskLevel = p.RiskLevel,
        FullName = p.FullName,
        IsPEP = p.IsPEP,
        CreatedAt = p.CreatedAt
    };
}

/// <summary>
/// Handler para subir documento KYC
/// </summary>
public class UploadKYCDocumentHandler : IRequestHandler<UploadKYCDocumentCommand, KYCDocumentDto>
{
    private readonly IKYCDocumentRepository _repository;
    private readonly IKYCProfileRepository _profileRepository;

    public UploadKYCDocumentHandler(IKYCDocumentRepository repository, IKYCProfileRepository profileRepository)
    {
        _repository = repository;
        _profileRepository = profileRepository;
    }

    public async Task<KYCDocumentDto> Handle(UploadKYCDocumentCommand request, CancellationToken cancellationToken)
    {
        // Verificar que el perfil existe
        var profile = await _profileRepository.GetByIdAsync(request.KYCProfileId, cancellationToken)
            ?? throw new InvalidOperationException($"KYC Profile with ID {request.KYCProfileId} not found");

        var document = new KYCDocument
        {
            Id = Guid.NewGuid(),
            KYCProfileId = request.KYCProfileId,
            Type = request.Type,
            DocumentName = request.DocumentName,
            FileName = request.FileName,
            FileUrl = request.FileUrl,
            FileType = request.FileType,
            FileSize = request.FileSize,
            FileHash = request.FileHash,
            Status = KYCDocumentStatus.Pending,
            UploadedBy = request.UploadedBy,
            UploadedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(document, cancellationToken);

        // Actualizar estado del perfil si está pendiente
        if (profile.Status == KYCStatus.Pending)
        {
            profile.Status = KYCStatus.InProgress;
            profile.UpdatedAt = DateTime.UtcNow;
            await _profileRepository.UpdateAsync(profile, cancellationToken);
        }

        return new KYCDocumentDto
        {
            Id = created.Id,
            KYCProfileId = created.KYCProfileId,
            Type = created.Type,
            DocumentName = created.DocumentName,
            FileName = created.FileName,
            FileUrl = created.FileUrl,
            FileType = created.FileType,
            FileSize = created.FileSize,
            Status = created.Status,
            UploadedAt = created.UploadedAt
        };
    }
}

/// <summary>
/// Handler para verificar documento KYC
/// </summary>
public class VerifyKYCDocumentHandler : IRequestHandler<VerifyKYCDocumentCommand, KYCDocumentDto>
{
    private readonly IKYCDocumentRepository _repository;

    public VerifyKYCDocumentHandler(IKYCDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<KYCDocumentDto> Handle(VerifyKYCDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"KYC Document with ID {request.Id} not found");

        document.Status = request.Approved ? KYCDocumentStatus.Verified : KYCDocumentStatus.Rejected;
        document.VerifiedAt = DateTime.UtcNow;
        document.VerifiedBy = request.VerifiedBy;
        document.RejectionReason = request.RejectionReason;
        document.ExtractedNumber = request.ExtractedNumber;
        document.ExtractedExpiry = request.ExtractedExpiry;
        document.ExtractedName = request.ExtractedName;

        var updated = await _repository.UpdateAsync(document, cancellationToken);

        return new KYCDocumentDto
        {
            Id = updated.Id,
            KYCProfileId = updated.KYCProfileId,
            Type = updated.Type,
            DocumentName = updated.DocumentName,
            FileName = updated.FileName,
            FileUrl = updated.FileUrl,
            FileType = updated.FileType,
            FileSize = updated.FileSize,
            Status = updated.Status,
            RejectionReason = updated.RejectionReason,
            UploadedAt = updated.UploadedAt,
            VerifiedAt = updated.VerifiedAt
        };
    }
}

/// <summary>
/// Handler para crear verificación KYC
/// </summary>
public class CreateKYCVerificationHandler : IRequestHandler<CreateKYCVerificationCommand, KYCVerificationDto>
{
    private readonly IKYCVerificationRepository _repository;
    private readonly IKYCProfileRepository _profileRepository;

    public CreateKYCVerificationHandler(IKYCVerificationRepository repository, IKYCProfileRepository profileRepository)
    {
        _repository = repository;
        _profileRepository = profileRepository;
    }

    public async Task<KYCVerificationDto> Handle(CreateKYCVerificationCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByIdAsync(request.KYCProfileId, cancellationToken)
            ?? throw new InvalidOperationException($"KYC Profile with ID {request.KYCProfileId} not found");

        var verification = new KYCVerification
        {
            Id = Guid.NewGuid(),
            KYCProfileId = request.KYCProfileId,
            VerificationType = request.VerificationType,
            Provider = request.Provider,
            Passed = request.Passed,
            FailureReason = request.FailureReason,
            RawResponse = request.RawResponse,
            ConfidenceScore = request.ConfidenceScore,
            VerifiedAt = DateTime.UtcNow,
            VerifiedBy = request.VerifiedBy,
            ExpiresAt = request.ValidityDays.HasValue ? DateTime.UtcNow.AddDays(request.ValidityDays.Value) : null
        };

        var created = await _repository.CreateAsync(verification, cancellationToken);

        // Actualizar timestamps de verificación en el perfil
        if (request.Passed)
        {
            switch (request.VerificationType.ToLower())
            {
                case "identity":
                    profile.IdentityVerifiedAt = DateTime.UtcNow;
                    break;
                case "address":
                    profile.AddressVerifiedAt = DateTime.UtcNow;
                    break;
                case "income":
                    profile.IncomeVerifiedAt = DateTime.UtcNow;
                    break;
                case "pep":
                    profile.PEPCheckedAt = DateTime.UtcNow;
                    break;
                case "sanctions":
                    profile.SanctionsCheckedAt = DateTime.UtcNow;
                    break;
            }
            profile.UpdatedAt = DateTime.UtcNow;
            await _profileRepository.UpdateAsync(profile, cancellationToken);
        }

        return new KYCVerificationDto
        {
            Id = created.Id,
            KYCProfileId = created.KYCProfileId,
            VerificationType = created.VerificationType,
            Provider = created.Provider,
            Passed = created.Passed,
            FailureReason = created.FailureReason,
            ConfidenceScore = created.ConfidenceScore,
            VerifiedAt = created.VerifiedAt,
            ExpiresAt = created.ExpiresAt
        };
    }
}

/// <summary>
/// Handler para evaluar riesgo KYC
/// </summary>
public class AssessKYCRiskHandler : IRequestHandler<AssessKYCRiskCommand, KYCRiskAssessmentDto>
{
    private readonly IKYCRiskAssessmentRepository _repository;
    private readonly IKYCProfileRepository _profileRepository;

    public AssessKYCRiskHandler(IKYCRiskAssessmentRepository repository, IKYCProfileRepository profileRepository)
    {
        _repository = repository;
        _profileRepository = profileRepository;
    }

    public async Task<KYCRiskAssessmentDto> Handle(AssessKYCRiskCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByIdAsync(request.KYCProfileId, cancellationToken)
            ?? throw new InvalidOperationException($"KYC Profile with ID {request.KYCProfileId} not found");

        var assessment = new KYCRiskAssessment
        {
            Id = Guid.NewGuid(),
            KYCProfileId = request.KYCProfileId,
            PreviousLevel = profile.RiskLevel,
            NewLevel = request.NewLevel,
            PreviousScore = profile.RiskScore,
            NewScore = request.NewScore,
            Reason = request.Reason,
            Factors = request.Factors,
            RecommendedActions = request.RecommendedActions,
            AssessedBy = request.AssessedBy,
            AssessedByName = request.AssessedByName,
            AssessedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(assessment, cancellationToken);

        // Actualizar nivel de riesgo en el perfil
        profile.RiskLevel = request.NewLevel;
        profile.RiskScore = request.NewScore;
        profile.RiskFactors = request.Factors;
        profile.UpdatedAt = DateTime.UtcNow;
        await _profileRepository.UpdateAsync(profile, cancellationToken);

        return new KYCRiskAssessmentDto
        {
            Id = created.Id,
            KYCProfileId = created.KYCProfileId,
            PreviousLevel = created.PreviousLevel,
            NewLevel = created.NewLevel,
            PreviousScore = created.PreviousScore,
            NewScore = created.NewScore,
            Reason = created.Reason,
            Factors = created.Factors,
            RecommendedActions = created.RecommendedActions,
            AssessedByName = created.AssessedByName,
            AssessedAt = created.AssessedAt
        };
    }
}

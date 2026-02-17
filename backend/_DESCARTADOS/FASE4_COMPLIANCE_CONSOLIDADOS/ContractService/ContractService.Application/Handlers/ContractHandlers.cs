// ContractService - Command Handlers

namespace ContractService.Application.Handlers;

using MediatR;
using ContractService.Domain.Entities;
using ContractService.Domain.Interfaces;
using ContractService.Application.Commands;
using ContractService.Application.DTOs;
using System.Security.Cryptography;
using System.Text;

#region Template Handlers

public class CreateTemplateHandler : IRequestHandler<CreateTemplateCommand, Guid>
{
    private readonly IContractTemplateRepository _repository;

    public CreateTemplateHandler(IContractTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateTemplateCommand request, CancellationToken ct)
    {
        if (await _repository.ExistsByCodeAsync(request.Code, ct))
            throw new InvalidOperationException($"Ya existe una plantilla con código {request.Code}");

        var template = new ContractTemplate
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            ContentHtml = request.ContentHtml,
            ContentJson = request.ContentJson,
            RequiredVariables = request.RequiredVariables ?? new List<string>(),
            OptionalVariables = request.OptionalVariables ?? new List<string>(),
            Language = request.Language,
            LegalBasis = request.LegalBasis,
            RequiresWitness = request.RequiresWitness,
            MinimumSignatures = request.MinimumSignatures,
            RequiresNotarization = request.RequiresNotarization,
            ValidityDays = request.ValidityDays,
            IsActive = true,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _repository.AddAsync(template, ct);
        return template.Id;
    }
}

public class UpdateTemplateHandler : IRequestHandler<UpdateTemplateCommand, bool>
{
    private readonly IContractTemplateRepository _repository;

    public UpdateTemplateHandler(IContractTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateTemplateCommand request, CancellationToken ct)
    {
        var template = await _repository.GetByIdAsync(request.Id, ct);
        if (template == null) return false;

        template.Name = request.Name;
        template.Description = request.Description;
        template.ContentHtml = request.ContentHtml;
        template.ContentJson = request.ContentJson;
        template.RequiredVariables = request.RequiredVariables ?? template.RequiredVariables;
        template.OptionalVariables = request.OptionalVariables ?? template.OptionalVariables;
        template.LegalBasis = request.LegalBasis;
        template.RequiresWitness = request.RequiresWitness;
        template.MinimumSignatures = request.MinimumSignatures;
        template.RequiresNotarization = request.RequiresNotarization;
        template.ValidityDays = request.ValidityDays;
        template.IsActive = request.IsActive;
        template.Version++;
        template.UpdatedAt = DateTime.UtcNow;
        template.UpdatedBy = request.UpdatedBy;

        await _repository.UpdateAsync(template, ct);
        return true;
    }
}

#endregion

#region Contract Handlers

public class CreateContractHandler : IRequestHandler<CreateContractCommand, CreateContractResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IContractTemplateRepository _templateRepository;
    private readonly IContractPartyRepository _partyRepository;
    private readonly IContractClauseRepository _clauseRepository;
    private readonly IContractAuditLogRepository _auditRepository;

    public CreateContractHandler(
        IContractRepository contractRepository,
        IContractTemplateRepository templateRepository,
        IContractPartyRepository partyRepository,
        IContractClauseRepository clauseRepository,
        IContractAuditLogRepository auditRepository)
    {
        _contractRepository = contractRepository;
        _templateRepository = templateRepository;
        _partyRepository = partyRepository;
        _clauseRepository = clauseRepository;
        _auditRepository = auditRepository;
    }

    public async Task<CreateContractResponse> Handle(CreateContractCommand request, CancellationToken ct)
    {
        var contractNumber = await _contractRepository.GenerateContractNumberAsync(ct);
        
        string contentHtml = string.Empty;
        ContractTemplate? template = null;
        
        if (request.TemplateId.HasValue)
        {
            template = await _templateRepository.GetByIdAsync(request.TemplateId.Value, ct);
            if (template != null)
            {
                contentHtml = ApplyTemplateVariables(template.ContentHtml, request.TemplateVariables);
            }
        }

        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            ContractNumber = contractNumber,
            TemplateId = request.TemplateId,
            Type = request.Type,
            Title = request.Title,
            Description = request.Description,
            ContentHtml = contentHtml,
            ContentHash = ComputeHash(contentHtml),
            EffectiveDate = request.EffectiveDate,
            ExpirationDate = request.ExpirationDate ?? (template?.ValidityDays != null 
                ? request.EffectiveDate.AddDays(template.ValidityDays.Value) 
                : null),
            Status = ContractStatus.Draft,
            SubjectType = request.SubjectType,
            SubjectId = request.SubjectId,
            SubjectDescription = request.SubjectDescription,
            ContractValue = request.ContractValue,
            Currency = request.Currency,
            LegalJurisdiction = request.LegalJurisdiction ?? "República Dominicana",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _contractRepository.AddAsync(contract, ct);

        // Add parties
        if (request.Parties != null)
        {
            foreach (var partyDto in request.Parties)
            {
                var party = new ContractParty
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    Type = partyDto.Type,
                    Role = partyDto.Role,
                    UserId = partyDto.UserId,
                    FullName = partyDto.FullName,
                    DocumentType = partyDto.DocumentType,
                    DocumentNumber = partyDto.DocumentNumber,
                    Email = partyDto.Email,
                    Phone = partyDto.Phone,
                    CompanyName = partyDto.CompanyName,
                    RNC = partyDto.RNC,
                    CreatedAt = DateTime.UtcNow
                };
                await _partyRepository.AddAsync(party, ct);
            }
        }

        // Copy clauses from template
        if (template != null)
        {
            var clauses = template.Clauses.Select(tc => new ContractClause
            {
                Id = Guid.NewGuid(),
                ContractId = contract.Id,
                TemplateClauseId = tc.Id,
                Code = tc.Code,
                Title = tc.Title,
                Content = tc.Content,
                OriginalContent = tc.Content,
                Type = tc.Type,
                Order = tc.Order
            }).ToList();

            await _clauseRepository.AddRangeAsync(clauses, ct);
        }

        // Audit log
        await _auditRepository.AddAsync(new ContractAuditLog
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            EventType = ContractAuditEventType.Created,
            Description = $"Contrato {contractNumber} creado",
            PerformedBy = request.CreatedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return new CreateContractResponse(contract.Id, contractNumber);
    }

    private string ApplyTemplateVariables(string template, Dictionary<string, string>? variables)
    {
        if (variables == null) return template;
        
        foreach (var kvp in variables)
        {
            template = template.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }
        return template;
    }

    private string ComputeHash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }
}

public class FinalizeContractHandler : IRequestHandler<FinalizeContractCommand, bool>
{
    private readonly IContractRepository _contractRepository;
    private readonly IContractSignatureRepository _signatureRepository;
    private readonly IContractPartyRepository _partyRepository;
    private readonly IContractAuditLogRepository _auditRepository;

    public FinalizeContractHandler(
        IContractRepository contractRepository,
        IContractSignatureRepository signatureRepository,
        IContractPartyRepository partyRepository,
        IContractAuditLogRepository auditRepository)
    {
        _contractRepository = contractRepository;
        _signatureRepository = signatureRepository;
        _partyRepository = partyRepository;
        _auditRepository = auditRepository;
    }

    public async Task<bool> Handle(FinalizeContractCommand request, CancellationToken ct)
    {
        var contract = await _contractRepository.GetByIdAsync(request.Id, ct);
        if (contract == null) return false;
        if (contract.Status != ContractStatus.Draft) return false;

        // Create signature requests for all parties
        var parties = await _partyRepository.GetByContractIdAsync(request.Id, ct);
        foreach (var party in parties)
        {
            var signature = new ContractSignature
            {
                Id = Guid.NewGuid(),
                ContractId = contract.Id,
                PartyId = party.Id,
                Type = SignatureType.Advanced,
                Status = SignatureStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _signatureRepository.AddAsync(signature, ct);
        }

        contract.Status = ContractStatus.PendingSignatures;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = request.FinalizedBy;
        await _contractRepository.UpdateAsync(contract, ct);

        await _auditRepository.AddAsync(new ContractAuditLog
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            EventType = ContractAuditEventType.StatusChanged,
            Description = "Contrato finalizado y listo para firmas",
            OldValue = ContractStatus.Draft.ToString(),
            NewValue = ContractStatus.PendingSignatures.ToString(),
            PerformedBy = request.FinalizedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return true;
    }
}

public class TerminateContractHandler : IRequestHandler<TerminateContractCommand, bool>
{
    private readonly IContractRepository _contractRepository;
    private readonly IContractAuditLogRepository _auditRepository;

    public TerminateContractHandler(
        IContractRepository contractRepository,
        IContractAuditLogRepository auditRepository)
    {
        _contractRepository = contractRepository;
        _auditRepository = auditRepository;
    }

    public async Task<bool> Handle(TerminateContractCommand request, CancellationToken ct)
    {
        var contract = await _contractRepository.GetByIdAsync(request.Id, ct);
        if (contract == null) return false;

        var oldStatus = contract.Status;
        contract.Status = ContractStatus.Terminated;
        contract.TerminatedAt = DateTime.UtcNow;
        contract.TerminationReason = request.Reason;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = request.TerminatedBy;

        await _contractRepository.UpdateAsync(contract, ct);

        await _auditRepository.AddAsync(new ContractAuditLog
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            EventType = ContractAuditEventType.Terminated,
            Description = $"Contrato terminado: {request.Reason}",
            OldValue = oldStatus.ToString(),
            NewValue = ContractStatus.Terminated.ToString(),
            PerformedBy = request.TerminatedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return true;
    }
}

#endregion

#region Signature Handlers

public class RequestSignatureHandler : IRequestHandler<RequestSignatureCommand, Guid>
{
    private readonly IContractSignatureRepository _signatureRepository;
    private readonly IContractPartyRepository _partyRepository;
    private readonly IContractAuditLogRepository _auditRepository;

    public RequestSignatureHandler(
        IContractSignatureRepository signatureRepository,
        IContractPartyRepository partyRepository,
        IContractAuditLogRepository auditRepository)
    {
        _signatureRepository = signatureRepository;
        _partyRepository = partyRepository;
        _auditRepository = auditRepository;
    }

    public async Task<Guid> Handle(RequestSignatureCommand request, CancellationToken ct)
    {
        var party = await _partyRepository.GetByIdAsync(request.PartyId, ct);
        if (party == null)
            throw new InvalidOperationException("Parte no encontrada");

        var signature = new ContractSignature
        {
            Id = Guid.NewGuid(),
            ContractId = request.ContractId,
            PartyId = request.PartyId,
            Type = request.Type,
            Status = SignatureStatus.Requested,
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _signatureRepository.AddAsync(signature, ct);

        await _auditRepository.AddAsync(new ContractAuditLog
        {
            Id = Guid.NewGuid(),
            ContractId = request.ContractId,
            EventType = ContractAuditEventType.SignatureRequested,
            Description = $"Solicitud de firma enviada a {party.FullName}",
            PerformedBy = request.RequestedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return signature.Id;
    }
}

public class SignContractHandler : IRequestHandler<SignContractCommand, SignContractResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IContractSignatureRepository _signatureRepository;
    private readonly IContractPartyRepository _partyRepository;
    private readonly IContractAuditLogRepository _auditRepository;

    public SignContractHandler(
        IContractRepository contractRepository,
        IContractSignatureRepository signatureRepository,
        IContractPartyRepository partyRepository,
        IContractAuditLogRepository auditRepository)
    {
        _contractRepository = contractRepository;
        _signatureRepository = signatureRepository;
        _partyRepository = partyRepository;
        _auditRepository = auditRepository;
    }

    public async Task<SignContractResponse> Handle(SignContractCommand request, CancellationToken ct)
    {
        var signature = await _signatureRepository.GetByIdAsync(request.SignatureId, ct);
        if (signature == null)
            return new SignContractResponse(false, request.SignatureId, "Firma no encontrada", false);

        if (signature.Status == SignatureStatus.Signed)
            return new SignContractResponse(false, request.SignatureId, "Ya firmado", false);

        if (signature.ExpiresAt.HasValue && signature.ExpiresAt < DateTime.UtcNow)
        {
            signature.Status = SignatureStatus.Expired;
            await _signatureRepository.UpdateAsync(signature, ct);
            return new SignContractResponse(false, request.SignatureId, "Solicitud de firma expirada", false);
        }

        var contract = await _contractRepository.GetByIdAsync(signature.ContractId, ct);
        if (contract == null)
            return new SignContractResponse(false, request.SignatureId, "Contrato no encontrado", false);

        // Update signature
        signature.Status = SignatureStatus.Signed;
        signature.SignatureData = request.SignatureData;
        signature.SignatureImage = request.SignatureImage;
        signature.CertificateData = request.CertificateData;
        signature.DocumentHash = contract.ContentHash;
        signature.SignedAt = DateTime.UtcNow;
        signature.TimestampDate = DateTime.UtcNow;
        signature.IPAddress = request.IPAddress;
        signature.UserAgent = request.UserAgent;
        signature.GeoLocation = request.GeoLocation;
        signature.DeviceFingerprint = request.DeviceFingerprint;
        signature.BiometricVerified = request.BiometricVerified;
        signature.BiometricType = request.BiometricType;
        signature.VerificationStatus = SignatureVerificationStatus.Verified;

        await _signatureRepository.UpdateAsync(signature, ct);

        // Update party
        var party = await _partyRepository.GetByIdAsync(signature.PartyId, ct);
        if (party != null)
        {
            party.HasSigned = true;
            party.SignedAt = DateTime.UtcNow;
            await _partyRepository.UpdateAsync(party, ct);
        }

        // Check if all parties signed
        var allSigned = await _signatureRepository.AllPartiesSignedAsync(contract.Id, ct);
        if (allSigned)
        {
            contract.Status = ContractStatus.FullySigned;
            contract.SignedAt = DateTime.UtcNow;
            contract.UpdatedAt = DateTime.UtcNow;
            await _contractRepository.UpdateAsync(contract, ct);
        }
        else
        {
            contract.Status = ContractStatus.PartiallySigned;
            contract.UpdatedAt = DateTime.UtcNow;
            await _contractRepository.UpdateAsync(contract, ct);
        }

        await _auditRepository.AddAsync(new ContractAuditLog
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            EventType = ContractAuditEventType.Signed,
            Description = $"Contrato firmado por {party?.FullName}",
            PerformedBy = party?.Email ?? "unknown",
            IPAddress = request.IPAddress,
            UserAgent = request.UserAgent,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return new SignContractResponse(true, signature.Id, "Firma registrada exitosamente", allSigned);
    }
}

public class DeclineSignatureHandler : IRequestHandler<DeclineSignatureCommand, bool>
{
    private readonly IContractSignatureRepository _signatureRepository;
    private readonly IContractAuditLogRepository _auditRepository;

    public DeclineSignatureHandler(
        IContractSignatureRepository signatureRepository,
        IContractAuditLogRepository auditRepository)
    {
        _signatureRepository = signatureRepository;
        _auditRepository = auditRepository;
    }

    public async Task<bool> Handle(DeclineSignatureCommand request, CancellationToken ct)
    {
        var signature = await _signatureRepository.GetByIdAsync(request.SignatureId, ct);
        if (signature == null) return false;

        signature.Status = SignatureStatus.Declined;
        signature.DeclinedAt = DateTime.UtcNow;
        signature.DeclineReason = request.Reason;

        await _signatureRepository.UpdateAsync(signature, ct);

        await _auditRepository.AddAsync(new ContractAuditLog
        {
            Id = Guid.NewGuid(),
            ContractId = signature.ContractId,
            EventType = ContractAuditEventType.SignatureDeclined,
            Description = $"Firma rechazada: {request.Reason}",
            PerformedBy = request.DeclinedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return true;
    }
}

#endregion

#region Document Handlers

public class UploadDocumentHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly IContractDocumentRepository _repository;
    private readonly IContractAuditLogRepository _auditRepository;

    public UploadDocumentHandler(
        IContractDocumentRepository repository,
        IContractAuditLogRepository auditRepository)
    {
        _repository = repository;
        _auditRepository = auditRepository;
    }

    public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken ct)
    {
        var document = new ContractDocument
        {
            Id = Guid.NewGuid(),
            ContractId = request.ContractId,
            Name = request.Name,
            Description = request.Description,
            DocumentType = request.DocumentType,
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            StoragePath = request.StoragePath,
            FileHash = request.FileHash,
            IsRequired = request.IsRequired,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = request.UploadedBy
        };

        await _repository.AddAsync(document, ct);

        await _auditRepository.AddAsync(new ContractAuditLog
        {
            Id = Guid.NewGuid(),
            ContractId = request.ContractId,
            EventType = ContractAuditEventType.Updated,
            Description = $"Documento adjunto: {request.Name}",
            PerformedBy = request.UploadedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return document.Id;
    }
}

#endregion

#region Terms Handler

public class AcceptTermsHandler : IRequestHandler<AcceptTermsCommand, bool>
{
    private readonly IContractRepository _contractRepository;
    private readonly IContractAuditLogRepository _auditRepository;

    public AcceptTermsHandler(
        IContractRepository contractRepository,
        IContractAuditLogRepository auditRepository)
    {
        _contractRepository = contractRepository;
        _auditRepository = auditRepository;
    }

    public async Task<bool> Handle(AcceptTermsCommand request, CancellationToken ct)
    {
        var contract = await _contractRepository.GetByIdAsync(request.ContractId, ct);
        if (contract == null) return false;

        contract.AcceptedTerms = request.AcceptedTerms;
        contract.AcceptedPrivacyPolicy = request.AcceptedPrivacyPolicy;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = request.AcceptedBy;

        await _contractRepository.UpdateAsync(contract, ct);

        await _auditRepository.AddAsync(new ContractAuditLog
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            EventType = ContractAuditEventType.Updated,
            Description = "Términos y condiciones aceptados",
            PerformedBy = request.AcceptedBy,
            IPAddress = request.IPAddress,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return true;
    }
}

#endregion

#region Signature Verification Handler

public class VerifySignatureHandler : IRequestHandler<VerifySignatureCommand, SignatureVerificationDto>
{
    private readonly IContractSignatureRepository _signatureRepository;
    private readonly ICertificationAuthorityRepository _caRepository;

    public VerifySignatureHandler(
        IContractSignatureRepository signatureRepository,
        ICertificationAuthorityRepository caRepository)
    {
        _signatureRepository = signatureRepository;
        _caRepository = caRepository;
    }

    public async Task<SignatureVerificationDto> Handle(VerifySignatureCommand request, CancellationToken ct)
    {
        var signature = await _signatureRepository.GetByIdAsync(request.SignatureId, ct);
        if (signature == null)
        {
            return new SignatureVerificationDto(
                request.SignatureId,
                false,
                SignatureVerificationStatus.Failed,
                "Firma no encontrada",
                null,
                null,
                null,
                null,
                false
            );
        }

        // Check if signature was signed
        if (signature.Status != SignatureStatus.Signed || signature.SignedAt == null)
        {
            return new SignatureVerificationDto(
                request.SignatureId,
                false,
                SignatureVerificationStatus.NotVerified,
                "La firma no ha sido completada",
                null,
                signature.CertificateIssuer,
                signature.CertificateValidFrom,
                signature.CertificateValidTo,
                false
            );
        }

        // Verify certificate validity
        var now = DateTime.UtcNow;
        var certIsValid = signature.CertificateValidFrom <= now && signature.CertificateValidTo >= now;

        // Update verification status
        signature.VerificationStatus = certIsValid 
            ? SignatureVerificationStatus.Verified 
            : SignatureVerificationStatus.Failed;
        
        await _signatureRepository.UpdateAsync(signature, ct);

        return new SignatureVerificationDto(
            signature.Id,
            certIsValid,
            signature.VerificationStatus,
            certIsValid ? "Firma verificada correctamente" : "El certificado ha expirado o no es válido",
            DateTime.UtcNow,
            signature.CertificateIssuer,
            signature.CertificateValidFrom,
            signature.CertificateValidTo,
            certIsValid
        );
    }
}

#endregion

// ContractService - Query Handlers

namespace ContractService.Application.Handlers;

using MediatR;
using ContractService.Domain.Entities;
using ContractService.Domain.Interfaces;
using ContractService.Application.Queries;
using ContractService.Application.DTOs;

#region Template Query Handlers

public class GetTemplateByIdHandler : IRequestHandler<GetTemplateByIdQuery, ContractTemplateDto?>
{
    private readonly IContractTemplateRepository _repository;

    public GetTemplateByIdHandler(IContractTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContractTemplateDto?> Handle(GetTemplateByIdQuery request, CancellationToken ct)
    {
        var template = await _repository.GetByIdAsync(request.Id, ct);
        return template == null ? null : MapToDto(template);
    }

    private static ContractTemplateDto MapToDto(ContractTemplate t) => new(
        t.Id, t.Code, t.Name, t.Description, t.Type, t.ContentHtml, t.ContentJson,
        t.RequiredVariables, t.OptionalVariables, t.Language, t.LegalBasis,
        t.RequiresWitness, t.MinimumSignatures, t.RequiresNotarization, t.ValidityDays,
        t.IsActive, t.Version, t.CreatedAt, t.CreatedBy, t.UpdatedAt
    );
}

public class GetTemplateByCodeHandler : IRequestHandler<GetTemplateByCodeQuery, ContractTemplateDto?>
{
    private readonly IContractTemplateRepository _repository;

    public GetTemplateByCodeHandler(IContractTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContractTemplateDto?> Handle(GetTemplateByCodeQuery request, CancellationToken ct)
    {
        var template = await _repository.GetByCodeAsync(request.Code, ct);
        return template == null ? null : new ContractTemplateDto(
            template.Id, template.Code, template.Name, template.Description, template.Type,
            template.ContentHtml, template.ContentJson, template.RequiredVariables,
            template.OptionalVariables, template.Language, template.LegalBasis,
            template.RequiresWitness, template.MinimumSignatures, template.RequiresNotarization,
            template.ValidityDays, template.IsActive, template.Version, template.CreatedAt,
            template.CreatedBy, template.UpdatedAt
        );
    }
}

public class GetActiveTemplatesHandler : IRequestHandler<GetActiveTemplatesQuery, List<ContractTemplateDto>>
{
    private readonly IContractTemplateRepository _repository;

    public GetActiveTemplatesHandler(IContractTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractTemplateDto>> Handle(GetActiveTemplatesQuery request, CancellationToken ct)
    {
        var templates = await _repository.GetActiveTemplatesAsync(ct);
        return templates.Select(t => new ContractTemplateDto(
            t.Id, t.Code, t.Name, t.Description, t.Type, t.ContentHtml, t.ContentJson,
            t.RequiredVariables, t.OptionalVariables, t.Language, t.LegalBasis,
            t.RequiresWitness, t.MinimumSignatures, t.RequiresNotarization, t.ValidityDays,
            t.IsActive, t.Version, t.CreatedAt, t.CreatedBy, t.UpdatedAt
        )).ToList();
    }
}

public class GetTemplatesByTypeHandler : IRequestHandler<GetTemplatesByTypeQuery, List<ContractTemplateDto>>
{
    private readonly IContractTemplateRepository _repository;

    public GetTemplatesByTypeHandler(IContractTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractTemplateDto>> Handle(GetTemplatesByTypeQuery request, CancellationToken ct)
    {
        var templates = await _repository.GetByTypeAsync(request.Type, ct);
        return templates.Select(t => new ContractTemplateDto(
            t.Id, t.Code, t.Name, t.Description, t.Type, t.ContentHtml, t.ContentJson,
            t.RequiredVariables, t.OptionalVariables, t.Language, t.LegalBasis,
            t.RequiresWitness, t.MinimumSignatures, t.RequiresNotarization, t.ValidityDays,
            t.IsActive, t.Version, t.CreatedAt, t.CreatedBy, t.UpdatedAt
        )).ToList();
    }
}

#endregion

#region Contract Query Handlers

public class GetContractByIdHandler : IRequestHandler<GetContractByIdQuery, ContractDto?>
{
    private readonly IContractRepository _repository;

    public GetContractByIdHandler(IContractRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContractDto?> Handle(GetContractByIdQuery request, CancellationToken ct)
    {
        var c = await _repository.GetByIdAsync(request.Id, ct);
        return c == null ? null : MapToDto(c);
    }

    private static ContractDto MapToDto(Contract c) => new(
        c.Id, c.ContractNumber, c.TemplateId, c.Type, c.Title, c.Description,
        c.ContentHtml, c.ContentHash, c.EffectiveDate, c.ExpirationDate, c.Status,
        c.SubjectType, c.SubjectId, c.SubjectDescription, c.ContractValue, c.Currency,
        c.LegalJurisdiction, c.SignedAt, c.TerminatedAt, c.TerminationReason,
        c.AcceptedTerms, c.AcceptedPrivacyPolicy, c.CreatedAt, c.CreatedBy, c.UpdatedAt
    );
}

public class GetContractByNumberHandler : IRequestHandler<GetContractByNumberQuery, ContractDto?>
{
    private readonly IContractRepository _repository;

    public GetContractByNumberHandler(IContractRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContractDto?> Handle(GetContractByNumberQuery request, CancellationToken ct)
    {
        var c = await _repository.GetByContractNumberAsync(request.ContractNumber, ct);
        return c == null ? null : new ContractDto(
            c.Id, c.ContractNumber, c.TemplateId, c.Type, c.Title, c.Description,
            c.ContentHtml, c.ContentHash, c.EffectiveDate, c.ExpirationDate, c.Status,
            c.SubjectType, c.SubjectId, c.SubjectDescription, c.ContractValue, c.Currency,
            c.LegalJurisdiction, c.SignedAt, c.TerminatedAt, c.TerminationReason,
            c.AcceptedTerms, c.AcceptedPrivacyPolicy, c.CreatedAt, c.CreatedBy, c.UpdatedAt
        );
    }
}

public class GetUserContractsHandler : IRequestHandler<GetUserContractsQuery, List<ContractSummaryDto>>
{
    private readonly IContractRepository _repository;

    public GetUserContractsHandler(IContractRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractSummaryDto>> Handle(GetUserContractsQuery request, CancellationToken ct)
    {
        var contracts = await _repository.GetByUserIdAsync(request.UserId, request.Page, request.PageSize, ct);
        return contracts.Select(c => new ContractSummaryDto(
            c.Id, c.ContractNumber, c.Type, c.Title, c.Status, c.EffectiveDate,
            c.ExpirationDate, c.ContractValue, c.Currency, c.CreatedAt
        )).ToList();
    }
}

public class GetSubjectContractsHandler : IRequestHandler<GetSubjectContractsQuery, List<ContractSummaryDto>>
{
    private readonly IContractRepository _repository;

    public GetSubjectContractsHandler(IContractRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractSummaryDto>> Handle(GetSubjectContractsQuery request, CancellationToken ct)
    {
        var contracts = await _repository.GetBySubjectAsync(request.SubjectType, request.SubjectId, ct);
        return contracts.Select(c => new ContractSummaryDto(
            c.Id, c.ContractNumber, c.Type, c.Title, c.Status, c.EffectiveDate,
            c.ExpirationDate, c.ContractValue, c.Currency, c.CreatedAt
        )).ToList();
    }
}

public class GetContractsByStatusHandler : IRequestHandler<GetContractsByStatusQuery, PagedResult<ContractSummaryDto>>
{
    private readonly IContractRepository _repository;

    public GetContractsByStatusHandler(IContractRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ContractSummaryDto>> Handle(GetContractsByStatusQuery request, CancellationToken ct)
    {
        var (contracts, total) = await _repository.GetByStatusAsync(request.Status, request.Page, request.PageSize, ct);
        var items = contracts.Select(c => new ContractSummaryDto(
            c.Id, c.ContractNumber, c.Type, c.Title, c.Status, c.EffectiveDate,
            c.ExpirationDate, c.ContractValue, c.Currency, c.CreatedAt
        )).ToList();
        return new PagedResult<ContractSummaryDto>(items, total, request.Page, request.PageSize);
    }
}

public class GetExpiringContractsHandler : IRequestHandler<GetExpiringContractsQuery, List<ContractSummaryDto>>
{
    private readonly IContractRepository _repository;

    public GetExpiringContractsHandler(IContractRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractSummaryDto>> Handle(GetExpiringContractsQuery request, CancellationToken ct)
    {
        var contracts = await _repository.GetExpiringContractsAsync(request.DaysUntilExpiration, ct);
        return contracts.Select(c => new ContractSummaryDto(
            c.Id, c.ContractNumber, c.Type, c.Title, c.Status, c.EffectiveDate,
            c.ExpirationDate, c.ContractValue, c.Currency, c.CreatedAt
        )).ToList();
    }
}

#endregion

#region Party Query Handlers

public class GetContractPartiesHandler : IRequestHandler<GetContractPartiesQuery, List<ContractPartyDto>>
{
    private readonly IContractPartyRepository _repository;

    public GetContractPartiesHandler(IContractPartyRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractPartyDto>> Handle(GetContractPartiesQuery request, CancellationToken ct)
    {
        var parties = await _repository.GetByContractIdAsync(request.ContractId, ct);
        return parties.Select(p => new ContractPartyDto(
            p.Id, p.ContractId, p.Type, p.Role, p.UserId, p.FullName, p.DocumentType,
            p.DocumentNumber, p.Email, p.Phone, p.Address, p.CompanyName, p.RNC,
            p.LegalRepresentative, p.PowerOfAttorneyNumber, p.HasSigned, p.SignedAt
        )).ToList();
    }
}

#endregion

#region Signature Query Handlers

public class GetContractSignaturesHandler : IRequestHandler<GetContractSignaturesQuery, List<ContractSignatureDto>>
{
    private readonly IContractSignatureRepository _repository;

    public GetContractSignaturesHandler(IContractSignatureRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractSignatureDto>> Handle(GetContractSignaturesQuery request, CancellationToken ct)
    {
        var signatures = await _repository.GetByContractIdAsync(request.ContractId, ct);
        return signatures.Select(s => new ContractSignatureDto(
            s.Id, s.ContractId, s.PartyId, s.Type, s.Status, s.SignatureData, s.SignatureImage,
            s.CertificateData, s.CertificationAuthorityId, s.DocumentHash, s.TimestampDate,
            s.SignedAt, s.IPAddress, s.GeoLocation, s.BiometricVerified, s.BiometricType,
            s.VerificationStatus, s.ExpiresAt, s.DeclineReason, s.DeclinedAt
        )).ToList();
    }
}

public class GetSignatureByIdHandler : IRequestHandler<GetSignatureByIdQuery, ContractSignatureDto?>
{
    private readonly IContractSignatureRepository _repository;

    public GetSignatureByIdHandler(IContractSignatureRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContractSignatureDto?> Handle(GetSignatureByIdQuery request, CancellationToken ct)
    {
        var s = await _repository.GetByIdAsync(request.Id, ct);
        return s == null ? null : new ContractSignatureDto(
            s.Id, s.ContractId, s.PartyId, s.Type, s.Status, s.SignatureData, s.SignatureImage,
            s.CertificateData, s.CertificationAuthorityId, s.DocumentHash, s.TimestampDate,
            s.SignedAt, s.IPAddress, s.GeoLocation, s.BiometricVerified, s.BiometricType,
            s.VerificationStatus, s.ExpiresAt, s.DeclineReason, s.DeclinedAt
        );
    }
}

public class GetPendingSignaturesHandler : IRequestHandler<GetPendingSignaturesQuery, List<ContractSignatureDto>>
{
    private readonly IContractSignatureRepository _repository;

    public GetPendingSignaturesHandler(IContractSignatureRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractSignatureDto>> Handle(GetPendingSignaturesQuery request, CancellationToken ct)
    {
        var signatures = await _repository.GetPendingSignaturesAsync(request.UserId, ct);
        return signatures.Select(s => new ContractSignatureDto(
            s.Id, s.ContractId, s.PartyId, s.Type, s.Status, s.SignatureData, s.SignatureImage,
            s.CertificateData, s.CertificationAuthorityId, s.DocumentHash, s.TimestampDate,
            s.SignedAt, s.IPAddress, s.GeoLocation, s.BiometricVerified, s.BiometricType,
            s.VerificationStatus, s.ExpiresAt, s.DeclineReason, s.DeclinedAt
        )).ToList();
    }
}

#endregion

#region Clause Query Handlers

public class GetContractClausesHandler : IRequestHandler<GetContractClausesQuery, List<ContractClauseDto>>
{
    private readonly IContractClauseRepository _repository;

    public GetContractClausesHandler(IContractClauseRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractClauseDto>> Handle(GetContractClausesQuery request, CancellationToken ct)
    {
        var clauses = await _repository.GetByContractIdAsync(request.ContractId, ct);
        return clauses.Select(c => new ContractClauseDto(
            c.Id, c.ContractId, c.TemplateClauseId, c.Code, c.Title, c.Content,
            c.OriginalContent, c.Type, c.Order, c.WasModified, c.ModifiedBy, c.AcceptedAt,
            c.ModificationReason
        )).ToList();
    }
}

#endregion

#region Version Query Handlers

public class GetContractVersionsHandler : IRequestHandler<GetContractVersionsQuery, List<ContractVersionDto>>
{
    private readonly IContractVersionRepository _repository;

    public GetContractVersionsHandler(IContractVersionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractVersionDto>> Handle(GetContractVersionsQuery request, CancellationToken ct)
    {
        var versions = await _repository.GetByContractIdAsync(request.ContractId, ct);
        return versions.Select(v => new ContractVersionDto(
            v.Id, v.ContractId, v.VersionNumber, v.ContentHtml, v.ContentHash,
            v.ChangeDescription, v.CreatedAt, v.CreatedBy
        )).ToList();
    }
}

#endregion

#region Document Query Handlers

public class GetContractDocumentsHandler : IRequestHandler<GetContractDocumentsQuery, List<ContractDocumentDto>>
{
    private readonly IContractDocumentRepository _repository;

    public GetContractDocumentsHandler(IContractDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractDocumentDto>> Handle(GetContractDocumentsQuery request, CancellationToken ct)
    {
        var documents = await _repository.GetByContractIdAsync(request.ContractId, ct);
        return documents.Select(d => new ContractDocumentDto(
            d.Id, d.ContractId, d.Name, d.Description, d.DocumentType, d.FileName,
            d.ContentType, d.FileSize, d.StoragePath, d.FileHash, d.IsRequired,
            d.UploadedAt, d.UploadedBy, d.VerifiedAt, d.VerifiedBy
        )).ToList();
    }
}

#endregion

#region Audit Query Handlers

public class GetContractAuditLogHandler : IRequestHandler<GetContractAuditLogQuery, List<ContractAuditLogDto>>
{
    private readonly IContractAuditLogRepository _repository;

    public GetContractAuditLogHandler(IContractAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractAuditLogDto>> Handle(GetContractAuditLogQuery request, CancellationToken ct)
    {
        var logs = await _repository.GetByContractIdAsync(request.ContractId, ct);
        return logs.Select(l => new ContractAuditLogDto(
            l.Id, l.ContractId, l.EventType, l.Description, l.OldValue, l.NewValue,
            l.PerformedBy, l.IPAddress, l.UserAgent, l.PerformedAt
        )).ToList();
    }
}

public class GetUserContractActivityHandler : IRequestHandler<GetUserContractActivityQuery, List<ContractAuditLogDto>>
{
    private readonly IContractAuditLogRepository _repository;

    public GetUserContractActivityHandler(IContractAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ContractAuditLogDto>> Handle(GetUserContractActivityQuery request, CancellationToken ct)
    {
        var logs = await _repository.GetByUserAsync(request.UserId, request.From, request.To, ct);
        return logs.Select(l => new ContractAuditLogDto(
            l.Id, l.ContractId, l.EventType, l.Description, l.OldValue, l.NewValue,
            l.PerformedBy, l.IPAddress, l.UserAgent, l.PerformedAt
        )).ToList();
    }
}

#endregion

#region Certification Authority Query Handlers

public class GetCertificationAuthoritiesHandler : IRequestHandler<GetCertificationAuthoritiesQuery, List<CertificationAuthorityDto>>
{
    private readonly ICertificationAuthorityRepository _repository;

    public GetCertificationAuthoritiesHandler(ICertificationAuthorityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CertificationAuthorityDto>> Handle(GetCertificationAuthoritiesQuery request, CancellationToken ct)
    {
        var authorities = await _repository.GetAllAsync(ct);
        return authorities.Select(a => new CertificationAuthorityDto(
            a.Id, a.Code, a.Name, a.Description, a.Country, a.Website, a.CertificateUrl,
            a.PublicKey, a.IsActive, a.IsGovernmentApproved, a.AccreditationNumber,
            a.ValidFrom, a.ValidUntil
        )).ToList();
    }
}

public class GetActiveCertificationAuthoritiesHandler : IRequestHandler<GetActiveCertificationAuthoritiesQuery, List<CertificationAuthorityDto>>
{
    private readonly ICertificationAuthorityRepository _repository;

    public GetActiveCertificationAuthoritiesHandler(ICertificationAuthorityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CertificationAuthorityDto>> Handle(GetActiveCertificationAuthoritiesQuery request, CancellationToken ct)
    {
        var authorities = await _repository.GetActiveAsync(ct);
        return authorities.Select(a => new CertificationAuthorityDto(
            a.Id, a.Code, a.Name, a.Description, a.Country, a.Website, a.CertificateUrl,
            a.PublicKey, a.IsActive, a.IsGovernmentApproved, a.AccreditationNumber,
            a.ValidFrom, a.ValidUntil
        )).ToList();
    }
}

#endregion

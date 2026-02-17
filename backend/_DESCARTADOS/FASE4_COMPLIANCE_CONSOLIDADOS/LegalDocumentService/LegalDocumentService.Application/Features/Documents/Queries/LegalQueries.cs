using MediatR;
using LegalDocumentService.Application.DTOs;
using LegalDocumentService.Domain.Entities;
using LegalDocumentService.Domain.Interfaces;
using LegalDocumentService.Domain.Enums;

namespace LegalDocumentService.Application.Features.Documents.Queries;

// ===== GET LEGAL DOCUMENT BY ID =====

public record GetLegalDocumentByIdQuery(Guid Id) : IRequest<LegalDocumentDto?>;

public class GetLegalDocumentByIdHandler : IRequestHandler<GetLegalDocumentByIdQuery, LegalDocumentDto?>
{
    private readonly ILegalDocumentRepository _repository;

    public GetLegalDocumentByIdHandler(ILegalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<LegalDocumentDto?> Handle(GetLegalDocumentByIdQuery request, CancellationToken ct)
    {
        var document = await _repository.GetByIdAsync(request.Id, ct);
        return document == null ? null : MapToDto(document);
    }

    private static LegalDocumentDto MapToDto(LegalDocument d) => new(
        d.Id, d.Title, d.Slug, d.DocumentType, d.Status, d.Content, d.ContentHtml,
        d.Summary, d.VersionMajor, d.VersionMinor, d.VersionLabel, d.Jurisdiction,
        d.Language, d.EffectiveDate, d.ExpirationDate, d.PublishedAt, d.RequiresAcceptance,
        d.IsActive, d.IsMandatory, d.CreatedBy, d.ApprovedBy, d.ApprovedAt,
        d.LegalReferences, d.CreatedAt, d.UpdatedAt);
}

// ===== GET LEGAL DOCUMENT BY SLUG =====

public record GetLegalDocumentBySlugQuery(string Slug) : IRequest<LegalDocumentDto?>;

public class GetLegalDocumentBySlugHandler : IRequestHandler<GetLegalDocumentBySlugQuery, LegalDocumentDto?>
{
    private readonly ILegalDocumentRepository _repository;

    public GetLegalDocumentBySlugHandler(ILegalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<LegalDocumentDto?> Handle(GetLegalDocumentBySlugQuery request, CancellationToken ct)
    {
        var document = await _repository.GetBySlugAsync(request.Slug, ct);
        return document == null ? null : MapToDto(document);
    }

    private static LegalDocumentDto MapToDto(LegalDocument d) => new(
        d.Id, d.Title, d.Slug, d.DocumentType, d.Status, d.Content, d.ContentHtml,
        d.Summary, d.VersionMajor, d.VersionMinor, d.VersionLabel, d.Jurisdiction,
        d.Language, d.EffectiveDate, d.ExpirationDate, d.PublishedAt, d.RequiresAcceptance,
        d.IsActive, d.IsMandatory, d.CreatedBy, d.ApprovedBy, d.ApprovedAt,
        d.LegalReferences, d.CreatedAt, d.UpdatedAt);
}

// ===== GET ACTIVE DOCUMENTS =====

public record GetActiveDocumentsQuery : IRequest<List<LegalDocumentSummaryDto>>;

public class GetActiveDocumentsHandler : IRequestHandler<GetActiveDocumentsQuery, List<LegalDocumentSummaryDto>>
{
    private readonly ILegalDocumentRepository _repository;

    public GetActiveDocumentsHandler(ILegalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<LegalDocumentSummaryDto>> Handle(GetActiveDocumentsQuery request, CancellationToken ct)
    {
        var documents = await _repository.GetActiveAsync(ct);
        return documents.Select(d => new LegalDocumentSummaryDto(
            d.Id, d.Title, d.Slug, d.DocumentType, d.Status, d.VersionLabel,
            d.IsActive, d.RequiresAcceptance, d.EffectiveDate)).ToList();
    }
}

// ===== GET DOCUMENTS BY TYPE =====

public record GetDocumentsByTypeQuery(LegalDocumentType Type) : IRequest<List<LegalDocumentSummaryDto>>;

public class GetDocumentsByTypeHandler : IRequestHandler<GetDocumentsByTypeQuery, List<LegalDocumentSummaryDto>>
{
    private readonly ILegalDocumentRepository _repository;

    public GetDocumentsByTypeHandler(ILegalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<LegalDocumentSummaryDto>> Handle(GetDocumentsByTypeQuery request, CancellationToken ct)
    {
        var documents = await _repository.GetByTypeAsync(request.Type, ct);
        return documents.Select(d => new LegalDocumentSummaryDto(
            d.Id, d.Title, d.Slug, d.DocumentType, d.Status, d.VersionLabel,
            d.IsActive, d.RequiresAcceptance, d.EffectiveDate)).ToList();
    }
}

// ===== GET DOCUMENTS REQUIRING ACCEPTANCE =====

public record GetDocumentsRequiringAcceptanceQuery : IRequest<List<LegalDocumentSummaryDto>>;

public class GetDocumentsRequiringAcceptanceHandler : IRequestHandler<GetDocumentsRequiringAcceptanceQuery, List<LegalDocumentSummaryDto>>
{
    private readonly ILegalDocumentRepository _repository;

    public GetDocumentsRequiringAcceptanceHandler(ILegalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<LegalDocumentSummaryDto>> Handle(GetDocumentsRequiringAcceptanceQuery request, CancellationToken ct)
    {
        var documents = await _repository.GetRequiringAcceptanceAsync(ct);
        return documents.Select(d => new LegalDocumentSummaryDto(
            d.Id, d.Title, d.Slug, d.DocumentType, d.Status, d.VersionLabel,
            d.IsActive, d.RequiresAcceptance, d.EffectiveDate)).ToList();
    }
}

// ===== GET USER ACCEPTANCES =====

public record GetUserAcceptancesQuery(string UserId) : IRequest<List<UserAcceptanceDto>>;

public class GetUserAcceptancesHandler : IRequestHandler<GetUserAcceptancesQuery, List<UserAcceptanceDto>>
{
    private readonly IUserAcceptanceRepository _repository;

    public GetUserAcceptancesHandler(IUserAcceptanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserAcceptanceDto>> Handle(GetUserAcceptancesQuery request, CancellationToken ct)
    {
        var acceptances = await _repository.GetByUserIdAsync(request.UserId, ct);
        return acceptances.Select(a => new UserAcceptanceDto(
            a.Id, a.LegalDocumentId, a.UserId, a.TransactionId, a.Status, a.Method,
            a.AcceptedAt, a.DeclinedAt, a.RevokedAt, a.IpAddress, a.UserAgent,
            a.GeoLocation, a.DocumentVersionAccepted, a.CreatedAt)).ToList();
    }
}

// ===== CHECK USER ACCEPTANCE STATUS =====

public record CheckUserAcceptanceQuery(string UserId, Guid DocumentId) : IRequest<UserAcceptanceStatusDto>;

public class CheckUserAcceptanceHandler : IRequestHandler<CheckUserAcceptanceQuery, UserAcceptanceStatusDto>
{
    private readonly IUserAcceptanceRepository _acceptanceRepository;
    private readonly ILegalDocumentRepository _documentRepository;

    public CheckUserAcceptanceHandler(
        IUserAcceptanceRepository acceptanceRepository,
        ILegalDocumentRepository documentRepository)
    {
        _acceptanceRepository = acceptanceRepository;
        _documentRepository = documentRepository;
    }

    public async Task<UserAcceptanceStatusDto> Handle(CheckUserAcceptanceQuery request, CancellationToken ct)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, ct)
            ?? throw new InvalidOperationException($"Documento {request.DocumentId} no encontrado");

        var acceptance = await _acceptanceRepository.GetUserDocumentAcceptanceAsync(
            request.UserId, request.DocumentId, ct);

        var hasAccepted = acceptance?.Status == AcceptanceStatus.Accepted;
        var needsReAcceptance = hasAccepted &&
            acceptance!.DocumentVersionAccepted != document.VersionLabel;

        return new UserAcceptanceStatusDto(
            document.Id,
            document.Title,
            document.VersionLabel,
            hasAccepted,
            acceptance?.AcceptedAt,
            needsReAcceptance);
    }
}

// ===== GET PENDING USER ACCEPTANCES =====

public record GetPendingAcceptancesQuery(string UserId) : IRequest<List<LegalDocumentSummaryDto>>;

public class GetPendingAcceptancesHandler : IRequestHandler<GetPendingAcceptancesQuery, List<LegalDocumentSummaryDto>>
{
    private readonly ILegalDocumentRepository _documentRepository;
    private readonly IUserAcceptanceRepository _acceptanceRepository;

    public GetPendingAcceptancesHandler(
        ILegalDocumentRepository documentRepository,
        IUserAcceptanceRepository acceptanceRepository)
    {
        _documentRepository = documentRepository;
        _acceptanceRepository = acceptanceRepository;
    }

    public async Task<List<LegalDocumentSummaryDto>> Handle(GetPendingAcceptancesQuery request, CancellationToken ct)
    {
        var requiringAcceptance = await _documentRepository.GetRequiringAcceptanceAsync(ct);
        var userAcceptances = await _acceptanceRepository.GetAcceptedByUserAsync(request.UserId, ct);
        var acceptedDocIds = userAcceptances.Select(a => a.LegalDocumentId).ToHashSet();

        var pending = requiringAcceptance
            .Where(d => !acceptedDocIds.Contains(d.Id))
            .Select(d => new LegalDocumentSummaryDto(
                d.Id, d.Title, d.Slug, d.DocumentType, d.Status, d.VersionLabel,
                d.IsActive, d.RequiresAcceptance, d.EffectiveDate))
            .ToList();

        return pending;
    }
}

// ===== GET TEMPLATES =====

public record GetActiveTemplatesQuery : IRequest<List<DocumentTemplateDto>>;

public class GetActiveTemplatesHandler : IRequestHandler<GetActiveTemplatesQuery, List<DocumentTemplateDto>>
{
    private readonly IDocumentTemplateRepository _repository;

    public GetActiveTemplatesHandler(IDocumentTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<DocumentTemplateDto>> Handle(GetActiveTemplatesQuery request, CancellationToken ct)
    {
        var templates = await _repository.GetActiveAsync(ct);
        return templates.Select(t => new DocumentTemplateDto(
            t.Id, t.Name, t.Code, t.DocumentType, t.TemplateContent, t.Description,
            t.Language, t.Jurisdiction, t.IsActive, t.CreatedBy,
            t.Variables.Select(v => new TemplateVariableDto(
                v.Id, v.VariableName, v.Placeholder, v.VariableType,
                v.IsRequired, v.DefaultValue, v.ValidationRegex, v.Description)).ToList(),
            t.CreatedAt)).ToList();
    }
}

// ===== GET COMPLIANCE REQUIREMENTS =====

public record GetActiveComplianceRequirementsQuery : IRequest<List<ComplianceRequirementDto>>;

public class GetActiveComplianceRequirementsHandler : IRequestHandler<GetActiveComplianceRequirementsQuery, List<ComplianceRequirementDto>>
{
    private readonly IComplianceRequirementRepository _repository;

    public GetActiveComplianceRequirementsHandler(IComplianceRequirementRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ComplianceRequirementDto>> Handle(GetActiveComplianceRequirementsQuery request, CancellationToken ct)
    {
        var requirements = await _repository.GetActiveAsync(ct);
        return requirements.Select(r => new ComplianceRequirementDto(
            r.Id, r.Name, r.Code, r.Description, r.LegalBasis, r.ArticleReference,
            r.Jurisdiction, r.IsMandatory, r.IsActive, r.EffectiveDate, r.SunsetDate,
            r.RequiredDocuments.Select(d => new RequiredDocumentDto(
                d.Id, d.DocumentType, d.Description, d.IsMandatory, d.DisplayOrder)).ToList(),
            r.CreatedAt)).ToList();
    }
}

// ===== GET STATISTICS =====

public record GetLegalStatisticsQuery : IRequest<LegalStatisticsDto>;

public class GetLegalStatisticsHandler : IRequestHandler<GetLegalStatisticsQuery, LegalStatisticsDto>
{
    private readonly ILegalDocumentRepository _documentRepository;
    private readonly IUserAcceptanceRepository _acceptanceRepository;
    private readonly IDocumentTemplateRepository _templateRepository;
    private readonly IComplianceRequirementRepository _requirementRepository;

    public GetLegalStatisticsHandler(
        ILegalDocumentRepository documentRepository,
        IUserAcceptanceRepository acceptanceRepository,
        IDocumentTemplateRepository templateRepository,
        IComplianceRequirementRepository requirementRepository)
    {
        _documentRepository = documentRepository;
        _acceptanceRepository = acceptanceRepository;
        _templateRepository = templateRepository;
        _requirementRepository = requirementRepository;
    }

    public async Task<LegalStatisticsDto> Handle(GetLegalStatisticsQuery request, CancellationToken ct)
    {
        var allDocs = await _documentRepository.GetAllAsync(ct);
        var activeDocs = await _documentRepository.GetActiveAsync(ct);
        var templates = await _templateRepository.GetAllAsync(ct);
        var requirements = await _requirementRepository.GetAllAsync(ct);

        var docsByType = allDocs
            .GroupBy(d => d.DocumentType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return new LegalStatisticsDto(
            TotalDocuments: allDocs.Count,
            ActiveDocuments: activeDocs.Count,
            DraftDocuments: allDocs.Count(d => d.Status == LegalDocumentStatus.Draft),
            PublishedDocuments: allDocs.Count(d => d.Status == LegalDocumentStatus.Published),
            DocumentsRequiringAcceptance: allDocs.Count(d => d.RequiresAcceptance),
            TotalAcceptances: 0,
            PendingAcceptances: 0,
            TotalTemplates: templates.Count,
            TotalComplianceRequirements: requirements.Count,
            DocumentsByType: docsByType,
            AcceptancesByStatus: new Dictionary<string, int>());
    }
}

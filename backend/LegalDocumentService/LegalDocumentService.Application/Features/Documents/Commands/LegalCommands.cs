using MediatR;
using LegalDocumentService.Application.DTOs;
using LegalDocumentService.Domain.Entities;
using LegalDocumentService.Domain.Interfaces;
using LegalDocumentService.Domain.Enums;

namespace LegalDocumentService.Application.Features.Documents.Commands;

// ===== CREATE LEGAL DOCUMENT =====

public record CreateLegalDocumentCommand(CreateLegalDocumentDto Dto) : IRequest<LegalDocumentDto>;

public class CreateLegalDocumentHandler : IRequestHandler<CreateLegalDocumentCommand, LegalDocumentDto>
{
    private readonly ILegalDocumentRepository _repository;

    public CreateLegalDocumentHandler(ILegalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<LegalDocumentDto> Handle(CreateLegalDocumentCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var document = new LegalDocument(
            dto.Title,
            dto.DocumentType,
            dto.Content,
            dto.Jurisdiction,
            dto.Language,
            dto.RequiresAcceptance,
            dto.IsMandatory,
            dto.CreatedBy);

        if (!string.IsNullOrEmpty(dto.ContentHtml))
            document.UpdateContent(dto.Content, dto.ContentHtml);

        if (!string.IsNullOrEmpty(dto.Summary))
            document.SetSummary(dto.Summary);

        if (!string.IsNullOrEmpty(dto.LegalReferences))
            document.SetLegalReferences(dto.LegalReferences);

        var created = await _repository.AddAsync(document, ct);

        return MapToDto(created);
    }

    private static LegalDocumentDto MapToDto(LegalDocument d) => new(
        d.Id, d.Title, d.Slug, d.DocumentType, d.Status, d.Content, d.ContentHtml,
        d.Summary, d.VersionMajor, d.VersionMinor, d.VersionLabel, d.Jurisdiction,
        d.Language, d.EffectiveDate, d.ExpirationDate, d.PublishedAt, d.RequiresAcceptance,
        d.IsActive, d.IsMandatory, d.CreatedBy, d.ApprovedBy, d.ApprovedAt,
        d.LegalReferences, d.CreatedAt, d.UpdatedAt);
}

// ===== UPDATE LEGAL DOCUMENT =====

public record UpdateLegalDocumentCommand(UpdateLegalDocumentDto Dto) : IRequest<LegalDocumentDto>;

public class UpdateLegalDocumentHandler : IRequestHandler<UpdateLegalDocumentCommand, LegalDocumentDto>
{
    private readonly ILegalDocumentRepository _repository;

    public UpdateLegalDocumentHandler(ILegalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<LegalDocumentDto> Handle(UpdateLegalDocumentCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var document = await _repository.GetByIdAsync(dto.Id, ct)
            ?? throw new InvalidOperationException($"Documento {dto.Id} no encontrado");

        if (!string.IsNullOrEmpty(dto.Content))
            document.UpdateContent(dto.Content, dto.ContentHtml);

        if (!string.IsNullOrEmpty(dto.Summary))
            document.SetSummary(dto.Summary);

        if (!string.IsNullOrEmpty(dto.LegalReferences))
            document.SetLegalReferences(dto.LegalReferences);

        var updated = await _repository.UpdateAsync(document, ct);

        return MapToDto(updated);
    }

    private static LegalDocumentDto MapToDto(LegalDocument d) => new(
        d.Id, d.Title, d.Slug, d.DocumentType, d.Status, d.Content, d.ContentHtml,
        d.Summary, d.VersionMajor, d.VersionMinor, d.VersionLabel, d.Jurisdiction,
        d.Language, d.EffectiveDate, d.ExpirationDate, d.PublishedAt, d.RequiresAcceptance,
        d.IsActive, d.IsMandatory, d.CreatedBy, d.ApprovedBy, d.ApprovedAt,
        d.LegalReferences, d.CreatedAt, d.UpdatedAt);
}

// ===== PUBLISH DOCUMENT =====

public record PublishDocumentCommand(PublishDocumentDto Dto) : IRequest<LegalDocumentDto>;

public class PublishDocumentHandler : IRequestHandler<PublishDocumentCommand, LegalDocumentDto>
{
    private readonly ILegalDocumentRepository _repository;

    public PublishDocumentHandler(ILegalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<LegalDocumentDto> Handle(PublishDocumentCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var document = await _repository.GetByIdAsync(dto.DocumentId, ct)
            ?? throw new InvalidOperationException($"Documento {dto.DocumentId} no encontrado");

        if (dto.IncrementMajorVersion)
            document.IncrementVersion(true, "Nueva versión publicada");

        document.Publish(dto.ApprovedBy);

        var updated = await _repository.UpdateAsync(document, ct);

        return MapToDto(updated);
    }

    private static LegalDocumentDto MapToDto(LegalDocument d) => new(
        d.Id, d.Title, d.Slug, d.DocumentType, d.Status, d.Content, d.ContentHtml,
        d.Summary, d.VersionMajor, d.VersionMinor, d.VersionLabel, d.Jurisdiction,
        d.Language, d.EffectiveDate, d.ExpirationDate, d.PublishedAt, d.RequiresAcceptance,
        d.IsActive, d.IsMandatory, d.CreatedBy, d.ApprovedBy, d.ApprovedAt,
        d.LegalReferences, d.CreatedAt, d.UpdatedAt);
}

// ===== ARCHIVE DOCUMENT =====

public record ArchiveDocumentCommand(Guid DocumentId) : IRequest<bool>;

public class ArchiveDocumentHandler : IRequestHandler<ArchiveDocumentCommand, bool>
{
    private readonly ILegalDocumentRepository _repository;

    public ArchiveDocumentHandler(ILegalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(ArchiveDocumentCommand request, CancellationToken ct)
    {
        var document = await _repository.GetByIdAsync(request.DocumentId, ct)
            ?? throw new InvalidOperationException($"Documento {request.DocumentId} no encontrado");

        document.Archive();
        await _repository.UpdateAsync(document, ct);
        return true;
    }
}

// ===== RECORD USER ACCEPTANCE =====

public record RecordUserAcceptanceCommand(CreateAcceptanceDto Dto) : IRequest<UserAcceptanceDto>;

public class RecordUserAcceptanceHandler : IRequestHandler<RecordUserAcceptanceCommand, UserAcceptanceDto>
{
    private readonly IUserAcceptanceRepository _acceptanceRepository;
    private readonly ILegalDocumentRepository _documentRepository;

    public RecordUserAcceptanceHandler(
        IUserAcceptanceRepository acceptanceRepository,
        ILegalDocumentRepository documentRepository)
    {
        _acceptanceRepository = acceptanceRepository;
        _documentRepository = documentRepository;
    }

    public async Task<UserAcceptanceDto> Handle(RecordUserAcceptanceCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var document = await _documentRepository.GetByIdAsync(dto.LegalDocumentId, ct)
            ?? throw new InvalidOperationException($"Documento {dto.LegalDocumentId} no encontrado");

        var acceptance = new UserAcceptance(
            dto.LegalDocumentId,
            dto.UserId,
            dto.Method,
            dto.IpAddress,
            dto.UserAgent,
            document.VersionLabel,
            dto.TransactionId);

        if (!string.IsNullOrEmpty(dto.GeoLocation))
            acceptance.SetGeoLocation(dto.GeoLocation);

        acceptance.Accept();

        var created = await _acceptanceRepository.AddAsync(acceptance, ct);

        return MapToDto(created);
    }

    private static UserAcceptanceDto MapToDto(UserAcceptance a) => new(
        a.Id, a.LegalDocumentId, a.UserId, a.TransactionId, a.Status, a.Method,
        a.AcceptedAt, a.DeclinedAt, a.RevokedAt, a.IpAddress, a.UserAgent,
        a.GeoLocation, a.DocumentVersionAccepted, a.CreatedAt);
}

// ===== REVOKE ACCEPTANCE =====

public record RevokeAcceptanceCommand(RevokeAcceptanceDto Dto) : IRequest<bool>;

public class RevokeAcceptanceHandler : IRequestHandler<RevokeAcceptanceCommand, bool>
{
    private readonly IUserAcceptanceRepository _repository;

    public RevokeAcceptanceHandler(IUserAcceptanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(RevokeAcceptanceCommand request, CancellationToken ct)
    {
        var acceptance = await _repository.GetByIdAsync(request.Dto.AcceptanceId, ct)
            ?? throw new InvalidOperationException($"Aceptación {request.Dto.AcceptanceId} no encontrada");

        acceptance.Revoke(request.Dto.Reason);
        await _repository.UpdateAsync(acceptance, ct);
        return true;
    }
}

// ===== CREATE TEMPLATE =====

public record CreateTemplateCommand(CreateTemplateDto Dto) : IRequest<DocumentTemplateDto>;

public class CreateTemplateHandler : IRequestHandler<CreateTemplateCommand, DocumentTemplateDto>
{
    private readonly IDocumentTemplateRepository _repository;

    public CreateTemplateHandler(IDocumentTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<DocumentTemplateDto> Handle(CreateTemplateCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var template = new DocumentTemplate(
            dto.Name,
            dto.Code,
            dto.DocumentType,
            dto.TemplateContent,
            dto.CreatedBy);

        if (dto.Variables != null)
        {
            foreach (var v in dto.Variables)
            {
                template.AddVariable(new TemplateVariable(
                    template.Id,
                    v.VariableName,
                    v.Placeholder,
                    v.VariableType,
                    v.IsRequired,
                    v.DefaultValue));
            }
        }

        var created = await _repository.AddAsync(template, ct);

        return MapToDto(created);
    }

    private static DocumentTemplateDto MapToDto(DocumentTemplate t) => new(
        t.Id, t.Name, t.Code, t.DocumentType, t.TemplateContent, t.Description,
        t.Language, t.Jurisdiction, t.IsActive, t.CreatedBy,
        t.Variables.Select(v => new TemplateVariableDto(
            v.Id, v.VariableName, v.Placeholder, v.VariableType,
            v.IsRequired, v.DefaultValue, v.ValidationRegex, v.Description)).ToList(),
        t.CreatedAt);
}

// ===== CREATE COMPLIANCE REQUIREMENT =====

public record CreateComplianceRequirementCommand(CreateComplianceRequirementDto Dto) : IRequest<ComplianceRequirementDto>;

public class CreateComplianceRequirementHandler : IRequestHandler<CreateComplianceRequirementCommand, ComplianceRequirementDto>
{
    private readonly IComplianceRequirementRepository _repository;

    public CreateComplianceRequirementHandler(IComplianceRequirementRepository repository)
    {
        _repository = repository;
    }

    public async Task<ComplianceRequirementDto> Handle(CreateComplianceRequirementCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var requirement = new ComplianceRequirement(
            dto.Name,
            dto.Code,
            dto.Description,
            dto.LegalBasis,
            dto.IsMandatory);

        var created = await _repository.AddAsync(requirement, ct);

        return MapToDto(created);
    }

    private static ComplianceRequirementDto MapToDto(ComplianceRequirement r) => new(
        r.Id, r.Name, r.Code, r.Description, r.LegalBasis, r.ArticleReference,
        r.Jurisdiction, r.IsMandatory, r.IsActive, r.EffectiveDate, r.SunsetDate,
        r.RequiredDocuments.Select(d => new RequiredDocumentDto(
            d.Id, d.DocumentType, d.Description, d.IsMandatory, d.DisplayOrder)).ToList(),
        r.CreatedAt);
}

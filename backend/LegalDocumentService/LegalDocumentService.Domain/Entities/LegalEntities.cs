using LegalDocumentService.Domain.Common;
using LegalDocumentService.Domain.Enums;

namespace LegalDocumentService.Domain.Entities;

/// <summary>
/// Legal document entity
/// Representa documentos legales: Términos, Políticas, Contratos
/// Cumple con Ley 126-02 (E-Commerce), Ley 172-13 (Datos), Pro-Consumidor
/// </summary>
public class LegalDocument : EntityBase, IAggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public LegalDocumentType DocumentType { get; private set; }
    public LegalDocumentStatus Status { get; private set; } = LegalDocumentStatus.Draft;
    public string Content { get; private set; } = string.Empty;
    public string ContentHtml { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    
    public int VersionMajor { get; private set; } = 1;
    public int VersionMinor { get; private set; } = 0;
    public string VersionLabel { get; private set; } = "1.0";
    
    public Jurisdiction Jurisdiction { get; private set; } = Jurisdiction.DominicanRepublic;
    public DocumentLanguage Language { get; private set; } = DocumentLanguage.Spanish;
    
    public DateTime EffectiveDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    
    public bool RequiresAcceptance { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsMandatory { get; private set; }
    
    public string? CreatedBy { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    
    public string? LegalReferences { get; private set; }
    public string? Notes { get; private set; }
    public string? MetadataJson { get; private set; }

    private readonly List<LegalDocumentVersion> _versions = new();
    public IReadOnlyCollection<LegalDocumentVersion> Versions => _versions.AsReadOnly();

    private LegalDocument() { }

    public LegalDocument(
        string title,
        LegalDocumentType documentType,
        string content,
        Jurisdiction jurisdiction = Jurisdiction.DominicanRepublic,
        DocumentLanguage language = DocumentLanguage.Spanish,
        bool requiresAcceptance = false,
        bool isMandatory = false,
        string? createdBy = null)
    {
        Title = title;
        Slug = GenerateSlug(title);
        DocumentType = documentType;
        Content = content;
        Jurisdiction = jurisdiction;
        Language = language;
        RequiresAcceptance = requiresAcceptance;
        IsMandatory = isMandatory;
        CreatedBy = createdBy;
        EffectiveDate = DateTime.UtcNow;
    }

    public void UpdateContent(string content, string? contentHtml = null)
    {
        Content = content;
        if (!string.IsNullOrEmpty(contentHtml))
            ContentHtml = contentHtml;
        MarkAsUpdated();
    }

    public void SetSummary(string summary)
    {
        Summary = summary;
        MarkAsUpdated();
    }

    public void Publish(string approvedBy)
    {
        Status = LegalDocumentStatus.Published;
        IsActive = true;
        PublishedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Archive()
    {
        Status = LegalDocumentStatus.Archived;
        IsActive = false;
        MarkAsUpdated();
    }

    public void IncrementVersion(bool major = false, string? changeNotes = null)
    {
        // Save current version to history
        _versions.Add(new LegalDocumentVersion(
            Id,
            VersionMajor,
            VersionMinor,
            VersionLabel,
            Content,
            ContentHtml,
            changeNotes ?? "Version update"));

        if (major)
        {
            VersionMajor++;
            VersionMinor = 0;
        }
        else
        {
            VersionMinor++;
        }
        VersionLabel = $"{VersionMajor}.{VersionMinor}";
        MarkAsUpdated();
    }

    public void SetLegalReferences(string references)
    {
        LegalReferences = references;
        MarkAsUpdated();
    }

    private static string GenerateSlug(string title)
    {
        return title.ToLower()
            .Replace(" ", "-")
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u")
            .Replace("ñ", "n");
    }
}

/// <summary>
/// Version history for legal documents
/// </summary>
public class LegalDocumentVersion : EntityBase
{
    public Guid LegalDocumentId { get; private set; }
    public int VersionMajor { get; private set; }
    public int VersionMinor { get; private set; }
    public string VersionLabel { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? ContentHtml { get; private set; }
    public string ChangeNotes { get; private set; } = string.Empty;

    public LegalDocument? LegalDocument { get; private set; }

    private LegalDocumentVersion() { }

    public LegalDocumentVersion(
        Guid legalDocumentId,
        int versionMajor,
        int versionMinor,
        string versionLabel,
        string content,
        string? contentHtml,
        string changeNotes)
    {
        LegalDocumentId = legalDocumentId;
        VersionMajor = versionMajor;
        VersionMinor = versionMinor;
        VersionLabel = versionLabel;
        Content = content;
        ContentHtml = contentHtml;
        ChangeNotes = changeNotes;
    }
}

/// <summary>
/// User acceptance record for legal documents
/// Ley 172-13: Registro de consentimiento
/// </summary>
public class UserAcceptance : EntityBase, IAggregateRoot
{
    public Guid LegalDocumentId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string? TransactionId { get; private set; }
    
    public AcceptanceStatus Status { get; private set; } = AcceptanceStatus.Pending;
    public AcceptanceMethod Method { get; private set; }
    
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? DeclinedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public string? GeoLocation { get; private set; }
    
    public string DocumentVersionAccepted { get; private set; } = string.Empty;
    public string? DocumentChecksum { get; private set; }
    
    public string? SignatureDataJson { get; private set; }
    public string? ConsentProofJson { get; private set; }
    
    public LegalDocument? LegalDocument { get; private set; }

    private UserAcceptance() { }

    public UserAcceptance(
        Guid legalDocumentId,
        string userId,
        AcceptanceMethod method,
        string ipAddress,
        string userAgent,
        string documentVersion,
        string? transactionId = null)
    {
        LegalDocumentId = legalDocumentId;
        UserId = userId;
        Method = method;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        DocumentVersionAccepted = documentVersion;
        TransactionId = transactionId;
    }

    public void Accept(string? signatureData = null)
    {
        Status = AcceptanceStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        SignatureDataJson = signatureData;
        MarkAsUpdated();
    }

    public void Decline()
    {
        Status = AcceptanceStatus.Declined;
        DeclinedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Revoke(string reason)
    {
        Status = AcceptanceStatus.Revoked;
        RevokedAt = DateTime.UtcNow;
        ConsentProofJson = System.Text.Json.JsonSerializer.Serialize(new { Reason = reason, RevokedAt = DateTime.UtcNow });
        MarkAsUpdated();
    }

    public void SetGeoLocation(string geoLocation)
    {
        GeoLocation = geoLocation;
        MarkAsUpdated();
    }

    public void SetChecksum(string checksum)
    {
        DocumentChecksum = checksum;
        MarkAsUpdated();
    }
}

/// <summary>
/// Legal document template
/// Templates con variables para generar contratos personalizados
/// </summary>
public class DocumentTemplate : EntityBase, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public LegalDocumentType DocumentType { get; private set; }
    public string TemplateContent { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    
    public DocumentLanguage Language { get; private set; } = DocumentLanguage.Spanish;
    public Jurisdiction Jurisdiction { get; private set; } = Jurisdiction.DominicanRepublic;
    
    public bool IsActive { get; private set; } = true;
    public string? CreatedBy { get; private set; }

    private readonly List<TemplateVariable> _variables = new();
    public IReadOnlyCollection<TemplateVariable> Variables => _variables.AsReadOnly();

    private DocumentTemplate() { }

    public DocumentTemplate(
        string name,
        string code,
        LegalDocumentType documentType,
        string templateContent,
        string? createdBy = null)
    {
        Name = name;
        Code = code;
        DocumentType = documentType;
        TemplateContent = templateContent;
        CreatedBy = createdBy;
    }

    public void AddVariable(TemplateVariable variable)
    {
        _variables.Add(variable);
        MarkAsUpdated();
    }

    public void UpdateTemplate(string content)
    {
        TemplateContent = content;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }
}

/// <summary>
/// Template variable definition
/// </summary>
public class TemplateVariable : EntityBase
{
    public Guid DocumentTemplateId { get; private set; }
    public string VariableName { get; private set; } = string.Empty;
    public string Placeholder { get; private set; } = string.Empty;
    public TemplateVariableType VariableType { get; private set; }
    public bool IsRequired { get; private set; }
    public string? DefaultValue { get; private set; }
    public string? ValidationRegex { get; private set; }
    public string? Description { get; private set; }

    public DocumentTemplate? DocumentTemplate { get; private set; }

    private TemplateVariable() { }

    public TemplateVariable(
        Guid templateId,
        string variableName,
        string placeholder,
        TemplateVariableType variableType,
        bool isRequired = false,
        string? defaultValue = null)
    {
        DocumentTemplateId = templateId;
        VariableName = variableName;
        Placeholder = placeholder;
        VariableType = variableType;
        IsRequired = isRequired;
        DefaultValue = defaultValue;
    }
}

/// <summary>
/// Compliance requirement tracker
/// Requisitos legales por tipo de operación
/// </summary>
public class ComplianceRequirement : EntityBase, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    public string LegalBasis { get; private set; } = string.Empty;
    public string? ArticleReference { get; private set; }
    
    public Jurisdiction Jurisdiction { get; private set; } = Jurisdiction.DominicanRepublic;
    
    public bool IsMandatory { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    public DateTime EffectiveDate { get; private set; }
    public DateTime? SunsetDate { get; private set; }

    private readonly List<RequiredDocument> _requiredDocuments = new();
    public IReadOnlyCollection<RequiredDocument> RequiredDocuments => _requiredDocuments.AsReadOnly();

    private ComplianceRequirement() { }

    public ComplianceRequirement(
        string name,
        string code,
        string description,
        string legalBasis,
        bool isMandatory = true)
    {
        Name = name;
        Code = code;
        Description = description;
        LegalBasis = legalBasis;
        IsMandatory = isMandatory;
        EffectiveDate = DateTime.UtcNow;
    }

    public void AddRequiredDocument(LegalDocumentType documentType, string description)
    {
        _requiredDocuments.Add(new RequiredDocument(Id, documentType, description));
        MarkAsUpdated();
    }
}

/// <summary>
/// Required document for a compliance requirement
/// </summary>
public class RequiredDocument : EntityBase
{
    public Guid ComplianceRequirementId { get; private set; }
    public LegalDocumentType DocumentType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool IsMandatory { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    public ComplianceRequirement? ComplianceRequirement { get; private set; }

    private RequiredDocument() { }

    public RequiredDocument(
        Guid requirementId,
        LegalDocumentType documentType,
        string description,
        bool isMandatory = true)
    {
        ComplianceRequirementId = requirementId;
        DocumentType = documentType;
        Description = description;
        IsMandatory = isMandatory;
    }
}

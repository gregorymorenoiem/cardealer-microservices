// =====================================================
// ConsumerProtectionService - Entities
// Ley 358-05 Derechos del Consumidor de RD
// =====================================================

using ConsumerProtectionService.Domain.Enums;

namespace ConsumerProtectionService.Domain.Entities;

/// <summary>
/// Garantía de producto según Ley 358-05
/// </summary>
public class Warranty
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public Guid? ConsumerId { get; set; }
    public string WarrantyNumber { get; set; } = string.Empty;
    public WarrantyType WarrantyType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public WarrantyStatus Status { get; set; }
    public string? CoverageDescription { get; set; }
    public string? Exclusions { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<WarrantyClaim> Claims { get; set; } = new List<WarrantyClaim>();
}

/// <summary>
/// Reclamación de garantía
/// </summary>
public class WarrantyClaim
{
    public Guid Id { get; set; }
    public Guid WarrantyId { get; set; }
    public Guid ConsumerId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string IssueDescription { get; set; } = string.Empty;
    public DateTime ClaimDate { get; set; }
    public ComplaintStatus Status { get; set; }
    public ResolutionType? Resolution { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Warranty? Warranty { get; set; }
}

/// <summary>
/// Reclamación del consumidor
/// </summary>
public class Complaint
{
    public Guid Id { get; set; }
    public Guid ConsumerId { get; set; }
    public Guid? SellerId { get; set; }
    public Guid? ProductId { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public ComplaintType ComplaintType { get; set; }
    public string Description { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; }
    public ComplaintPriority Priority { get; set; }
    public DateTime ResponseDueDate { get; set; }
    public DateTime? ResponseDate { get; set; }
    public string? ResponseNotes { get; set; }
    public bool IsEscalatedToProConsumidor { get; set; }
    public string? ProConsumidorCaseNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<ComplaintEvidence> Evidences { get; set; } = new List<ComplaintEvidence>();
    public ICollection<Mediation> Mediations { get; set; } = new List<Mediation>();
}

/// <summary>
/// Evidencia de reclamación
/// </summary>
public class ComplaintEvidence
{
    public Guid Id { get; set; }
    public Guid ComplaintId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
    
    // Navigation
    public Complaint? Complaint { get; set; }
}

/// <summary>
/// Mediación entre consumidor y vendedor
/// </summary>
public class Mediation
{
    public Guid Id { get; set; }
    public Guid ComplaintId { get; set; }
    public string MediationNumber { get; set; } = string.Empty;
    public MediationStatus Status { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? MediatorName { get; set; }
    public string? Location { get; set; }
    public string? AgreementSummary { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Complaint? Complaint { get; set; }
}

/// <summary>
/// Consumidor registrado
/// </summary>
public class Consumer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool AcceptsNotifications { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
}

/// <summary>
/// Información de producto para cumplimiento
/// </summary>
public class ProductDisclosure
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool PriceIncludesITBIS { get; set; }
    public int WarrantyMonths { get; set; }
    public bool IsNew { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? Specifications { get; set; }
    public string? Warnings { get; set; }
    public bool MeetsDisclosureRequirements { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
}

// =====================================================
// ConsumerProtectionService - DTOs
// Ley 358-05 Derechos del Consumidor de RD
// =====================================================

namespace ConsumerProtectionService.Application.DTOs;

public record WarrantyDto(
    Guid Id,
    Guid ProductId,
    Guid SellerId,
    Guid? ConsumerId,
    string WarrantyNumber,
    string WarrantyType,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    string? CoverageDescription,
    string? Exclusions,
    decimal? PurchasePrice,
    DateTime CreatedAt
);

public record CreateWarrantyDto(
    Guid ProductId,
    Guid SellerId,
    Guid? ConsumerId,
    string WarrantyType,
    DateTime StartDate,
    DateTime EndDate,
    string? CoverageDescription,
    string? Exclusions,
    decimal? PurchasePrice
);

public record WarrantyClaimDto(
    Guid Id,
    Guid WarrantyId,
    Guid ConsumerId,
    string ClaimNumber,
    string IssueDescription,
    DateTime ClaimDate,
    string Status,
    string? Resolution,
    string? ResolutionNotes,
    DateTime? ResolvedAt,
    DateTime CreatedAt
);

public record CreateWarrantyClaimDto(
    Guid WarrantyId,
    Guid ConsumerId,
    string IssueDescription
);

public record ComplaintDto(
    Guid Id,
    Guid ConsumerId,
    Guid? SellerId,
    Guid? ProductId,
    string ComplaintNumber,
    string ComplaintType,
    string Description,
    string Status,
    string Priority,
    DateTime ResponseDueDate,
    DateTime? ResponseDate,
    string? ResponseNotes,
    bool IsEscalatedToProConsumidor,
    string? ProConsumidorCaseNumber,
    DateTime CreatedAt
);

public record CreateComplaintDto(
    Guid ConsumerId,
    Guid? SellerId,
    Guid? ProductId,
    string ComplaintType,
    string Description,
    string Priority
);

public record ComplaintEvidenceDto(
    Guid Id,
    Guid ComplaintId,
    string FileName,
    string FileType,
    string FilePath,
    string? Description,
    DateTime UploadedAt
);

public record MediationDto(
    Guid Id,
    Guid ComplaintId,
    DateTime ScheduledDate,
    string? Location,
    string Status,
    string? MediatorName,
    string? AgreementSummary,
    string? Outcome,
    DateTime CreatedAt
);

public record CreateMediationDto(
    Guid ComplaintId,
    DateTime ScheduledDate,
    string? Location,
    string? MediatorName
);

public record ConsumerProtectionStatisticsDto(
    int TotalWarranties,
    int ActiveWarranties,
    int TotalClaims,
    int PendingClaims,
    int TotalComplaints,
    int PendingComplaints,
    int EscalatedComplaints,
    decimal AverageResolutionDays,
    DateTime GeneratedAt
);

public record ResolveClaimDto(
    string Resolution,
    string ResolutionNotes
);

public record RespondToComplaintDto(
    string ResponseNotes
);

public record EscalateToProConsumidorDto(
    string Reason
);

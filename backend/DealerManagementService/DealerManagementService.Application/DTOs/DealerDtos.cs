using DealerManagementService.Domain.Entities;

namespace DealerManagementService.Application.DTOs;

public record DealerDto(
    Guid Id,
    Guid UserId,
    string BusinessName,
    string RNC,
    string? LegalName,
    string? TradeName,
    string Type, // DealerType as string
    string Status, // DealerStatus as string
    string VerificationStatus, // VerificationStatus as string
    string Email,
    string Phone,
    string? MobilePhone,
    string? Website,
    string Address,
    string City,
    string Province,
    string? ZipCode,
    string Country,
    string? Description,
    string? LogoUrl,
    string? BannerUrl,
    DateTime? EstablishedDate,
    int? EmployeeCount,
    string CurrentPlan, // DealerPlan as string
    DateTime? SubscriptionStartDate,
    DateTime? SubscriptionEndDate,
    bool IsSubscriptionActive,
    int MaxActiveListings,
    int CurrentActiveListings,
    int RemainingListings,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? VerifiedAt,
    List<DealerDocumentDto>? Documents,
    List<DealerLocationDto>? Locations
);

public record DealerDocumentDto(
    Guid Id,
    Guid DealerId,
    string Type, // DocumentType as string
    string FileName,
    string FileUrl,
    long FileSizeBytes,
    string MimeType,
    string VerificationStatus, // DocumentVerificationStatus as string
    DateTime? VerifiedAt,
    string? RejectionReason,
    DateTime? ExpiryDate,
    bool IsExpired,
    DateTime UploadedAt
);

public record DealerLocationDto(
    Guid Id,
    Guid DealerId,
    string Name,
    string Type, // LocationType as string
    bool IsPrimary,
    string Address,
    string City,
    string Province,
    string? ZipCode,
    string Country,
    double? Latitude,
    double? Longitude,
    string Phone,
    string? Email,
    string? WorkingHours,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateDealerRequest(
    Guid UserId,
    string BusinessName,
    string RNC,
    string? LegalName,
    string? TradeName,
    string Type,
    string Email,
    string Phone,
    string? MobilePhone,
    string? Website,
    string Address,
    string City,
    string Province,
    string? ZipCode,
    string? Description,
    DateTime? EstablishedDate,
    int? EmployeeCount
);

public record UpdateDealerRequest(
    string? BusinessName,
    string? LegalName,
    string? TradeName,
    string? Email,
    string? Phone,
    string? MobilePhone,
    string? Website,
    string? Address,
    string? City,
    string? Province,
    string? ZipCode,
    string? Description,
    string? LogoUrl,
    string? BannerUrl,
    DateTime? EstablishedDate,
    int? EmployeeCount
);

public record UploadDocumentRequest(
    Guid DealerId,
    string Type, // DocumentType as string
    string FileName,
    string FileUrl,
    string FileKey,
    long FileSizeBytes,
    string MimeType,
    DateTime? ExpiryDate
);

public record VerifyDealerRequest(
    Guid DealerId,
    bool Approved,
    string? RejectionReason
);

public record VerifyDocumentRequest(
    Guid DocumentId,
    bool Approved,
    string? RejectionReason,
    string? Notes
);

public record AddLocationRequest(
    Guid DealerId,
    string Name,
    string Type, // LocationType as string
    bool IsPrimary,
    string Address,
    string City,
    string Province,
    string? ZipCode,
    double? Latitude,
    double? Longitude,
    string Phone,
    string? Email,
    string? WorkingHours
);

public record UpdateSubscriptionRequest(
    Guid DealerId,
    Guid SubscriptionId,
    string Plan, // DealerPlan as string
    DateTime StartDate,
    DateTime EndDate
);

public record DealerListResponse(
    List<DealerDto> Dealers,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

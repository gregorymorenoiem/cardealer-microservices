namespace MarketingService.Application.DTOs;

public record AudienceDto(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string? FilterCriteria,
    int MemberCount,
    bool IsActive,
    DateTime? LastSyncedAt,
    DateTime CreatedAt
);

public record CreateAudienceRequest(
    string Name,
    string Type,
    string? Description = null,
    string? FilterCriteria = null
);

public record UpdateAudienceRequest(
    string Name,
    string? Description,
    string? FilterCriteria = null
);

public record AudienceMemberDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? Phone,
    Guid? CustomerId,
    Guid? LeadId,
    bool IsSubscribed,
    DateTime AddedAt
);

public record AddAudienceMemberRequest(
    string Email,
    string? FirstName = null,
    string? LastName = null,
    string? Phone = null
);

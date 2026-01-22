using MediatR;
using DataProtectionService.Application.DTOs;

namespace DataProtectionService.Application.Queries;

public record GetUserConsentsQuery(Guid UserId, bool? ActiveOnly = true) : IRequest<List<UserConsentDto>>;

public record GetConsentByIdQuery(Guid ConsentId) : IRequest<UserConsentDto?>;

public record GetConsentsByTypeQuery(Guid UserId, string Type) : IRequest<List<UserConsentDto>>;

public record CheckConsentQuery(Guid UserId, string Type) : IRequest<bool>;

public record GetUsersRequiringReConsentQuery(string PolicyVersion) : IRequest<List<Guid>>;

public record GetConsentHistoryQuery(
    Guid UserId,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedResult<UserConsentDto>>;

public record GetCurrentPrivacyPolicyQuery(string Language = "es") : IRequest<PrivacyPolicyDto?>;

public record GetAllPrivacyPoliciesQuery(bool? ActiveOnly = true) : IRequest<List<PrivacyPolicyDto>>;

public record PaginatedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

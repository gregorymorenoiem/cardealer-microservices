using MediatR;

namespace CacheService.Application.Queries;

public record GetCacheQuery(
    string Key,
    string? TenantId = null
) : IRequest<string?>;

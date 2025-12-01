using MediatR;

namespace CacheService.Application.Commands;

public record SetCacheCommand(
    string Key,
    string Value,
    string? TenantId = null,
    int? TtlSeconds = null
) : IRequest<bool>;

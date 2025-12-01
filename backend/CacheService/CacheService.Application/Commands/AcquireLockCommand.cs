using MediatR;
using CacheService.Domain;

namespace CacheService.Application.Commands;

public record AcquireLockCommand(
    string Key,
    string OwnerId,
    int TtlSeconds = 30
) : IRequest<CacheLock?>;

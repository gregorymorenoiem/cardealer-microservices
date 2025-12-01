using MediatR;
using CacheService.Application.Commands;
using CacheService.Application.Interfaces;
using CacheService.Domain;

namespace CacheService.Application.Handlers;

public class AcquireLockCommandHandler : IRequestHandler<AcquireLockCommand, CacheLock?>
{
    private readonly IDistributedLockManager _lockManager;

    public AcquireLockCommandHandler(IDistributedLockManager lockManager)
    {
        _lockManager = lockManager;
    }

    public async Task<CacheLock?> Handle(AcquireLockCommand request, CancellationToken cancellationToken)
    {
        var ttl = TimeSpan.FromSeconds(request.TtlSeconds);
        return await _lockManager.AcquireLockAsync(request.Key, request.OwnerId, ttl, cancellationToken);
    }
}

using MediatR;
using CacheService.Application.Commands;
using CacheService.Application.Interfaces;

namespace CacheService.Application.Handlers;

public class ReleaseLockCommandHandler : IRequestHandler<ReleaseLockCommand, bool>
{
    private readonly IDistributedLockManager _lockManager;

    public ReleaseLockCommandHandler(IDistributedLockManager lockManager)
    {
        _lockManager = lockManager;
    }

    public async Task<bool> Handle(ReleaseLockCommand request, CancellationToken cancellationToken)
    {
        return await _lockManager.ReleaseLockAsync(request.Key, request.OwnerId, cancellationToken);
    }
}

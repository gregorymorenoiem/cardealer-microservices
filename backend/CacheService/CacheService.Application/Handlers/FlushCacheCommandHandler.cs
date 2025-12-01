using MediatR;
using CacheService.Application.Commands;
using CacheService.Application.Interfaces;

namespace CacheService.Application.Handlers;

public class FlushCacheCommandHandler : IRequestHandler<FlushCacheCommand, Unit>
{
    private readonly ICacheManager _cacheManager;

    public FlushCacheCommandHandler(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task<Unit> Handle(FlushCacheCommand request, CancellationToken cancellationToken)
    {
        await _cacheManager.FlushAllAsync(cancellationToken);
        return Unit.Value;
    }
}

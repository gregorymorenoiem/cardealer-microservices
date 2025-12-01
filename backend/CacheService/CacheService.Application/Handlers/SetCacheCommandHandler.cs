using MediatR;
using CacheService.Application.Commands;
using CacheService.Application.Interfaces;

namespace CacheService.Application.Handlers;

public class SetCacheCommandHandler : IRequestHandler<SetCacheCommand, bool>
{
    private readonly ICacheManager _cacheManager;

    public SetCacheCommandHandler(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task<bool> Handle(SetCacheCommand request, CancellationToken cancellationToken)
    {
        TimeSpan? ttl = request.TtlSeconds.HasValue 
            ? TimeSpan.FromSeconds(request.TtlSeconds.Value) 
            : null;

        if (!string.IsNullOrEmpty(request.TenantId))
        {
            return await _cacheManager.SetAsync(request.Key, request.Value, request.TenantId, ttl, cancellationToken);
        }

        return await _cacheManager.SetAsync(request.Key, request.Value, ttl, cancellationToken);
    }
}

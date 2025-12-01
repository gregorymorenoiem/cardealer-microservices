using MediatR;
using CacheService.Application.Queries;
using CacheService.Application.Interfaces;

namespace CacheService.Application.Handlers;

public class GetCacheQueryHandler : IRequestHandler<GetCacheQuery, string?>
{
    private readonly ICacheManager _cacheManager;

    public GetCacheQueryHandler(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task<string?> Handle(GetCacheQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.TenantId))
        {
            return await _cacheManager.GetAsync(request.Key, request.TenantId, cancellationToken);
        }

        return await _cacheManager.GetAsync(request.Key, cancellationToken);
    }
}

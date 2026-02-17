using MediatR;
using CacheService.Application.Commands;
using CacheService.Application.Interfaces;
using CarDealer.Shared.Configuration;

namespace CacheService.Application.Handlers;

public class SetCacheCommandHandler : IRequestHandler<SetCacheCommand, bool>
{
    private readonly ICacheManager _cacheManager;
    private readonly IConfigurationServiceClient _configClient;

    public SetCacheCommandHandler(ICacheManager cacheManager, IConfigurationServiceClient configClient)
    {
        _cacheManager = cacheManager;
        _configClient = configClient;
    }

    public async Task<bool> Handle(SetCacheCommand request, CancellationToken cancellationToken)
    {
        TimeSpan? ttl;
        if (request.TtlSeconds.HasValue)
        {
            ttl = TimeSpan.FromSeconds(request.TtlSeconds.Value);
        }
        else
        {
            // Use dynamic default from admin panel (cache.default_expiration_minutes)
            var defaultMinutes = await _configClient.GetIntAsync("cache.default_expiration_minutes", 30, cancellationToken);
            ttl = TimeSpan.FromMinutes(defaultMinutes);
        }

        if (!string.IsNullOrEmpty(request.TenantId))
        {
            return await _cacheManager.SetAsync(request.Key, request.Value, request.TenantId, ttl, cancellationToken);
        }

        return await _cacheManager.SetAsync(request.Key, request.Value, ttl, cancellationToken);
    }
}

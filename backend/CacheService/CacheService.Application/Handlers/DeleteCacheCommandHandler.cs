using MediatR;
using CacheService.Application.Commands;
using CacheService.Application.Interfaces;

namespace CacheService.Application.Handlers;

public class DeleteCacheCommandHandler : IRequestHandler<DeleteCacheCommand, bool>
{
    private readonly ICacheManager _cacheManager;

    public DeleteCacheCommandHandler(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task<bool> Handle(DeleteCacheCommand request, CancellationToken cancellationToken)
    {
        return await _cacheManager.DeleteAsync(request.Key, cancellationToken);
    }
}

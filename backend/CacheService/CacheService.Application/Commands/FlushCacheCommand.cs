using MediatR;

namespace CacheService.Application.Commands;

public record FlushCacheCommand : IRequest<Unit>;

using MediatR;

namespace CacheService.Application.Commands;

public record DeleteCacheCommand(string Key) : IRequest<bool>;

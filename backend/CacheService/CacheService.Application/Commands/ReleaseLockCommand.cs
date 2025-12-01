using MediatR;

namespace CacheService.Application.Commands;

public record ReleaseLockCommand(
    string Key,
    string OwnerId
) : IRequest<bool>;

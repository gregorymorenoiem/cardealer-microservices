using MediatR;

namespace AuthService.Application.Features.ExternalAuth.Commands.UnlinkExternalAccount;

public record UnlinkExternalAccountCommand(string UserId, string Provider) : IRequest<Unit>;
using MediatR;
using ModerationAgent.Application.DTOs;

namespace ModerationAgent.Application.Features.Moderation.Commands;

public sealed record ModerateContentCommand(ModerationRequest Request) : IRequest<ModerationResponse>;

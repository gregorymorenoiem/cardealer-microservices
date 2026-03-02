using MediatR;
using SearchAgent.Domain.Entities;

namespace SearchAgent.Application.Features.Config.Queries;

public record GetSearchAgentConfigQuery : IRequest<SearchAgentConfig>;

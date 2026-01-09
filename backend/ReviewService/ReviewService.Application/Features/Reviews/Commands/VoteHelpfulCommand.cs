using MediatR;
using ReviewService.Domain.Base;

namespace ReviewService.Application.Features.Reviews.Commands;

/// <summary>
/// Sprint 15 - Votar si una review es útil o no
/// Un usuario solo puede votar una vez por review
/// </summary>
public record VoteHelpfulCommand : IRequest<Result<VoteResultDto>>
{
    public Guid ReviewId { get; init; }
    public Guid UserId { get; init; }
    public bool IsHelpful { get; init; }
    public string? UserIpAddress { get; init; }
    public string? UserAgent { get; init; }
}

/// <summary>
/// Resultado del voto
/// </summary>
public record VoteResultDto
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int TotalHelpfulVotes { get; init; }
    public int TotalVotes { get; init; }
    public decimal HelpfulPercentage { get; init; }
    public bool UserVotedHelpful { get; init; }
}

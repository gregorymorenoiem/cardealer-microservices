using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Queries.ListExperiments;

public class ListExperimentsQuery : IRequest<ListExperimentsResponse>
{
    public ExperimentStatus? Status { get; set; }
    public Guid? FeatureFlagId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class ListExperimentsResponse
{
    public List<ExperimentSummaryDto> Experiments { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class ExperimentSummaryDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int VariantCount { get; set; }
    public int ParticipantCount { get; set; }
    public bool HasStatisticalSignificance { get; set; }
    public Guid? WinningVariantId { get; set; }
}

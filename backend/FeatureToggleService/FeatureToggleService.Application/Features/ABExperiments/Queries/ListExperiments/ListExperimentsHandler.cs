using FeatureToggleService.Application.Interfaces;
using FeatureToggleService.Domain.Enums;
using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Queries.ListExperiments;

public class ListExperimentsHandler : IRequestHandler<ListExperimentsQuery, ListExperimentsResponse>
{
    private readonly IABTestingService _abTestingService;

    public ListExperimentsHandler(IABTestingService abTestingService)
    {
        _abTestingService = abTestingService;
    }

    public async Task<ListExperimentsResponse> Handle(ListExperimentsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.ABExperiment> experiments;

        if (request.Status.HasValue)
        {
            experiments = await _abTestingService.GetByStatusAsync(request.Status.Value, cancellationToken);
        }
        else if (request.FeatureFlagId.HasValue)
        {
            experiments = await _abTestingService.GetByFeatureFlagAsync(request.FeatureFlagId.Value, cancellationToken);
        }
        else
        {
            experiments = await _abTestingService.GetAllExperimentsAsync(cancellationToken);
        }

        var experimentList = experiments.ToList();
        var totalCount = experimentList.Count;

        var pagedExperiments = experimentList
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(exp => new ExperimentSummaryDto
            {
                Id = exp.Id,
                Key = exp.Key,
                Name = exp.Name,
                Status = exp.Status.ToString(),
                CreatedAt = exp.CreatedAt,
                StartedAt = exp.StartedAt,
                CompletedAt = exp.CompletedAt,
                VariantCount = exp.Variants.Count,
                ParticipantCount = exp.Assignments.Count,
                HasStatisticalSignificance = exp.Status == ExperimentStatus.Running
                    && exp.HasReachedMinSampleSize(),
                WinningVariantId = exp.WinningVariantId
            })
            .ToList();

        return new ListExperimentsResponse
        {
            Experiments = pagedExperiments,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

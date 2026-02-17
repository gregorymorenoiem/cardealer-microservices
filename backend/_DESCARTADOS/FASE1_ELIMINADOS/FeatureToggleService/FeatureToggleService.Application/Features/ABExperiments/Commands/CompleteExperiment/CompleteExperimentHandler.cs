using FeatureToggleService.Application.Interfaces;
using FeatureToggleService.Domain.Enums;
using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Commands.CompleteExperiment;

public class CompleteExperimentHandler : IRequestHandler<CompleteExperimentCommand, CompleteExperimentResponse>
{
    private readonly IABTestingService _abTestingService;

    public CompleteExperimentHandler(IABTestingService abTestingService)
    {
        _abTestingService = abTestingService;
    }

    public async Task<CompleteExperimentResponse> Handle(CompleteExperimentCommand request, CancellationToken cancellationToken)
    {
        var experiment = await _abTestingService.GetExperimentAsync(request.ExperimentId, cancellationToken);

        if (experiment == null)
        {
            throw new KeyNotFoundException($"Experiment with ID {request.ExperimentId} not found");
        }

        if (experiment.Status != ExperimentStatus.Running)
        {
            throw new InvalidOperationException($"Cannot complete experiment in {experiment.Status} status");
        }

        if (request.WinningVariantId.HasValue)
        {
            var winningVariant = experiment.Variants.FirstOrDefault(v => v.Id == request.WinningVariantId.Value);
            if (winningVariant == null)
            {
                throw new ArgumentException($"Variant {request.WinningVariantId.Value} does not belong to this experiment");
            }
        }

        await _abTestingService.CompleteExperimentAsync(
            request.ExperimentId,
            request.WinningVariantId,
            request.CompletionNotes,
            "system",
            cancellationToken
        );

        // Refresh to get updated data
        experiment = await _abTestingService.GetExperimentAsync(request.ExperimentId, cancellationToken);

        var winningVariantKey = request.WinningVariantId.HasValue
            ? experiment?.Variants.FirstOrDefault(v => v.Id == request.WinningVariantId.Value)?.Key
            : null;

        return new CompleteExperimentResponse
        {
            Id = experiment!.Id,
            Key = experiment.Key,
            Status = experiment.Status.ToString(),
            CompletedAt = experiment.CompletedAt ?? DateTime.UtcNow,
            WinningVariantId = experiment.WinningVariantId,
            WinningVariantKey = winningVariantKey,
            WinnerConfidence = experiment.WinnerConfidence,
            Message = experiment.WinningVariantId.HasValue
                ? $"Experiment completed with winner: {winningVariantKey}"
                : "Experiment completed without selecting a winner"
        };
    }
}

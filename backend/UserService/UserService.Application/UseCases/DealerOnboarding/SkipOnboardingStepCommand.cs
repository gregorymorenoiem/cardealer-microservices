using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;
using System.Text.Json;

namespace UserService.Application.UseCases.DealerOnboarding;

/// <summary>
/// Command to skip an onboarding step
/// </summary>
public record SkipOnboardingStepCommand(Guid DealerId, string Step) : IRequest<Unit>;

public class SkipOnboardingStepCommandHandler : IRequestHandler<SkipOnboardingStepCommand, Unit>
{
    private readonly IDealerOnboardingRepository _onboardingRepository;
    private readonly ILogger<SkipOnboardingStepCommandHandler> _logger;

    public SkipOnboardingStepCommandHandler(
        IDealerOnboardingRepository onboardingRepository,
        ILogger<SkipOnboardingStepCommandHandler> logger)
    {
        _onboardingRepository = onboardingRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(SkipOnboardingStepCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Skipping step {Step} for dealer {DealerId}", request.Step, request.DealerId);

        var onboarding = await _onboardingRepository.GetByDealerIdAsync(request.DealerId);

        if (onboarding == null)
        {
            throw new NotFoundException($"Onboarding process not found for dealer {request.DealerId}");
        }

        if (!Enum.TryParse<DealerOnboardingStep>(request.Step, out var step))
        {
            throw new BadRequestException($"Invalid step: {request.Step}");
        }

        // Add to skipped steps
        var stepsSkipped = JsonSerializer.Deserialize<List<string>>(onboarding.StepsSkipped) ?? new List<string>();
        if (!stepsSkipped.Contains(request.Step))
        {
            stepsSkipped.Add(request.Step);
        }

        onboarding.StepsSkipped = JsonSerializer.Serialize(stepsSkipped);
        onboarding.CurrentStep = GetNextStep(step);

        await _onboardingRepository.UpdateAsync(onboarding);

        _logger.LogInformation("Step {Step} skipped for dealer {DealerId}", request.Step, request.DealerId);

        return Unit.Value;
    }

    private DealerOnboardingStep GetNextStep(DealerOnboardingStep currentStep)
    {
        return currentStep switch
        {
            DealerOnboardingStep.BasicInfo => DealerOnboardingStep.Documents,
            DealerOnboardingStep.Documents => DealerOnboardingStep.BillingInfo,
            DealerOnboardingStep.BillingInfo => DealerOnboardingStep.Features,
            DealerOnboardingStep.Features => DealerOnboardingStep.Completed,
            _ => DealerOnboardingStep.Completed
        };
    }
}

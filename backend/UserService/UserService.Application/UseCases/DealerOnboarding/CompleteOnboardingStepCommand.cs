using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;
using System.Text.Json;

namespace UserService.Application.UseCases.DealerOnboarding;

/// <summary>
/// Command to complete an onboarding step
/// </summary>
public record CompleteOnboardingStepCommand(
    Guid DealerId,
    string Step,
    Dictionary<string, object>? Data = null
) : IRequest<Unit>;

public class CompleteOnboardingStepCommandHandler : IRequestHandler<CompleteOnboardingStepCommand, Unit>
{
    private readonly IDealerOnboardingRepository _onboardingRepository;
    private readonly IDealerRepository _dealerRepository;
    private readonly ILogger<CompleteOnboardingStepCommandHandler> _logger;

    public CompleteOnboardingStepCommandHandler(
        IDealerOnboardingRepository onboardingRepository,
        IDealerRepository dealerRepository,
        ILogger<CompleteOnboardingStepCommandHandler> logger)
    {
        _onboardingRepository = onboardingRepository;
        _dealerRepository = dealerRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(CompleteOnboardingStepCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Completing step {Step} for dealer {DealerId}", request.Step, request.DealerId);

        var onboarding = await _onboardingRepository.GetByDealerIdAsync(request.DealerId);

        if (onboarding == null)
        {
            throw new NotFoundException($"Onboarding process not found for dealer {request.DealerId}");
        }

        if (!Enum.TryParse<DealerOnboardingStep>(request.Step, out var step))
        {
            throw new BadRequestException($"Invalid step: {request.Step}");
        }

        // Add to completed steps
        var stepsCompleted = JsonSerializer.Deserialize<List<string>>(onboarding.StepsCompleted) ?? new List<string>();
        if (!stepsCompleted.Contains(request.Step))
        {
            stepsCompleted.Add(request.Step);
        }

        onboarding.StepsCompleted = JsonSerializer.Serialize(stepsCompleted);
        onboarding.CurrentStep = GetNextStep(step);

        // Update dealer with step data if provided
        if (request.Data != null)
        {
            var dealer = await _dealerRepository.GetByIdAsync(request.DealerId);
            if (dealer != null)
            {
                UpdateDealerFromStepData(dealer, step, request.Data);
                await _dealerRepository.UpdateAsync(dealer);
            }
        }

        // Check if onboarding is complete
        var totalSteps = Enum.GetValues<DealerOnboardingStep>().Length;
        if (stepsCompleted.Count >= totalSteps)
        {
            onboarding.Status = OnboardingStatus.Completed;
            onboarding.CompletedAt = DateTime.UtcNow;
        }

        await _onboardingRepository.UpdateAsync(onboarding);

        _logger.LogInformation("Step {Step} completed for dealer {DealerId}", request.Step, request.DealerId);

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

    private void UpdateDealerFromStepData(Dealer dealer, DealerOnboardingStep step, Dictionary<string, object> data)
    {
        // Update dealer properties based on step data
        // This is a placeholder - implement based on your business logic
        switch (step)
        {
            case DealerOnboardingStep.BasicInfo:
                if (data.ContainsKey("BusinessName"))
                    dealer.BusinessName = data["BusinessName"]?.ToString() ?? dealer.BusinessName;
                if (data.ContainsKey("Phone"))
                    dealer.Phone = data["Phone"]?.ToString() ?? string.Empty;
                break;
            case DealerOnboardingStep.Documents:
                // Handle document uploads
                break;
            case DealerOnboardingStep.BillingInfo:
                // Handle billing information
                break;
            case DealerOnboardingStep.Features:
                // Handle feature selection
                break;
        }
    }
}

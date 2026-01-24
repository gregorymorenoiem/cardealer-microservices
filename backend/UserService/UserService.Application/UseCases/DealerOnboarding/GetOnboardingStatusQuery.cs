using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;
using System.Text.Json;

namespace UserService.Application.UseCases.DealerOnboarding;

/// <summary>
/// Query to get dealer onboarding status
/// </summary>
public record GetOnboardingStatusQuery(Guid DealerId) : IRequest<DealerOnboardingDto>;

public class GetOnboardingStatusQueryHandler : IRequestHandler<GetOnboardingStatusQuery, DealerOnboardingDto>
{
    private readonly IDealerOnboardingRepository _onboardingRepository;
    private readonly ILogger<GetOnboardingStatusQueryHandler> _logger;

    public GetOnboardingStatusQueryHandler(
        IDealerOnboardingRepository onboardingRepository,
        ILogger<GetOnboardingStatusQueryHandler> logger)
    {
        _onboardingRepository = onboardingRepository;
        _logger = logger;
    }

    public async Task<DealerOnboardingDto> Handle(GetOnboardingStatusQuery request, CancellationToken cancellationToken)
    {
        var onboarding = await _onboardingRepository.GetByDealerIdAsync(request.DealerId);

        if (onboarding == null)
        {
            throw new NotFoundException($"Onboarding process not found for dealer {request.DealerId}");
        }

        var stepsCompleted = JsonSerializer.Deserialize<List<string>>(onboarding.StepsCompleted) ?? new List<string>();
        var totalSteps = Enum.GetValues<Domain.Entities.DealerOnboardingStep>().Length;
        var progress = (int)((double)stepsCompleted.Count / totalSteps * 100);

        return new DealerOnboardingDto
        {
            DealerId = onboarding.DealerId,
            Status = onboarding.Status.ToString(),
            CurrentStep = onboarding.CurrentStep.ToString(),
            StepsCompleted = stepsCompleted,
            Progress = progress,
            StartedAt = onboarding.StartedAt,
            CompletedAt = onboarding.CompletedAt
        };
    }
}

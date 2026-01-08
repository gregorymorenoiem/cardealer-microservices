using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using System.Security.Claims;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OnboardingController : ControllerBase
{
    private readonly IUserOnboardingRepository _repository;
    private readonly ILogger<OnboardingController> _logger;

    public OnboardingController(
        IUserOnboardingRepository repository,
        ILogger<OnboardingController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el estado de onboarding del usuario actual
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<OnboardingStatusDto>> GetStatus()
    {
        var userId = GetCurrentUserId();
        var onboarding = await _repository.GetByUserIdAsync(userId);

        if (onboarding == null)
        {
            // Crear onboarding automáticamente si no existe
            onboarding = new UserOnboarding(userId);
            await _repository.CreateAsync(onboarding);

            _logger.LogInformation("Created onboarding for user {UserId}", userId);
        }

        return Ok(MapToDto(onboarding));
    }

    /// <summary>
    /// Marca un step como completado
    /// </summary>
    [HttpPost("complete-step")]
    public async Task<ActionResult<OnboardingStatusDto>> CompleteStep(
        [FromBody] CompleteStepRequest request)
    {
        var userId = GetCurrentUserId();
        var onboarding = await _repository.GetByUserIdAsync(userId);

        if (onboarding == null)
        {
            return NotFound(new { error = "Onboarding not found" });
        }

        onboarding.CompleteStep(request.Step);
        await _repository.UpdateAsync(onboarding);

        _logger.LogInformation(
            "User {UserId} completed onboarding step {Step}",
            userId, request.Step);

        return Ok(MapToDto(onboarding));
    }

    /// <summary>
    /// Completa el onboarding manualmente
    /// </summary>
    [HttpPost("complete")]
    public async Task<ActionResult<OnboardingStatusDto>> Complete()
    {
        var userId = GetCurrentUserId();
        var onboarding = await _repository.GetByUserIdAsync(userId);

        if (onboarding == null)
        {
            return NotFound(new { error = "Onboarding not found" });
        }

        onboarding.CompleteOnboarding();
        await _repository.UpdateAsync(onboarding);

        _logger.LogInformation("User {UserId} completed onboarding", userId);

        return Ok(MapToDto(onboarding));
    }

    /// <summary>
    /// Permite al usuario saltar el onboarding
    /// </summary>
    [HttpPost("skip")]
    public async Task<ActionResult<OnboardingStatusDto>> Skip()
    {
        var userId = GetCurrentUserId();
        var onboarding = await _repository.GetByUserIdAsync(userId);

        if (onboarding == null)
        {
            return NotFound(new { error = "Onboarding not found" });
        }

        onboarding.Skip();
        await _repository.UpdateAsync(onboarding);

        _logger.LogInformation("User {UserId} skipped onboarding", userId);

        return Ok(MapToDto(onboarding));
    }

    /// <summary>
    /// Obtiene estadísticas de onboarding (Admin only)
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OnboardingStatsDto>> GetStats()
    {
        var completionRate = await _repository.GetCompletionRateAsync();
        var incomplete = await _repository.GetIncompleteAsync();

        return Ok(new OnboardingStatsDto
        {
            CompletionRate = completionRate,
            IncompleteCount = incomplete.Count
        });
    }

    // Helper methods
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        return userId;
    }

    private static OnboardingStatusDto MapToDto(UserOnboarding onboarding)
    {
        return new OnboardingStatusDto
        {
            IsCompleted = onboarding.IsCompleted,
            WasSkipped = onboarding.WasSkipped,
            ProgressPercentage = onboarding.GetProgressPercentage(),
            NextStep = onboarding.GetNextStep(),
            Steps = new OnboardingStepsDto
            {
                Profile = new StepStatusDto
                {
                    IsCompleted = onboarding.StepProfileCompleted,
                    CompletedAt = onboarding.StepProfileCompletedAt
                },
                Preferences = new StepStatusDto
                {
                    IsCompleted = onboarding.StepPreferencesCompleted,
                    CompletedAt = onboarding.StepPreferencesCompletedAt
                },
                FirstSearch = new StepStatusDto
                {
                    IsCompleted = onboarding.StepFirstSearchCompleted,
                    CompletedAt = onboarding.StepFirstSearchCompletedAt
                },
                Tour = new StepStatusDto
                {
                    IsCompleted = onboarding.StepTourCompleted,
                    CompletedAt = onboarding.StepTourCompletedAt
                }
            },
            CompletedAt = onboarding.CompletedAt,
            SkippedAt = onboarding.SkippedAt
        };
    }
}

#region DTOs

public record CompleteStepRequest
{
    public OnboardingStep Step { get; init; }
}

public record OnboardingStatusDto
{
    public bool IsCompleted { get; init; }
    public bool WasSkipped { get; init; }
    public int ProgressPercentage { get; init; }
    public OnboardingStep? NextStep { get; init; }
    public OnboardingStepsDto Steps { get; init; } = new();
    public DateTime? CompletedAt { get; init; }
    public DateTime? SkippedAt { get; init; }
}

public record OnboardingStepsDto
{
    public StepStatusDto Profile { get; init; } = new();
    public StepStatusDto Preferences { get; init; } = new();
    public StepStatusDto FirstSearch { get; init; } = new();
    public StepStatusDto Tour { get; init; } = new();
}

public record StepStatusDto
{
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public record OnboardingStatsDto
{
    public int CompletionRate { get; init; }
    public int IncompleteCount { get; init; }
}

#endregion

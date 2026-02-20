using FluentValidation;

namespace AdvertisingService.Application.Features.Rotation.Commands.UpdateRotationConfig;

public class UpdateRotationConfigCommandValidator : AbstractValidator<UpdateRotationConfigCommand>
{
    public UpdateRotationConfigCommandValidator()
    {
        RuleFor(x => x.RefreshIntervalMinutes)
            .GreaterThanOrEqualTo(5)
            .When(x => x.RefreshIntervalMinutes.HasValue)
            .WithMessage("Refresh interval must be at least 5 minutes");

        RuleFor(x => x.MaxVehiclesShown)
            .InclusiveBetween(1, 50)
            .When(x => x.MaxVehiclesShown.HasValue)
            .WithMessage("Max vehicles shown must be between 1 and 50");

        RuleFor(x => x.WeightRemainingBudget)
            .InclusiveBetween(0, 1)
            .When(x => x.WeightRemainingBudget.HasValue)
            .WithMessage("Weight must be between 0 and 1 (0% to 100%)");

        RuleFor(x => x.WeightCtr)
            .InclusiveBetween(0, 1)
            .When(x => x.WeightCtr.HasValue)
            .WithMessage("Weight must be between 0 and 1 (0% to 100%)");

        RuleFor(x => x.WeightQualityScore)
            .InclusiveBetween(0, 1)
            .When(x => x.WeightQualityScore.HasValue)
            .WithMessage("Weight must be between 0 and 1 (0% to 100%)");

        RuleFor(x => x.WeightRecency)
            .InclusiveBetween(0, 1)
            .When(x => x.WeightRecency.HasValue)
            .WithMessage("Weight must be between 0 and 1 (0% to 100%)");

        // When ALL four weights are provided, they must sum to 1.0 (100%)
        RuleFor(x => x)
            .Must(cmd =>
            {
                // Only validate sum when all four weights are explicitly provided
                if (!cmd.WeightRemainingBudget.HasValue &&
                    !cmd.WeightCtr.HasValue &&
                    !cmd.WeightQualityScore.HasValue &&
                    !cmd.WeightRecency.HasValue)
                    return true; // No weights being updated

                // If any weight is provided, we need to validate the complete set
                // For partial updates, accept as-is (handler will merge with existing)
                if (!cmd.WeightRemainingBudget.HasValue ||
                    !cmd.WeightCtr.HasValue ||
                    !cmd.WeightQualityScore.HasValue ||
                    !cmd.WeightRecency.HasValue)
                    return true; // Partial update — can't validate sum

                var sum = cmd.WeightRemainingBudget.Value +
                          cmd.WeightCtr.Value +
                          cmd.WeightQualityScore.Value +
                          cmd.WeightRecency.Value;

                // Allow small floating-point tolerance
                return Math.Abs(sum - 1.0m) < 0.01m;
            })
            .WithMessage("When providing all four weights, they must sum to 100% (1.0). " +
                         "Current sum: {PropertyValue}")
            .WithName("WeightSum");
    }
}

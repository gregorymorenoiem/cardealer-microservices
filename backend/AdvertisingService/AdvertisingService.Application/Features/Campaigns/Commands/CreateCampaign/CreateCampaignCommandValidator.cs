using AdvertisingService.Application.Validators;
using FluentValidation;

namespace AdvertisingService.Application.Features.Campaigns.Commands.CreateCampaign;

public class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId is required");
        RuleFor(x => x.OwnerType)
            .NotEmpty()
            .Must(t => t == "Individual" || t == "Dealer")
            .WithMessage("OwnerType must be 'Individual' or 'Dealer'")
            .NoSqlInjection()
            .NoXss();
        RuleFor(x => x.VehicleId).NotEmpty().WithMessage("VehicleId is required");
        RuleFor(x => x.PlacementType).IsInEnum();
        RuleFor(x => x.PricingModel).IsInEnum();
        RuleFor(x => x.TotalBudget).GreaterThan(0).WithMessage("Budget must be greater than 0");
        RuleFor(x => x.StartDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Start date must be today or in the future");
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");
    }
}

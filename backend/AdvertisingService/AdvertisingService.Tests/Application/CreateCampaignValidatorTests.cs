using AdvertisingService.Application.Features.Campaigns.Commands.CreateCampaign;
using AdvertisingService.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace AdvertisingService.Tests.Application;

public class CreateCampaignValidatorTests
{
    private readonly CreateCampaignCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new CreateCampaignCommand(
            OwnerId: Guid.NewGuid(),
            OwnerType: "Individual",
            VehicleId: Guid.NewGuid(),
            PlacementType: AdPlacementType.FeaturedSpot,
            PricingModel: CampaignPricingModel.PerView,
            TotalBudget: 500m,
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(31));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyOwnerId_ShouldHaveError()
    {
        var command = new CreateCampaignCommand(
            OwnerId: Guid.Empty,
            OwnerType: "Individual",
            VehicleId: Guid.NewGuid(),
            PlacementType: AdPlacementType.FeaturedSpot,
            PricingModel: CampaignPricingModel.PerView,
            TotalBudget: 500m,
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(31));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
    }

    [Fact]
    public void Validate_WithNegativeBudget_ShouldHaveError()
    {
        var command = new CreateCampaignCommand(
            OwnerId: Guid.NewGuid(),
            OwnerType: "Individual",
            VehicleId: Guid.NewGuid(),
            PlacementType: AdPlacementType.FeaturedSpot,
            PricingModel: CampaignPricingModel.PerView,
            TotalBudget: -100m,
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(31));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TotalBudget);
    }

    [Fact]
    public void Validate_WithEndDateBeforeStartDate_ShouldHaveError()
    {
        var command = new CreateCampaignCommand(
            OwnerId: Guid.NewGuid(),
            OwnerType: "Individual",
            VehicleId: Guid.NewGuid(),
            PlacementType: AdPlacementType.FeaturedSpot,
            PricingModel: CampaignPricingModel.PerView,
            TotalBudget: 500m,
            StartDate: DateTime.UtcNow.AddDays(10),
            EndDate: DateTime.UtcNow.AddDays(5));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Validate_WithSqlInjectionInOwnerType_ShouldHaveError()
    {
        var command = new CreateCampaignCommand(
            OwnerId: Guid.NewGuid(),
            OwnerType: "'; DROP TABLE campaigns;--",
            VehicleId: Guid.NewGuid(),
            PlacementType: AdPlacementType.FeaturedSpot,
            PricingModel: CampaignPricingModel.PerView,
            TotalBudget: 500m,
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(31));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OwnerType);
    }
}

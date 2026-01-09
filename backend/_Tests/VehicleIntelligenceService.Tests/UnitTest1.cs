using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehicleIntelligenceService.Application.DTOs;
using VehicleIntelligenceService.Application.Features.Demand.Queries;
using VehicleIntelligenceService.Application.Features.Pricing.Queries;
using VehicleIntelligenceService.Infrastructure.Persistence;
using VehicleIntelligenceService.Infrastructure.Persistence.Repositories;
using VehicleIntelligenceService.Infrastructure.Services;

namespace VehicleIntelligenceService.Tests;

public class VehicleIntelligenceServiceTests
{
    [Fact]
    public void VehiclePricingEngine_Computes_PositiveMarketAndSuggestedPrice()
    {
        var engine = new VehiclePricingEngine();

        var result = engine.ComputeSuggestion(
            make: "Toyota",
            model: "Corolla",
            year: 2020,
            mileage: 45000,
            bodyType: "Sedan",
            location: "Santo Domingo",
            askingPrice: 20000m);

        result.MarketPrice.Should().BeGreaterThan(0);
        result.SuggestedPrice.Should().BeGreaterThan(0);
        result.Confidence.Should().BeInRange(0.40m, 0.90m);
    }

    [Fact]
    public void VehiclePricingEngine_DeltaPercent_IsPositiveWhenOverMarket()
    {
        var engine = new VehiclePricingEngine();

        var result = engine.ComputeSuggestion(
            make: "Honda",
            model: "CR-V",
            year: 2019,
            mileage: 60000,
            bodyType: "SUV",
            location: "Santo Domingo",
            askingPrice: 50000m);

        result.DeltaPercent.Should().BeGreaterThan(0);
    }

    [Fact]
    public void VehiclePricingEngine_EstimatedDaysToSell_DecreasesWhenUnderMarket()
    {
        var engine = new VehiclePricingEngine();

        var overpriced = engine.ComputeSuggestion(
            make: "Toyota",
            model: "RAV4",
            year: 2021,
            mileage: 25000,
            bodyType: "SUV",
            location: "Santo Domingo",
            askingPrice: 80000m);

        var underpriced = engine.ComputeSuggestion(
            make: "Toyota",
            model: "RAV4",
            year: 2021,
            mileage: 25000,
            bodyType: "SUV",
            location: "Santo Domingo",
            askingPrice: 12000m);

        underpriced.EstimatedDaysToSell.Should().BeLessThan(overpriced.EstimatedDaysToSell);
    }

    [Fact]
    public void VehiclePricingEngine_BuildSellingTips_IncludesPriceAdvice_WhenOverpriced()
    {
        var engine = new VehiclePricingEngine();

        var result = engine.ComputeSuggestion(
            make: "Ford",
            model: "Explorer",
            year: 2018,
            mileage: 90000,
            bodyType: "SUV",
            location: "Santiago",
            askingPrice: 90000m);

        var tips = engine.BuildSellingTips(result);
        tips.Should().NotBeEmpty();
        tips.Should().Contain(t => t.Contains("Baja el precio", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetDemandByCategoryHandler_ReturnsOrderedByDemandScore()
    {
        await using var db = CreateDb();
        var repo = new CategoryDemandRepository(db);

        await repo.UpsertManyAsync(new[]
        {
            new VehicleIntelligenceService.Domain.Entities.CategoryDemandSnapshot { Category = "Sedan", DemandScore = 50, Trend = "stable" },
            new VehicleIntelligenceService.Domain.Entities.CategoryDemandSnapshot { Category = "SUV", DemandScore = 80, Trend = "up" },
        });

        var handler = new GetDemandByCategoryQueryHandler(repo);
        var result = await handler.Handle(new GetDemandByCategoryQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Category.Should().Be("SUV");
        result[1].Category.Should().Be("Sedan");
    }

    [Fact]
    public async Task GetPriceSuggestionHandler_PersistsSuggestionRecord()
    {
        await using var db = CreateDb();
        var suggestionRepo = new PriceSuggestionRepository(db);
        var demandRepo = new CategoryDemandRepository(db);
        await demandRepo.UpsertManyAsync(new[]
        {
            new VehicleIntelligenceService.Domain.Entities.CategoryDemandSnapshot { Category = "SUV", DemandScore = 85, Trend = "up" }
        });

        var engine = new VehiclePricingEngine();
        var handler = new GetPriceSuggestionQueryHandler(engine, suggestionRepo, demandRepo);

        var dto = new PriceSuggestionRequestDto("Toyota", "RAV4", 2021, 25000, "SUV", "Santo Domingo", 25000m);
        var response = await handler.Handle(new GetPriceSuggestionQuery(dto), CancellationToken.None);

        response.SuggestedPrice.Should().BeGreaterThan(0);
        (await db.PriceSuggestions.CountAsync()).Should().Be(1);
    }

    private static VehicleIntelligenceDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<VehicleIntelligenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new VehicleIntelligenceDbContext(options);
    }
}
using CarDealer.Contracts.Enums;
using Xunit;

namespace AdminService.Tests.Shared;

/// <summary>
/// Tests for PlanConfiguration — the single source of truth for OKLA v2 plan mapping.
/// Ensures consistency between backend enum names, frontend display names, and pricing.
/// </summary>
public class PlanConfigurationTests
{
    // =========================================================================
    // GetDisplayName — maps internal names → v2 display names
    // =========================================================================

    [Theory]
    [InlineData("Free", "Libre")]
    [InlineData("free", "Libre")]
    [InlineData("FREE", "Libre")]
    [InlineData("Basic", "Visible")]
    [InlineData("basic", "Visible")]
    [InlineData("Professional", "Pro")]
    [InlineData("professional", "Pro")]
    [InlineData("Enterprise", "Elite")]
    [InlineData("enterprise", "Elite")]
    public void GetDisplayName_FromEnumNames_ReturnsV2Name(string internalName, string expected)
    {
        Assert.Equal(expected, PlanConfiguration.GetDisplayName(internalName));
    }

    [Theory]
    [InlineData("Libre", "Libre")]
    [InlineData("Visible", "Visible")]
    [InlineData("Pro", "Pro")]
    [InlineData("Elite", "Elite")]
    [InlineData("libre", "Libre")]
    [InlineData("visible", "Visible")]
    [InlineData("pro", "Pro")]
    [InlineData("elite", "Elite")]
    public void GetDisplayName_FromV2Names_IsIdempotent(string name, string expected)
    {
        Assert.Equal(expected, PlanConfiguration.GetDisplayName(name));
    }

    [Theory]
    [InlineData("Starter", "Visible")]  // Old v1 name maps to Visible
    [InlineData("starter", "Visible")]
    [InlineData("Premium", "Elite")]     // Alias maps to Elite
    [InlineData("Custom", "Elite")]      // Custom maps to Elite
    public void GetDisplayName_FromLegacyNames_MapsCorrectly(string legacyName, string expected)
    {
        Assert.Equal(expected, PlanConfiguration.GetDisplayName(legacyName));
    }

    [Theory]
    [InlineData(null, "Libre")]
    [InlineData("", "Libre")]
    [InlineData("  ", "Libre")]
    [InlineData("none", "Libre")]
    [InlineData("unknown_plan", "Libre")]
    public void GetDisplayName_NullEmptyUnknown_ReturnsLibre(string? input, string expected)
    {
        Assert.Equal(expected, PlanConfiguration.GetDisplayName(input));
    }

    // =========================================================================
    // GetMonthlyPrice — returns v2 USD prices
    // =========================================================================

    [Theory]
    [InlineData("Free", 0)]
    [InlineData("Basic", 29)]
    [InlineData("Professional", 89)]
    [InlineData("Enterprise", 199)]
    public void GetMonthlyPrice_FromEnumNames_ReturnsV2Price(string name, decimal expected)
    {
        Assert.Equal(expected, PlanConfiguration.GetMonthlyPrice(name));
    }

    [Theory]
    [InlineData("Libre", 0)]
    [InlineData("Visible", 29)]
    [InlineData("Pro", 89)]
    [InlineData("Elite", 199)]
    public void GetMonthlyPrice_FromV2Names_ReturnsCorrectPrice(string name, decimal expected)
    {
        Assert.Equal(expected, PlanConfiguration.GetMonthlyPrice(name));
    }

    [Theory]
    [InlineData("Starter", 29)]   // Old v1 Starter→Visible=$29 (NOT $49)
    [InlineData("Premium", 199)]  // Premium→Elite=$199 (NOT $299)
    public void GetMonthlyPrice_FromLegacyNames_ReturnsV2Price_NotV1Price(string name, decimal expected)
    {
        // Critical regression test: old v1 prices ($49, $149, $299) must NOT be returned
        Assert.Equal(expected, PlanConfiguration.GetMonthlyPrice(name));
        Assert.NotEqual(49m, PlanConfiguration.GetMonthlyPrice("Starter"));
        Assert.NotEqual(149m, PlanConfiguration.GetMonthlyPrice("Pro"));
        Assert.NotEqual(299m, PlanConfiguration.GetMonthlyPrice("Enterprise"));
    }

    [Fact]
    public void GetMonthlyPrice_NullOrEmpty_ReturnsZero()
    {
        Assert.Equal(0m, PlanConfiguration.GetMonthlyPrice(null));
        Assert.Equal(0m, PlanConfiguration.GetMonthlyPrice(""));
    }

    // =========================================================================
    // GetFrontendKey — returns lowercase keys for frontend DealerPlan enum
    // =========================================================================

    [Theory]
    [InlineData("Free", "libre")]
    [InlineData("Basic", "visible")]
    [InlineData("Professional", "pro")]
    [InlineData("Enterprise", "elite")]
    [InlineData("Starter", "visible")]
    [InlineData("Premium", "elite")]
    [InlineData(null, "libre")]
    [InlineData("", "libre")]
    public void GetFrontendKey_MapsCorrectly(string? input, string expected)
    {
        Assert.Equal(expected, PlanConfiguration.GetFrontendKey(input));
    }

    // =========================================================================
    // Static collections
    // =========================================================================

    [Fact]
    public void AllDisplayNames_HasFourTiersInOrder()
    {
        var names = PlanConfiguration.AllDisplayNames;
        Assert.Equal(4, names.Count);
        Assert.Equal("Libre", names[0]);
        Assert.Equal("Visible", names[1]);
        Assert.Equal("Pro", names[2]);
        Assert.Equal("Elite", names[3]);
    }

    [Fact]
    public void PricesByDisplayName_MatchesExpected()
    {
        var prices = PlanConfiguration.PricesByDisplayName;
        Assert.Equal(4, prices.Count);
        Assert.Equal(0m, prices["Libre"]);
        Assert.Equal(29m, prices["Visible"]);
        Assert.Equal(89m, prices["Pro"]);
        Assert.Equal(199m, prices["Elite"]);
    }

    [Fact]
    public void AllPrices_AreConsistentWithConstants()
    {
        Assert.Equal(PlanConfiguration.PriceLibre, PlanConfiguration.PricesByDisplayName["Libre"]);
        Assert.Equal(PlanConfiguration.PriceVisible, PlanConfiguration.PricesByDisplayName["Visible"]);
        Assert.Equal(PlanConfiguration.PricePro, PlanConfiguration.PricesByDisplayName["Pro"]);
        Assert.Equal(PlanConfiguration.PriceElite, PlanConfiguration.PricesByDisplayName["Elite"]);
    }

    // =========================================================================
    // Consistency: v2 prices match frontend plan-config.ts exactly
    // =========================================================================

    [Fact]
    public void V2Prices_MatchFrontendPlanConfig()
    {
        // These must match frontend/web-next/src/lib/plan-config.ts DEALER_PLAN_PRICES
        Assert.Equal(0m, PlanConfiguration.PriceLibre);
        Assert.Equal(29m, PlanConfiguration.PriceVisible);
        Assert.Equal(89m, PlanConfiguration.PricePro);
        Assert.Equal(199m, PlanConfiguration.PriceElite);
    }
}

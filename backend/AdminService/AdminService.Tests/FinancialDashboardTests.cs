using AdminService.Application.Interfaces;
using AdminService.Application.UseCases.Dealers;
using AdminService.Application.UseCases.Finance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AdminService.Tests;

/// <summary>
/// CONTRA #5 FIX: Financial Dashboard Tests
///
/// Validates the financial dashboard handler correctly:
///   1. Calculates expenses by category (API, infrastructure, marketing, development)
///   2. Calculates revenue by plan (Libre, Visible, Pro, Elite) using real pricing
///   3. Computes net margin = Revenue - Expenses
///   4. Projects runway = CashBalance / MonthlyBurnRate
///   5. Handles edge cases (no data, zero revenue, zero expenses)
/// </summary>
public class FinancialDashboardTests
{
    private readonly Mock<IDealerService> _dealerServiceMock;
    private readonly Mock<IFinancialDataProvider> _financialDataMock;
    private readonly GetFinancialDashboardQueryHandler _handler;

    public FinancialDashboardTests()
    {
        _dealerServiceMock = new Mock<IDealerService>();
        _financialDataMock = new Mock<IFinancialDataProvider>();
        var logger = NullLogger<GetFinancialDashboardQueryHandler>.Instance;

        _handler = new GetFinancialDashboardQueryHandler(
            _dealerServiceMock.Object,
            _financialDataMock.Object,
            logger);
    }

    // ════════════════════════════════════════════════════════════════════════
    // REVENUE CALCULATION
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Revenue_CalculatesCorrectMrrFromPlanBreakdown()
    {
        // Arrange: 10 Libre ($0), 5 Visible ($29), 3 Pro ($89), 2 Elite ($199)
        SetupDealerStats(libre: 10, visible: 5, pro: 3, elite: 2);
        SetupDefaultExpenses();

        // Act
        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        // Assert
        // Expected MRR = (10×$0) + (5×$29) + (3×$89) + (2×$199) = $0 + $145 + $267 + $398 = $810
        Assert.Equal(810m, result.Revenue.Mrr);
        Assert.Equal(4, result.Revenue.ByPlan.Count);

        var elitePlan = result.Revenue.ByPlan.First(p => p.PlanKey == "elite");
        Assert.Equal(2, elitePlan.DealerCount);
        Assert.Equal(199m, elitePlan.PricePerMonth);
        Assert.Equal(398m, elitePlan.TotalMrr);
    }

    [Fact]
    public async Task Revenue_LibrePlanContributesZeroMrr()
    {
        SetupDealerStats(libre: 100, visible: 0, pro: 0, elite: 0);
        SetupDefaultExpenses();

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        Assert.Equal(0m, result.Revenue.Mrr);
        var librePlan = result.Revenue.ByPlan.First(p => p.PlanKey == "libre");
        Assert.Equal(0m, librePlan.TotalMrr);
        Assert.Equal(100, librePlan.DealerCount);
    }

    [Fact]
    public async Task Revenue_IncludesOverageAndAdvertising()
    {
        SetupDealerStats(libre: 0, visible: 0, pro: 0, elite: 5);
        SetupDefaultExpenses();
        _financialDataMock.Setup(x => x.GetOverageRevenueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(80m); // $80 in overage
        _financialDataMock.Setup(x => x.GetAdvertisingRevenueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(150m); // $150 in advertising

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        // MRR = 5×$199 = $995
        // Total Revenue = $995 + $80 + $150 = $1,225
        Assert.Equal(1225m, result.TotalRevenue);
        Assert.Equal(3, result.Revenue.BySources.Count);

        var overageSource = result.Revenue.BySources.First(s => s.Source.Contains("Overage"));
        Assert.Equal(80m, overageSource.Amount);
    }

    // ════════════════════════════════════════════════════════════════════════
    // EXPENSE CALCULATION
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Expenses_AggregatesAllCategories()
    {
        SetupDealerStats(libre: 0, visible: 0, pro: 0, elite: 1);
        SetupExpenses(api: 300m, infra: 285m, marketing: 500m, dev: 3300m);

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        // Total = $300 + $285 + $500 + $3300 = $4,385
        Assert.Equal(4385m, result.TotalExpenses);
        Assert.Equal(300m, result.Expenses.Api.Amount);
        Assert.Equal(285m, result.Expenses.Infrastructure.Amount);
        Assert.Equal(500m, result.Expenses.Marketing.Amount);
        Assert.Equal(3300m, result.Expenses.Development.Amount);
    }

    [Fact]
    public async Task Expenses_PercentOfTotalCalculatesCorrectly()
    {
        SetupDealerStats(libre: 0, visible: 0, pro: 0, elite: 1);
        SetupExpenses(api: 100m, infra: 100m, marketing: 100m, dev: 100m);

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        // Each category is 25% of $400 total
        Assert.Equal(25m, result.Expenses.Api.PercentOfTotal);
        Assert.Equal(25m, result.Expenses.Infrastructure.PercentOfTotal);
    }

    [Fact]
    public async Task Expenses_AsList_ReturnsFourCategories()
    {
        SetupDealerStats(libre: 0, visible: 0, pro: 0, elite: 0);
        SetupDefaultExpenses();

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        var list = result.Expenses.AsList();
        Assert.Equal(4, list.Count);
    }

    // ════════════════════════════════════════════════════════════════════════
    // NET MARGIN
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task NetMargin_WhenProfitable_IsPositive()
    {
        SetupDealerStats(libre: 0, visible: 10, pro: 10, elite: 10);
        SetupExpenses(api: 200m, infra: 285m, marketing: 100m, dev: 1000m);

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        // Revenue = (10×$29) + (10×$89) + (10×$199) = $290 + $890 + $1990 = $3,170
        // Expenses = $200 + $285 + $100 + $1000 = $1,585
        // Margin = $3,170 - $1,585 = $1,585
        Assert.Equal(3170m, result.TotalRevenue);
        Assert.Equal(1585m, result.TotalExpenses);
        Assert.Equal(1585m, result.NetMargin);
        Assert.True(result.IsProfitable);
        Assert.True(result.NetMarginPercent > 0);
    }

    [Fact]
    public async Task NetMargin_WhenLosing_IsNegative()
    {
        SetupDealerStats(libre: 100, visible: 1, pro: 0, elite: 0);
        SetupExpenses(api: 500m, infra: 285m, marketing: 1000m, dev: 3300m);

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        // Revenue = 1×$29 = $29
        // Expenses = $500 + $285 + $1000 + $3300 = $5,085
        // Margin = $29 - $5,085 = -$5,056
        Assert.Equal(29m, result.TotalRevenue);
        Assert.Equal(-5056m, result.NetMargin);
        Assert.False(result.IsProfitable);
    }

    // ════════════════════════════════════════════════════════════════════════
    // BURN RATE & RUNWAY
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Runway_WhenProfitable_IsMinusOne()
    {
        SetupDealerStats(libre: 0, visible: 0, pro: 0, elite: 50);
        SetupExpenses(api: 500m, infra: 285m, marketing: 1000m, dev: 3300m);
        _financialDataMock.Setup(x => x.GetCashBalanceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(50000m);

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        // Revenue = 50×$199 = $9,950
        // Expenses = $5,085
        // Profitable → runway = -1 (infinite)
        Assert.True(result.IsProfitable);
        Assert.Equal(-1m, result.RunwayMonths);
    }

    [Fact]
    public async Task Runway_WhenBurning_CalculatesMonths()
    {
        SetupDealerStats(libre: 100, visible: 0, pro: 0, elite: 0);
        SetupExpenses(api: 500m, infra: 285m, marketing: 1000m, dev: 3300m);
        _financialDataMock.Setup(x => x.GetCashBalanceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(50000m);

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        // Revenue = $0 | Expenses = $5,085 | Net = -$5,085
        // If day 15 of month: daily burn = $5085/15 ≈ $339, monthly burn = $339 × 30 ≈ $10,170
        // Runway = $50,000 / $10,170 ≈ 4.9 months
        Assert.False(result.IsProfitable);
        Assert.True(result.RunwayMonths > 0);
        Assert.True(result.CashBalance == 50000m);
    }

    [Fact]
    public async Task Runway_NoCashBalance_ReturnsZero()
    {
        SetupDealerStats(libre: 100, visible: 0, pro: 0, elite: 0);
        SetupExpenses(api: 500m, infra: 285m, marketing: 1000m, dev: 3300m);
        _financialDataMock.Setup(x => x.GetCashBalanceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        Assert.Equal(0m, result.RunwayMonths);
    }

    // ════════════════════════════════════════════════════════════════════════
    // PERIOD HANDLING
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Period_DefaultsToCurrentMonth()
    {
        SetupDealerStats(libre: 0, visible: 0, pro: 0, elite: 0);
        SetupDefaultExpenses();

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        var expectedPeriod = DateTime.UtcNow.ToString("yyyy-MM");
        Assert.Equal(expectedPeriod, result.Period);
    }

    [Fact]
    public async Task Period_UsesProvidedValue()
    {
        SetupDealerStats(libre: 0, visible: 0, pro: 0, elite: 0);
        SetupDefaultExpenses();

        var result = await _handler.Handle(
            new GetFinancialDashboardQuery("2025-12"), CancellationToken.None);

        Assert.Equal("2025-12", result.Period);
    }

    // ════════════════════════════════════════════════════════════════════════
    // EDGE CASES
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ZeroRevenue_ZeroExpenses_AllZeros()
    {
        SetupDealerStats(libre: 0, visible: 0, pro: 0, elite: 0);
        SetupExpenses(api: 0m, infra: 0m, marketing: 0m, dev: 0m);

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        Assert.Equal(0m, result.TotalRevenue);
        Assert.Equal(0m, result.TotalExpenses);
        Assert.Equal(0m, result.NetMargin);
        Assert.Equal(0m, result.NetMarginPercent);
    }

    [Fact]
    public async Task NullDealerStats_HandlesGracefully()
    {
        _dealerServiceMock.Setup(x => x.GetDealerStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DealerStatsDto());
        SetupDefaultExpenses();

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        Assert.Equal(0m, result.Revenue.Mrr);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Dashboard_CurrencyIsUsd()
    {
        SetupDealerStats(libre: 0, visible: 0, pro: 0, elite: 0);
        SetupDefaultExpenses();

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task Dashboard_GeneratedAtIsRecent()
    {
        SetupDealerStats(libre: 0, visible: 0, pro: 0, elite: 0);
        SetupDefaultExpenses();

        var before = DateTimeOffset.UtcNow;
        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);
        var after = DateTimeOffset.UtcNow;

        Assert.True(result.GeneratedAt >= before);
        Assert.True(result.GeneratedAt <= after);
    }

    // ════════════════════════════════════════════════════════════════════════
    // PLAN REVENUE BREAKDOWN
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task PlanRevenue_UsesCorrectPricing()
    {
        SetupDealerStats(libre: 1, visible: 1, pro: 1, elite: 1);
        SetupDefaultExpenses();

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        var plans = result.Revenue.ByPlan;
        Assert.Equal(0m, plans.First(p => p.PlanKey == "libre").PricePerMonth);
        Assert.Equal(29m, plans.First(p => p.PlanKey == "visible").PricePerMonth);
        Assert.Equal(89m, plans.First(p => p.PlanKey == "pro").PricePerMonth);
        Assert.Equal(199m, plans.First(p => p.PlanKey == "elite").PricePerMonth);
    }

    [Fact]
    public async Task PlanRevenue_PercentOfMrrSumsToApproximately100()
    {
        SetupDealerStats(libre: 5, visible: 10, pro: 8, elite: 3);
        SetupDefaultExpenses();

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        var totalPercent = result.Revenue.ByPlan
            .Where(p => p.TotalMrr > 0)
            .Sum(p => p.PercentOfMrr);

        // Should be ~100% (rounding may cause ±0.5%)
        Assert.InRange(totalPercent, 99m, 101m);
    }

    // ════════════════════════════════════════════════════════════════════════
    // REVENUE SOURCES
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task RevenueSources_AlwaysHasThreeSources()
    {
        SetupDealerStats(libre: 0, visible: 0, pro: 0, elite: 1);
        SetupDefaultExpenses();

        var result = await _handler.Handle(new GetFinancialDashboardQuery(), CancellationToken.None);

        Assert.Equal(3, result.Revenue.BySources.Count);
        Assert.Contains(result.Revenue.BySources, s => s.Source == "Suscripciones");
        Assert.Contains(result.Revenue.BySources, s => s.Source.Contains("Overage"));
        Assert.Contains(result.Revenue.BySources, s => s.Source == "Publicidad");
    }

    // ════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════════════

    private void SetupDealerStats(int libre, int visible, int pro, int elite)
    {
        var stats = new DealerStatsDto
        {
            Total = libre + visible + pro + elite,
            Active = visible + pro + elite,
            ByPlan = new DealerPlanBreakdown
            {
                Libre = libre,
                Visible = visible,
                Pro = pro,
                Elite = elite,
            },
            TotalMrr = visible * 29m + pro * 89m + elite * 199m,
        };
        _dealerServiceMock.Setup(x => x.GetDealerStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);
    }

    private void SetupExpenses(decimal api, decimal infra, decimal marketing, decimal dev)
    {
        _financialDataMock.Setup(x => x.GetApiCostsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((api, new List<ExpenseSubItemDto> { new() { Name = "Claude", Amount = api } }));
        _financialDataMock.Setup(x => x.GetInfrastructureCostsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((infra, new List<ExpenseSubItemDto> { new() { Name = "DigitalOcean", Amount = infra } }));
        _financialDataMock.Setup(x => x.GetMarketingCostsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((marketing, new List<ExpenseSubItemDto> { new() { Name = "Google Ads", Amount = marketing } }));
        _financialDataMock.Setup(x => x.GetDevelopmentCostsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((dev, new List<ExpenseSubItemDto> { new() { Name = "Team", Amount = dev } }));
        _financialDataMock.Setup(x => x.GetOverageRevenueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);
        _financialDataMock.Setup(x => x.GetAdvertisingRevenueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);
        _financialDataMock.Setup(x => x.GetDailyExpenseHistoryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DailyFinancialEntryDto>());
        _financialDataMock.Setup(x => x.GetCashBalanceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);
    }

    private void SetupDefaultExpenses()
    {
        SetupExpenses(api: 0m, infra: 0m, marketing: 0m, dev: 0m);
    }
}

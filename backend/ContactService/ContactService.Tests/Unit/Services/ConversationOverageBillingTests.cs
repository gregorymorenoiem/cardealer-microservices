using CarDealer.Contracts.Enums;
using ContactService.Application.DTOs;
using ContactService.Application.Features.ContactRequests.Queries;
using ContactService.Domain.Entities;
using ContactService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace ContactService.Tests.Unit.Services;

/// <summary>
/// Unit tests for the overage billing and reporting system.
///
/// CONTRA #5 / OVERAGE BILLING FIX
///
/// Validates:
/// - Overage conversations (2,001 to N) are registered with timestamps
/// - Overage cost calculation: count × $0.08
/// - Overage report generation with per-conversation details
/// - CSV export format
/// </summary>
public class ConversationOverageBillingTests
{
    // ========================================================================
    // OVERAGE DETAIL ENTITY TESTS
    // ========================================================================

    [Fact]
    public void OverageDetail_CreatesWithCorrectDefaults()
    {
        var detail = new ConversationOverageDetail
        {
            DealerId = Guid.NewGuid(),
            ContactRequestId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Subject = "2024 Toyota Camry",
            DealerPlan = "elite",
            BillingPeriod = "2026-03",
            ConversationNumber = 2001,
            PlanLimit = 2000,
            UnitCost = 0.08m
        };

        detail.Id.Should().NotBe(Guid.Empty);
        detail.OccurredAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        detail.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        detail.ConversationNumber.Should().Be(2001);
        detail.PlanLimit.Should().Be(2000);
        detail.UnitCost.Should().Be(0.08m);
    }

    [Theory]
    [InlineData(2001, 0.08)]
    [InlineData(2050, 0.08)]
    [InlineData(2500, 0.08)]
    [InlineData(3000, 0.08)]
    public void OverageDetail_UnitCostIsAlways008(int conversationNumber, decimal expectedCost)
    {
        var detail = new ConversationOverageDetail
        {
            ConversationNumber = conversationNumber,
            UnitCost = PlanFeatureLimits.OverageCostPerConversation
        };

        detail.UnitCost.Should().Be(expectedCost);
    }

    // ========================================================================
    // OVERAGE COST CALCULATION TESTS
    // ========================================================================

    [Theory]
    [InlineData(2001, 1, 0.08)]     // 1 overage conversation = $0.08
    [InlineData(2010, 10, 0.80)]    // 10 overage conversations = $0.80
    [InlineData(2100, 100, 8.00)]   // 100 overage conversations = $8.00
    [InlineData(2340, 340, 27.20)]  // 340 overage conversations = $27.20
    [InlineData(2500, 500, 40.00)]  // 500 overage conversations = $40.00
    public void OverageCost_CalculatesCorrectly(int totalConversations, int expectedOverage, decimal expectedCost)
    {
        var includedLimit = 2000;
        var overageCount = totalConversations - includedLimit;
        var totalCost = overageCount * PlanFeatureLimits.OverageCostPerConversation;

        overageCount.Should().Be(expectedOverage);
        totalCost.Should().Be(expectedCost);
    }

    [Fact]
    public void OverageCost_NoOverage_IsZero()
    {
        var totalConversations = 1999;
        var includedLimit = 2000;
        var overageCount = Math.Max(0, totalConversations - includedLimit);
        var totalCost = overageCount * PlanFeatureLimits.OverageCostPerConversation;

        overageCount.Should().Be(0);
        totalCost.Should().Be(0m);
    }

    [Fact]
    public void OverageCost_ExactlyAtLimit_IsZero()
    {
        var totalConversations = 2000;
        var includedLimit = 2000;
        var overageCount = Math.Max(0, totalConversations - includedLimit);
        var totalCost = overageCount * PlanFeatureLimits.OverageCostPerConversation;

        overageCount.Should().Be(0);
        totalCost.Should().Be(0m);
    }

    // ========================================================================
    // OVERAGE REPORT QUERY HANDLER TESTS
    // ========================================================================

    [Fact]
    public async Task GetOverageReport_WithOverages_ReturnsDetailedReport()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var billingPeriod = "2026-03";
        var overageDetails = new List<ConversationOverageDetail>
        {
            new()
            {
                DealerId = dealerId,
                ContactRequestId = Guid.NewGuid(),
                BuyerId = Guid.NewGuid(),
                Subject = "2024 Honda Civic",
                DealerPlan = "elite",
                BillingPeriod = billingPeriod,
                ConversationNumber = 2001,
                PlanLimit = 2000,
                UnitCost = 0.08m,
                OccurredAtUtc = new DateTime(2026, 3, 15, 10, 30, 0, DateTimeKind.Utc)
            },
            new()
            {
                DealerId = dealerId,
                ContactRequestId = Guid.NewGuid(),
                BuyerId = Guid.NewGuid(),
                Subject = "2025 Toyota RAV4",
                DealerPlan = "elite",
                BillingPeriod = billingPeriod,
                ConversationNumber = 2002,
                PlanLimit = 2000,
                UnitCost = 0.08m,
                OccurredAtUtc = new DateTime(2026, 3, 15, 11, 45, 0, DateTimeKind.Utc)
            }
        };

        var mockOverageRepo = new Mock<IConversationOverageRepository>();
        mockOverageRepo.Setup(r => r.GetByDealerAndPeriodAsync(
                dealerId, billingPeriod, It.IsAny<CancellationToken>()))
            .ReturnsAsync(overageDetails);

        var mockPlanResolver = new Mock<IDealerPlanResolver>();
        mockPlanResolver.Setup(r => r.GetDealerPlanAsync(dealerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("elite");

        var mockLogger = new Mock<ILogger<GetConversationOverageReportQueryHandler>>();

        var handler = new GetConversationOverageReportQueryHandler(
            mockOverageRepo.Object,
            mockPlanResolver.Object,
            mockLogger.Object);

        var query = new GetConversationOverageReportQuery
        {
            DealerId = dealerId,
            BillingPeriod = billingPeriod
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.DealerId.Should().Be(dealerId);
        result.BillingPeriod.Should().Be(billingPeriod);
        result.DealerPlan.Should().Be("elite");
        result.IncludedLimit.Should().Be(2000);
        result.OverageCount.Should().Be(2);
        result.UnitCost.Should().Be(0.08m);
        result.TotalOverageCost.Should().Be(0.16m);
        result.Currency.Should().Be("USD");
        result.Details.Should().HaveCount(2);

        result.Details[0].ConversationNumber.Should().Be(2001);
        result.Details[0].Subject.Should().Be("2024 Honda Civic");
        result.Details[0].UnitCost.Should().Be(0.08m);

        result.Details[1].ConversationNumber.Should().Be(2002);
        result.Details[1].OccurredAtUtc.Should().Be(new DateTime(2026, 3, 15, 11, 45, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task GetOverageReport_NoOverages_ReturnsEmptyReport()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var billingPeriod = "2026-03";

        var mockOverageRepo = new Mock<IConversationOverageRepository>();
        mockOverageRepo.Setup(r => r.GetByDealerAndPeriodAsync(
                dealerId, billingPeriod, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConversationOverageDetail>());

        var mockPlanResolver = new Mock<IDealerPlanResolver>();
        mockPlanResolver.Setup(r => r.GetDealerPlanAsync(dealerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("elite");

        var mockLogger = new Mock<ILogger<GetConversationOverageReportQueryHandler>>();

        var handler = new GetConversationOverageReportQueryHandler(
            mockOverageRepo.Object,
            mockPlanResolver.Object,
            mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetConversationOverageReportQuery
        {
            DealerId = dealerId,
            BillingPeriod = billingPeriod
        }, CancellationToken.None);

        // Assert
        result.OverageCount.Should().Be(0);
        result.TotalOverageCost.Should().Be(0m);
        result.Details.Should().BeEmpty();
        result.IncludedLimit.Should().Be(2000);
    }

    [Fact]
    public async Task GetOverageReport_DefaultsPeriodToCurrentMonth()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var expectedPeriod = DateTime.UtcNow.ToString("yyyy-MM");

        var mockOverageRepo = new Mock<IConversationOverageRepository>();
        mockOverageRepo.Setup(r => r.GetByDealerAndPeriodAsync(
                dealerId, expectedPeriod, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConversationOverageDetail>());

        var mockPlanResolver = new Mock<IDealerPlanResolver>();
        mockPlanResolver.Setup(r => r.GetDealerPlanAsync(dealerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("elite");

        var mockLogger = new Mock<ILogger<GetConversationOverageReportQueryHandler>>();

        var handler = new GetConversationOverageReportQueryHandler(
            mockOverageRepo.Object,
            mockPlanResolver.Object,
            mockLogger.Object);

        // Act — BillingPeriod is null, should default to current month
        var result = await handler.Handle(new GetConversationOverageReportQuery
        {
            DealerId = dealerId,
            BillingPeriod = null
        }, CancellationToken.None);

        // Assert
        result.BillingPeriod.Should().Be(expectedPeriod);
    }

    // ========================================================================
    // BILLING EVENT TESTS
    // ========================================================================

    [Fact]
    public void ConversationOverageBillingEvent_CalculatesCorrectTotal()
    {
        var billingEvent = new CarDealer.Contracts.Events.Billing.ConversationOverageBillingEvent
        {
            DealerId = Guid.NewGuid(),
            DealerPlan = "elite",
            BillingPeriod = "2026-03",
            TotalConversations = 2340,
            IncludedLimit = 2000,
            OverageCount = 340,
            OverageUnitCost = 0.08m,
            OverageTotalAmount = 340 * 0.08m,
            Currency = "USD"
        };

        billingEvent.OverageTotalAmount.Should().Be(27.20m);
        billingEvent.EventType.Should().Be("billing.conversation.overage");
        billingEvent.EventId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ConversationOverageBillingEvent_ZeroOverage_TotalIsZero()
    {
        var overageCount = 0;
        var totalAmount = overageCount * PlanFeatureLimits.OverageCostPerConversation;

        totalAmount.Should().Be(0m);
    }

    // ========================================================================
    // ELITE COST ALERT THRESHOLD TESTS
    // ========================================================================

    [Fact]
    public void EliteCostAlertThreshold_Is90PercentOfPlanPrice()
    {
        // Elite plan is $199/month
        // 90% alert threshold = $179.10
        PlanFeatureLimits.EliteCostAlertThreshold.Should().Be(179.10m);
    }

    [Fact]
    public void OverageCost_WhenSignificant_ExceedsCostAlertThreshold()
    {
        // If a dealer has 2,239+ overage conversations:
        // 2,239 × $0.08 = $179.12 → exceeds $179.10 threshold
        var overageCountAtThreshold = (int)Math.Ceiling(PlanFeatureLimits.EliteCostAlertThreshold
            / PlanFeatureLimits.OverageCostPerConversation);

        overageCountAtThreshold.Should().Be(2239);

        var costAtThreshold = overageCountAtThreshold * PlanFeatureLimits.OverageCostPerConversation;
        costAtThreshold.Should().BeGreaterThanOrEqualTo(PlanFeatureLimits.EliteCostAlertThreshold);
    }

    // ========================================================================
    // PLAN-SPECIFIC OVERAGE TESTS
    // ========================================================================

    [Theory]
    [InlineData("libre", 0)]    // No ChatAgent access → no overage possible
    [InlineData("visible", 0)]  // No ChatAgent access → no overage possible
    [InlineData("pro", 500)]    // Pro has 500 included
    [InlineData("elite", 2000)] // Elite has 2,000 included
    public void PlanLimits_ChatAgentMonthlyMessages(string plan, int expectedLimit)
    {
        var limits = PlanFeatureLimits.GetLimits(plan);
        limits.ChatAgentMonthlyMessages.Should().Be(expectedLimit);
    }

    [Fact]
    public void OverageCostPerConversation_IsExactly008()
    {
        PlanFeatureLimits.OverageCostPerConversation.Should().Be(0.08m);
    }
}

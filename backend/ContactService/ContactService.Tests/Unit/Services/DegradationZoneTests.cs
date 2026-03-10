using CarDealer.Contracts.Enums;
using CarDealer.Shared.LlmGateway.Abstractions;
using FluentAssertions;
using Xunit;

namespace ContactService.Tests.Unit.Services;

/// <summary>
/// CONTRA #5 FIX: Degradation Zone Tests
///
/// Validates that the soft limit with progressive degradation system works correctly:
/// - Between 1,600–2,000 conversations (80%–100% of ÉLITE limit), the system
///   aggressively activates cache + cheaper providers
/// - Cost per conversation in degradation zone must be ≤ $0.04
/// - Cache TTL is extended from 24h to 72h
/// - Traffic split: 15% Claude, 60% Gemini Flash, 25% Llama local
/// </summary>
public class DegradationZoneTests
{
    // ════════════════════════════════════════════════════════════════════════
    // PLAN FEATURE LIMITS — Degradation Zone Constants
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void DegradationZoneMaxCost_Is004()
    {
        PlanFeatureLimits.DegradationZoneMaxCostPerConversation
            .Should().Be(0.04m, "marginal cost must not exceed $0.04 in degradation zone");
    }

    [Fact]
    public void DegradationZoneCacheTtl_Is72Hours()
    {
        PlanFeatureLimits.DegradationZoneCacheTtl
            .Should().Be(TimeSpan.FromHours(72), "cache TTL should be 72h (3× standard) in degradation zone");
    }

    [Theory]
    [InlineData(15, 60, 25)]
    public void DegradationZoneTrafficSplit_IsCheapModelHeavy(
        int expectedClaude, int expectedGemini, int expectedLlama)
    {
        PlanFeatureLimits.DegradationClaudePercent.Should().Be(expectedClaude);
        PlanFeatureLimits.DegradationGeminiPercent.Should().Be(expectedGemini);
        PlanFeatureLimits.DegradationLlamaPercent.Should().Be(expectedLlama);

        var total = PlanFeatureLimits.DegradationClaudePercent
                  + PlanFeatureLimits.DegradationGeminiPercent
                  + PlanFeatureLimits.DegradationLlamaPercent;
        total.Should().Be(100, "traffic split must sum to 100%");
    }

    // ════════════════════════════════════════════════════════════════════════
    // IsInDegradationZone — Range Detection
    // ════════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData("elite", 1599, false)]  // Just below 80%
    [InlineData("elite", 1600, true)]   // Exactly at 80% (warning threshold)
    [InlineData("elite", 1800, true)]   // Mid-zone
    [InlineData("elite", 2000, true)]   // At hard limit
    [InlineData("elite", 2001, false)]  // Above hard limit (overage, not degradation)
    public void IsInDegradationZone_Elite_CorrectRange(string plan, int count, bool expected)
    {
        PlanFeatureLimits.IsInDegradationZone(plan, count).Should().Be(expected);
    }

    [Theory]
    [InlineData("pro", 399, false)]     // Below 80% of Pro's 500
    [InlineData("pro", 400, true)]      // At 80% of 500
    [InlineData("pro", 500, true)]      // At hard limit
    [InlineData("pro", 501, false)]     // Above limit
    public void IsInDegradationZone_Pro_CorrectRange(string plan, int count, bool expected)
    {
        PlanFeatureLimits.IsInDegradationZone(plan, count).Should().Be(expected);
    }

    [Theory]
    [InlineData("libre", 0, false)]     // No ChatAgent access
    [InlineData("visible", 0, false)]   // No ChatAgent access
    public void IsInDegradationZone_NoChatAgentPlans_AlwaysFalse(string plan, int count, bool expected)
    {
        PlanFeatureLimits.IsInDegradationZone(plan, count).Should().Be(expected);
    }

    // ════════════════════════════════════════════════════════════════════════
    // GetUsagePercent — Usage Percentage Calculation
    // ════════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData("elite", 0, 0.0)]
    [InlineData("elite", 1000, 0.5)]
    [InlineData("elite", 1600, 0.8)]
    [InlineData("elite", 2000, 1.0)]
    [InlineData("elite", 2500, 1.25)]
    public void GetUsagePercent_Elite_CalculatesCorrectly(string plan, int count, double expected)
    {
        PlanFeatureLimits.GetUsagePercent(plan, count)
            .Should().BeApproximately(expected, 0.001);
    }

    [Theory]
    [InlineData("libre", 100)]
    [InlineData("visible", 100)]
    public void GetUsagePercent_NoChatAgent_ReturnsZero(string plan, int count)
    {
        PlanFeatureLimits.GetUsagePercent(plan, count).Should().Be(0.0);
    }

    // ════════════════════════════════════════════════════════════════════════
    // LlmRequest — PreferCache and DealerUsagePercent
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void LlmRequest_PreferCache_DefaultsFalse()
    {
        var request = new LlmRequest { UserMessage = "test" };
        request.PreferCache.Should().BeFalse();
        request.DealerUsagePercent.Should().Be(0.0);
    }

    [Fact]
    public void LlmRequest_PreferCache_CanBeSetForDegradationZone()
    {
        var request = new LlmRequest
        {
            UserMessage = "test",
            DealerId = Guid.NewGuid(),
            PreferCache = true,
            DealerUsagePercent = 0.85
        };

        request.PreferCache.Should().BeTrue();
        request.DealerUsagePercent.Should().Be(0.85);
    }

    // ════════════════════════════════════════════════════════════════════════
    // COST ANALYSIS — Degradation Zone Target ≤ $0.04/conversation
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void DegradationZone_WeightedAverageCost_ShouldNotExceed004()
    {
        // Calculate the weighted average cost per conversation in degradation zone
        // based on the traffic split and provider pricing.
        //
        // Assumptions for a typical ChatAgent conversation:
        //   - Input: ~500 tokens (user query)
        //   - Output: ~300 tokens (response)
        //   - Using cheapest models per provider

        const int inputTokens = 500;
        const int outputTokens = 300;

        // Claude Sonnet: $3.00 input / $15.00 output per 1M tokens
        var claudeCostPer1M = 3.00m * inputTokens / 1_000_000m + 15.00m * outputTokens / 1_000_000m;

        // Gemini Flash: $0.075 input / $0.30 output per 1M tokens
        var geminiCostPer1M = 0.075m * inputTokens / 1_000_000m + 0.30m * outputTokens / 1_000_000m;

        // Llama local: $0.00 (self-hosted)
        var llamaCost = 0.00m;

        // Weighted average based on degradation zone split (15% Claude, 60% Gemini, 25% Llama)
        var weightedAvg =
            claudeCostPer1M * PlanFeatureLimits.DegradationClaudePercent / 100m +
            geminiCostPer1M * PlanFeatureLimits.DegradationGeminiPercent / 100m +
            llamaCost * PlanFeatureLimits.DegradationLlamaPercent / 100m;

        weightedAvg.Should().BeLessThanOrEqualTo(
            PlanFeatureLimits.DegradationZoneMaxCostPerConversation,
            $"weighted average cost per conversation ({weightedAvg:F6}) must be ≤ $0.04 " +
            $"with split: {PlanFeatureLimits.DegradationClaudePercent}% Claude, " +
            $"{PlanFeatureLimits.DegradationGeminiPercent}% Gemini, " +
            $"{PlanFeatureLimits.DegradationLlamaPercent}% Llama");
    }

    [Fact]
    public void DegradationZone_HeaviestTokenUsage_StillUnder004()
    {
        // Worst case: heavy ChatAgent conversation (1500 input, 1000 output tokens)
        const int inputTokens = 1500;
        const int outputTokens = 1000;

        var claudeCost = 3.00m * inputTokens / 1_000_000m + 15.00m * outputTokens / 1_000_000m;
        var geminiCost = 0.075m * inputTokens / 1_000_000m + 0.30m * outputTokens / 1_000_000m;
        var llamaCost = 0.00m;

        var weightedAvg =
            claudeCost * PlanFeatureLimits.DegradationClaudePercent / 100m +
            geminiCost * PlanFeatureLimits.DegradationGeminiPercent / 100m +
            llamaCost * PlanFeatureLimits.DegradationLlamaPercent / 100m;

        // Even with 1500/1000 tokens, the weighted average should be reasonable
        // Claude: $0.0195 | Gemini: $0.0004125 | Llama: $0.00
        // Weighted: $0.0195 * 0.15 + $0.0004125 * 0.60 + $0 * 0.25 = ~$0.003
        weightedAvg.Should().BeLessThanOrEqualTo(
            PlanFeatureLimits.DegradationZoneMaxCostPerConversation,
            "even heavy conversations must stay ≤ $0.04 in degradation zone");
    }

    [Fact]
    public void DegradationZone_CacheHitsReduceCostFurther()
    {
        // With 72h cache TTL, many FAQ-like queries will be served from cache (cost=$0.00).
        // If 40% of requests in degradation zone are cache hits:
        const double cacheHitRate = 0.40;

        const int inputTokens = 500;
        const int outputTokens = 300;

        var claudeCost = 3.00m * inputTokens / 1_000_000m + 15.00m * outputTokens / 1_000_000m;
        var geminiCost = 0.075m * inputTokens / 1_000_000m + 0.30m * outputTokens / 1_000_000m;

        var nonCacheWeightedAvg =
            claudeCost * PlanFeatureLimits.DegradationClaudePercent / 100m +
            geminiCost * PlanFeatureLimits.DegradationGeminiPercent / 100m;

        // Effective cost with cache hits
        var effectiveCost = nonCacheWeightedAvg * (decimal)(1.0 - cacheHitRate);

        effectiveCost.Should().BeLessThan(
            PlanFeatureLimits.DegradationZoneMaxCostPerConversation,
            "cache hits further reduce the effective cost per conversation");
    }

    // ════════════════════════════════════════════════════════════════════════
    // OVERAGE VS DEGRADATION — Correct Zone Identification
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void OverageConversations_AreNotInDegradationZone()
    {
        // Conversations 2,001+ are overage at $0.08 each, NOT in degradation zone
        PlanFeatureLimits.IsInDegradationZone("elite", 2001).Should().BeFalse();
        PlanFeatureLimits.IsInDegradationZone("elite", 3000).Should().BeFalse();
    }

    [Fact]
    public void DegradationZone_OnlyAppliesTo80To100Percent()
    {
        // Normal usage (0–80%) is NOT in degradation zone
        for (var i = 0; i < 1600; i += 200)
        {
            PlanFeatureLimits.IsInDegradationZone("elite", i).Should().BeFalse(
                $"count {i} is below 80% and should not be in degradation zone");
        }

        // Degradation zone (80%–100%) IS in degradation zone
        for (var i = 1600; i <= 2000; i += 50)
        {
            PlanFeatureLimits.IsInDegradationZone("elite", i).Should().BeTrue(
                $"count {i} is in the 80%-100% range and should be in degradation zone");
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // COST PER CONVERSATION — Calculation Verification
    // ════════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(100.00, 2000, 0.05)]    // $100 / 2000 = $0.05
    [InlineData(80.00, 2000, 0.04)]     // $80 / 2000 = $0.04 (at target)
    [InlineData(60.00, 2000, 0.03)]     // $60 / 2000 = $0.03 (below target)
    [InlineData(0.00, 2000, 0.00)]      // No cost = $0.00 per conversation
    public void CostPerConversation_CalculatesCorrectly(
        decimal totalCost, int conversationCount, decimal expectedCostPerConv)
    {
        var result = conversationCount > 0 ? totalCost / conversationCount : 0m;
        result.Should().Be(expectedCostPerConv);
    }

    [Fact]
    public void DegradationZoneMaxCost_IsHalfOfOverageCost()
    {
        // $0.04 = $0.08 / 2 — degradation zone target is half the overage rate
        // This ensures marginal profitability in the degradation zone.
        var halfOverage = PlanFeatureLimits.OverageCostPerConversation / 2;
        PlanFeatureLimits.DegradationZoneMaxCostPerConversation
            .Should().Be(halfOverage, "degradation zone cost should be half the overage rate for profitability");
    }

    // ════════════════════════════════════════════════════════════════════════
    // CONFIGURATION — CostAlertOptions Degradation Settings
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void CostAlertOptions_DegradationDefaults_MatchPlanFeatureLimits()
    {
        var options = new CarDealer.Shared.LlmGateway.Configuration.CostAlertOptions();

        options.DegradationModeClaudePercent.Should().Be(PlanFeatureLimits.DegradationClaudePercent);
        options.DegradationModeGeminiPercent.Should().Be(PlanFeatureLimits.DegradationGeminiPercent);
        options.DegradationModeLlamaPercent.Should().Be(PlanFeatureLimits.DegradationLlamaPercent);
        options.DegradationModeCacheTtl.Should().Be(PlanFeatureLimits.DegradationZoneCacheTtl);
    }

    [Fact]
    public void CostAlertOptions_DegradationTrafficSplit_SumsTo100()
    {
        var options = new CarDealer.Shared.LlmGateway.Configuration.CostAlertOptions();
        var total = options.DegradationModeClaudePercent
                  + options.DegradationModeGeminiPercent
                  + options.DegradationModeLlamaPercent;
        total.Should().Be(100, "degradation mode traffic split must sum to 100%");
    }
}

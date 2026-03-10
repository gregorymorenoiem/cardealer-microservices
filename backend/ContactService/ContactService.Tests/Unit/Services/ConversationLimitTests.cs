using CarDealer.Contracts.Enums;

namespace ContactService.Tests.Unit.Services;

/// <summary>
/// Tests for PlanFeatureLimits conversation limit enforcement — CONTRA #5 fix.
/// Validates the hard limit of 2,000 conversations for ÉLITE plan.
/// </summary>
public class ConversationLimitTests
{
    // ═══════════════════════════════════════════════════════════════════
    // Elite plan: Hard limit of 2,000 conversations
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ElitePlan_ChatAgentMonthlyMessages_ShouldBe2000()
    {
        // CONTRA #5: Elite plan must NOT be unlimited (-1)
        var limits = PlanFeatureLimits.EliteLimits;
        limits.ChatAgentMonthlyMessages.Should().Be(2000,
            "CONTRA #5: Elite plan must have a hard limit of 2,000 conversations/month to prevent negative margin");
    }

    [Fact]
    public void ElitePlan_ShouldNotBeUnlimited()
    {
        var limits = PlanFeatureLimits.EliteLimits;
        limits.ChatAgentMonthlyMessages.Should().BeGreaterThan(0,
            "Elite plan ChatAgent must have a positive limit, not unlimited (-1) or no access (0)");
    }

    // ═══════════════════════════════════════════════════════════════════
    // Warning threshold at 80% (1,600 for Elite)
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void GetConversationWarningCount_Elite_ShouldReturn1600()
    {
        var warningCount = PlanFeatureLimits.GetConversationWarningCount("elite");
        warningCount.Should().Be(1600,
            "80% of 2,000 = 1,600 conversations triggers warning notification");
    }

    [Fact]
    public void GetConversationWarningCount_Pro_ShouldReturn400()
    {
        var warningCount = PlanFeatureLimits.GetConversationWarningCount("pro");
        warningCount.Should().Be(400,
            "80% of 500 = 400 conversations triggers warning notification for Pro");
    }

    [Fact]
    public void GetConversationWarningCount_Libre_ShouldReturnNegative()
    {
        var warningCount = PlanFeatureLimits.GetConversationWarningCount("libre");
        warningCount.Should().Be(-1,
            "Libre plan has no ChatAgent access, so no warning threshold");
    }

    // ═══════════════════════════════════════════════════════════════════
    // Conversation usage status
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void GetConversationStatus_Elite_Below80Percent_ShouldBeNormal()
    {
        var status = PlanFeatureLimits.GetConversationStatus("elite", 1000);
        status.Should().Be(ConversationUsageStatus.Normal);
    }

    [Fact]
    public void GetConversationStatus_Elite_At80Percent_ShouldBeWarning()
    {
        var status = PlanFeatureLimits.GetConversationStatus("elite", 1600);
        status.Should().Be(ConversationUsageStatus.WarningThreshold);
    }

    [Fact]
    public void GetConversationStatus_Elite_Between80And100_ShouldBeWarning()
    {
        var status = PlanFeatureLimits.GetConversationStatus("elite", 1800);
        status.Should().Be(ConversationUsageStatus.WarningThreshold);
    }

    [Fact]
    public void GetConversationStatus_Elite_At100Percent_ShouldBeLimitReached()
    {
        var status = PlanFeatureLimits.GetConversationStatus("elite", 2000);
        status.Should().Be(ConversationUsageStatus.LimitReached);
    }

    [Fact]
    public void GetConversationStatus_Elite_Over100Percent_ShouldBeLimitReached()
    {
        var status = PlanFeatureLimits.GetConversationStatus("elite", 2500);
        status.Should().Be(ConversationUsageStatus.LimitReached);
    }

    [Fact]
    public void GetConversationStatus_Libre_ShouldBeNoAccess()
    {
        var status = PlanFeatureLimits.GetConversationStatus("libre", 0);
        status.Should().Be(ConversationUsageStatus.NoAccess);
    }

    // ═══════════════════════════════════════════════════════════════════
    // Overage constants
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void OverageCostPerConversation_ShouldBe008()
    {
        PlanFeatureLimits.OverageCostPerConversation.Should().Be(0.08m,
            "Overage cost per conversation beyond hard limit must be $0.08");
    }

    [Fact]
    public void ConversationWarningThreshold_ShouldBe80Percent()
    {
        PlanFeatureLimits.ConversationWarningThreshold.Should().Be(0.80,
            "Warning threshold must be 80% of the monthly limit");
    }

    [Fact]
    public void EliteCostAlertThreshold_ShouldBe17910()
    {
        PlanFeatureLimits.EliteCostAlertThreshold.Should().Be(179.10m,
            "Internal cost alert should fire at 90% of Elite plan revenue ($199 × 0.90)");
    }

    // ═══════════════════════════════════════════════════════════════════
    // IsWithinLimit integration check
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void IsWithinLimit_Elite_ChatAgent_Below2000_ShouldReturnTrue()
    {
        var withinLimit = PlanFeatureLimits.IsWithinLimit("elite", "chat_agent_monthly", 1999);
        withinLimit.Should().BeTrue();
    }

    [Fact]
    public void IsWithinLimit_Elite_ChatAgent_At2000_ShouldReturnFalse()
    {
        var withinLimit = PlanFeatureLimits.IsWithinLimit("elite", "chat_agent_monthly", 2000);
        withinLimit.Should().BeFalse();
    }

    [Fact]
    public void IsWithinLimit_Elite_ChatAgent_Over2000_ShouldReturnFalse()
    {
        var withinLimit = PlanFeatureLimits.IsWithinLimit("elite", "chat_agent_monthly", 2500);
        withinLimit.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════
    // Other plans should remain unchanged
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ProPlan_ChatAgentMonthlyMessages_ShouldBe500()
    {
        PlanFeatureLimits.ProLimits.ChatAgentMonthlyMessages.Should().Be(500);
    }

    [Fact]
    public void VisiblePlan_ChatAgentMonthlyMessages_ShouldBe0()
    {
        PlanFeatureLimits.VisibleLimits.ChatAgentMonthlyMessages.Should().Be(0);
    }

    [Fact]
    public void LibrePlan_ChatAgentMonthlyMessages_ShouldBe0()
    {
        PlanFeatureLimits.LibreLimits.ChatAgentMonthlyMessages.Should().Be(0);
    }

    [Fact]
    public void ElitePlan_RecosAgent_ShouldRemainUnlimited()
    {
        PlanFeatureLimits.EliteLimits.RecosAgentMonthlyMessages.Should().Be(-1,
            "RecosAgent should remain unlimited for Elite plan");
    }
}

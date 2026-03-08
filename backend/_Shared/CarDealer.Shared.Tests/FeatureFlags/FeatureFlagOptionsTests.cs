using CarDealer.Shared.FeatureFlags.Configuration;
using CarDealer.Shared.FeatureFlags.Models;
using FluentAssertions;

namespace CarDealer.Shared.Tests.FeatureFlags;

public class FeatureFlagOptionsTests
{
    // ── SectionName ──────────────────────────────────────────────────
    [Fact]
    public void SectionName_ShouldBe_FeatureFlags()
    {
        FeatureFlagOptions.SectionName.Should().Be("FeatureFlags");
    }

    // ── Default values ───────────────────────────────────────────────
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var opts = new FeatureFlagOptions();

        opts.ServiceUrl.Should().Be("http://featuretoggleservice");
        opts.CacheTimeSeconds.Should().Be(60);
        opts.PollingIntervalSeconds.Should().Be(30);
        opts.EnableCache.Should().BeTrue();
        opts.EnablePolling.Should().BeTrue();
        opts.HttpTimeoutSeconds.Should().Be(10);
        opts.Environment.Should().Be("Development");
        opts.DefaultValueOnError.Should().BeFalse();
    }
}

public class FeatureFlagModelsTests
{
    // ── FeatureFlagDto defaults ──────────────────────────────────────
    [Fact]
    public void FeatureFlagDto_DefaultValues_ShouldBeCorrect()
    {
        var dto = new FeatureFlagDto();

        dto.Id.Should().Be(Guid.Empty);
        dto.Key.Should().BeEmpty();
        dto.Name.Should().BeEmpty();
        dto.Description.Should().BeNull();
        dto.IsEnabled.Should().BeFalse();
        dto.Environment.Should().BeNull();
        dto.RolloutPercentage.Should().BeNull();
        dto.TargetUsers.Should().BeNull();
        dto.TargetGroups.Should().BeNull();
        dto.Metadata.Should().BeNull();
        dto.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void FeatureFlagDto_ShouldStoreValues()
    {
        var id = Guid.NewGuid();
        var dto = new FeatureFlagDto
        {
            Id = id,
            Key = "dark-mode",
            Name = "Dark Mode",
            Description = "Enable dark mode UI",
            IsEnabled = true,
            Environment = "Production",
            RolloutPercentage = 50,
            TargetUsers = new List<string> { "user-1" },
            TargetGroups = new List<string> { "beta" },
        };

        dto.Id.Should().Be(id);
        dto.Key.Should().Be("dark-mode");
        dto.IsEnabled.Should().BeTrue();
        dto.RolloutPercentage.Should().Be(50);
        dto.TargetUsers.Should().ContainSingle("user-1");
        dto.TargetGroups.Should().ContainSingle("beta");
    }

    // ── FeatureFlagEvaluationResult ──────────────────────────────────
    [Fact]
    public void FeatureFlagEvaluationResult_DefaultValues_ShouldBeCorrect()
    {
        var result = new FeatureFlagEvaluationResult();

        result.Key.Should().BeEmpty();
        result.IsEnabled.Should().BeFalse();
        result.Variant.Should().BeNull();
        result.Reason.Should().Be("default");
        result.EvaluatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void FeatureFlagEvaluationResult_ShouldStoreValues()
    {
        var result = new FeatureFlagEvaluationResult
        {
            Key = "beta-feature",
            IsEnabled = true,
            Variant = "control",
            Reason = "user_targeted"
        };

        result.Key.Should().Be("beta-feature");
        result.IsEnabled.Should().BeTrue();
        result.Variant.Should().Be("control");
        result.Reason.Should().Be("user_targeted");
    }

    // ── FeatureFlagContext ───────────────────────────────────────────
    [Fact]
    public void FeatureFlagContext_DefaultValues_ShouldBeNull()
    {
        var ctx = new FeatureFlagContext();

        ctx.UserId.Should().BeNull();
        ctx.UserEmail.Should().BeNull();
        ctx.UserGroups.Should().BeNull();
        ctx.Environment.Should().BeNull();
        ctx.CustomAttributes.Should().BeNull();
    }

    [Fact]
    public void FeatureFlagContext_ShouldStoreValues()
    {
        var ctx = new FeatureFlagContext
        {
            UserId = "u-123",
            UserEmail = "test@example.com",
            UserGroups = new List<string> { "admins", "beta" },
            Environment = "Production",
            CustomAttributes = new Dictionary<string, object> { { "plan", "pro" } }
        };

        ctx.UserId.Should().Be("u-123");
        ctx.UserGroups.Should().HaveCount(2);
        ctx.CustomAttributes.Should().ContainKey("plan");
    }
}

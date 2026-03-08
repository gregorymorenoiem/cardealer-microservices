using CarDealer.Shared.Resilience.Profiles;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Resilience;

public class ResilienceProfilesTests
{
    [Fact]
    public void Critical_HasStrictFailureRatio()
    {
        var profile = ResilienceProfiles.Critical;

        profile.CircuitBreaker.FailureRatio.Should().Be(0.3);
        profile.CircuitBreaker.MinimumThroughput.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Critical_HasShortTimeout()
    {
        var profile = ResilienceProfiles.Critical;

        profile.Timeout.TimeoutSeconds.Should().BeLessThanOrEqualTo(15);
        profile.Retry.MaxRetries.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public void Critical_HasHighParallelism()
    {
        var profile = ResilienceProfiles.Critical;

profile.Retry.MaxRetries.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public void Standard_HasModerateSettings()
    {
        var profile = ResilienceProfiles.Standard;

        profile.CircuitBreaker.FailureRatio.Should().Be(0.5);
        profile.Retry.MaxRetries.Should().Be(3);
        profile.Timeout.TimeoutSeconds.Should().Be(30);
    }

    [Fact]
    public void Background_IsMoreLenient()
    {
        var profile = ResilienceProfiles.Background;

        profile.CircuitBreaker.FailureRatio.Should().BeGreaterThan(0.5);
        profile.Retry.MaxRetries.Should().BeGreaterThanOrEqualTo(5);
        profile.Timeout.TimeoutSeconds.Should().BeGreaterThanOrEqualTo(60);
    }

    [Fact]
    public void MediaUpload_HasHighTimeout()
    {
        var profile = ResilienceProfiles.MediaUpload;

        profile.Timeout.TimeoutSeconds.Should().BeGreaterThanOrEqualTo(120);
        profile.Retry.MaxRetries.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public void InternalService_HasFastTimeout()
    {
        var profile = ResilienceProfiles.InternalService;

        profile.Timeout.TimeoutSeconds.Should().BeLessThanOrEqualTo(5);
        profile.Bulkhead.MaxParallelization.Should().BeGreaterThanOrEqualTo(30);
    }

    [Theory]
    [InlineData("critical")]
    [InlineData("standard")]
    [InlineData("background")]
    [InlineData("mediaupload")]
    [InlineData("internalservice")]
    public void GetProfile_ReturnsProfile_ForValidName(string name)
    {
        var profile = ResilienceProfiles.GetProfile(name);

        profile.Should().NotBeNull();
        profile.Retry.Should().NotBeNull();
        profile.CircuitBreaker.Should().NotBeNull();
        profile.Timeout.Should().NotBeNull();
        profile.Bulkhead.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Critical")]
    [InlineData("STANDARD")]
    [InlineData("Background")]
    public void GetProfile_IsCaseInsensitive(string name)
    {
        var profile = ResilienceProfiles.GetProfile(name);

        profile.Should().NotBeNull();
    }

    [Fact]
    public void GetProfile_ReturnsStandard_ForUnknownName()
    {
        var profile = ResilienceProfiles.GetProfile("nonexistent-profile");

        // Should return Standard as default
        profile.Should().NotBeNull();
        profile.Retry.MaxRetries.Should().Be(ResilienceProfiles.Standard.Retry.MaxRetries);
    }
}

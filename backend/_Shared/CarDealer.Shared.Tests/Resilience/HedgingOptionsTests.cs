using CarDealer.Shared.Resilience.Configuration;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Resilience;

public class HedgingOptionsTests
{
    [Fact]
    public void HedgingOptions_DefaultValues_ShouldBeCorrect()
    {
        var options = new HedgingOptions();

        options.Enabled.Should().BeFalse();
        options.MaxHedgedAttempts.Should().Be(2);
        options.DelayMilliseconds.Should().Be(2000);
    }

    [Fact]
    public void HedgingOptions_CustomValues_ShouldBeRetained()
    {
        var options = new HedgingOptions
        {
            Enabled = true,
            MaxHedgedAttempts = 5,
            DelayMilliseconds = 500
        };

        options.Enabled.Should().BeTrue();
        options.MaxHedgedAttempts.Should().Be(5);
        options.DelayMilliseconds.Should().Be(500);
    }
}

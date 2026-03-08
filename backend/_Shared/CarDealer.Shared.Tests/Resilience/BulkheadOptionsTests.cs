using CarDealer.Shared.Resilience.Configuration;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Resilience;

public class BulkheadOptionsTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var options = new BulkheadOptions();

        options.Enabled.Should().BeTrue();
        options.MaxParallelization.Should().Be(25);
        options.MaxQueuingActions.Should().Be(50);
    }

    [Fact]
    public void CanSetCustomValues()
    {
        var options = new BulkheadOptions
        {
            Enabled = false,
            MaxParallelization = 100,
            MaxQueuingActions = 200
        };

        options.Enabled.Should().BeFalse();
        options.MaxParallelization.Should().Be(100);
        options.MaxQueuingActions.Should().Be(200);
    }
}

using CarDealer.Shared.Database;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Database;

public class DatabaseConfigurationTests
{
    [Fact]
    public void DatabaseConfiguration_DefaultValues_ShouldBeCorrect()
    {
        var config = new DatabaseConfiguration();

        config.Provider.Should().Be(DatabaseProvider.PostgreSQL);
        config.ConnectionStrings.Should().NotBeNull().And.BeEmpty();
        config.AutoMigrate.Should().BeFalse();
        config.CommandTimeout.Should().Be(30);
        config.MaxRetryCount.Should().Be(3);
        config.MaxRetryDelay.Should().Be(30);
        config.EnableSensitiveDataLogging.Should().BeFalse();
        config.EnableDetailedErrors.Should().BeFalse();
    }

    [Fact]
    public void GetConnectionString_WithMatchingProvider_ShouldReturnString()
    {
        var config = new DatabaseConfiguration
        {
            Provider = DatabaseProvider.PostgreSQL,
            ConnectionStrings = new Dictionary<string, string>
            {
                { "PostgreSQL", "Host=localhost;Database=test" }
            }
        };

        var result = config.GetConnectionString();

        result.Should().Be("Host=localhost;Database=test");
    }

    [Fact]
    public void GetConnectionString_WithMissingProvider_ShouldThrow()
    {
        var config = new DatabaseConfiguration
        {
            Provider = DatabaseProvider.PostgreSQL,
            ConnectionStrings = new Dictionary<string, string>()
        };

        var act = () => config.GetConnectionString();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*PostgreSQL*");
    }

    [Fact]
    public void GetConnectionString_WithSqlServer_ShouldReturnCorrectString()
    {
        var config = new DatabaseConfiguration
        {
            Provider = DatabaseProvider.SqlServer,
            ConnectionStrings = new Dictionary<string, string>
            {
                { "SqlServer", "Server=localhost;Database=test" },
                { "PostgreSQL", "Host=localhost;Database=other" }
            }
        };

        var result = config.GetConnectionString();

        result.Should().Be("Server=localhost;Database=test");
    }

    [Fact]
    public void DatabaseConfiguration_CustomValues_ShouldBeRetained()
    {
        var config = new DatabaseConfiguration
        {
            Provider = DatabaseProvider.MySQL,
            AutoMigrate = true,
            CommandTimeout = 60,
            MaxRetryCount = 5,
            MaxRetryDelay = 60,
            EnableSensitiveDataLogging = true,
            EnableDetailedErrors = true
        };

        config.Provider.Should().Be(DatabaseProvider.MySQL);
        config.AutoMigrate.Should().BeTrue();
        config.CommandTimeout.Should().Be(60);
        config.MaxRetryCount.Should().Be(5);
        config.MaxRetryDelay.Should().Be(60);
        config.EnableSensitiveDataLogging.Should().BeTrue();
        config.EnableDetailedErrors.Should().BeTrue();
    }
}

public class DatabaseProviderTests
{
    [Fact]
    public void DatabaseProvider_ShouldHaveExpectedValues()
    {
        Enum.GetValues<DatabaseProvider>().Should().HaveCount(5);
        Enum.IsDefined(DatabaseProvider.PostgreSQL).Should().BeTrue();
        Enum.IsDefined(DatabaseProvider.SqlServer).Should().BeTrue();
        Enum.IsDefined(DatabaseProvider.MySQL).Should().BeTrue();
        Enum.IsDefined(DatabaseProvider.Oracle).Should().BeTrue();
        Enum.IsDefined(DatabaseProvider.InMemory).Should().BeTrue();
    }

    [Theory]
    [InlineData(DatabaseProvider.PostgreSQL, 0)]
    [InlineData(DatabaseProvider.SqlServer, 1)]
    [InlineData(DatabaseProvider.MySQL, 2)]
    [InlineData(DatabaseProvider.Oracle, 3)]
    [InlineData(DatabaseProvider.InMemory, 4)]
    public void DatabaseProvider_EnumValues_ShouldHaveCorrectOrdinals(DatabaseProvider provider, int expected)
    {
        ((int)provider).Should().Be(expected);
    }
}

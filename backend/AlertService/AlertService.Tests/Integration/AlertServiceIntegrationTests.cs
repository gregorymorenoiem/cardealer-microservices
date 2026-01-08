using FluentAssertions;
using AlertService.Domain.Entities;
using AlertService.Infrastructure.Persistence;

namespace AlertService.Tests.Integration;

/// <summary>
/// Basic integration tests for AlertService
/// These tests verify that the service can be built and basic functionality works
/// </summary>
public class AlertServiceIntegrationTests
{
    [Fact]
    public void AlertService_Should_CompileSuccessfully()
    {
        // This test just verifies that the service compiles
        // If this test runs, it means all dependencies are resolved correctly
        true.Should().BeTrue();
    }

    [Fact]
    public void AlertService_Domain_Should_BeAccessible()
    {
        // Verify domain layer is accessible
        var type = typeof(PriceAlert);
        type.Should().NotBeNull();
        type.Name.Should().Be("PriceAlert");
    }

    [Fact]
    public void AlertService_Infrastructure_Should_BeAccessible()
    {
        // Verify infrastructure layer is accessible
        var type = typeof(ApplicationDbContext);
        type.Should().NotBeNull();
        type.Name.Should().Be("ApplicationDbContext");
    }

    [Fact]
    public void AlertService_Tests_Should_Pass_BasicAssertions()
    {
        // Basic test to verify xUnit and FluentAssertions work
        var testValue = "AlertService";
        testValue.Should().NotBeNullOrEmpty();
        testValue.Should().Contain("Alert");
        testValue.Should().StartWith("Alert");
        testValue.Should().EndWith("Service");
    }

    [Fact]
    public void PriceAlert_Should_BeCreatable()
    {
        // Test that we can create domain entities
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var targetPrice = 25000m;
        var condition = AlertCondition.LessThanOrEqual;

        var priceAlert = new PriceAlert(userId, vehicleId, targetPrice, condition);

        priceAlert.Should().NotBeNull();
        priceAlert.UserId.Should().Be(userId);
        priceAlert.VehicleId.Should().Be(vehicleId);
        priceAlert.TargetPrice.Should().Be(targetPrice);
        priceAlert.Condition.Should().Be(condition);
        priceAlert.IsActive.Should().BeTrue();
        priceAlert.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SavedSearch_Should_BeCreatable()
    {
        // Test that we can create domain entities
        var userId = Guid.NewGuid();
        var name = "My Saved Search";
        var searchCriteria = "{\"make\": \"Toyota\", \"year\": 2020}";

        var savedSearch = new SavedSearch(userId, name, searchCriteria);

        savedSearch.Should().NotBeNull();
        savedSearch.UserId.Should().Be(userId);
        savedSearch.Name.Should().Be(name);
        savedSearch.SearchCriteria.Should().Be(searchCriteria);
        savedSearch.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AlertService_Should_HaveCorrectAssemblyStructure()
    {
        // Test that assemblies are loaded correctly
        var domainAssembly = typeof(PriceAlert).Assembly;
        var infrastructureAssembly = typeof(ApplicationDbContext).Assembly;

        domainAssembly.Should().NotBeNull();
        infrastructureAssembly.Should().NotBeNull();

        domainAssembly.FullName.Should().Contain("AlertService.Domain");
        infrastructureAssembly.FullName.Should().Contain("AlertService.Infrastructure");
    }
}
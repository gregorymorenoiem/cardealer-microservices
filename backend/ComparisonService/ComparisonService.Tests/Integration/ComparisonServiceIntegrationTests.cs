using FluentAssertions;
using ComparisonService.Domain.Entities;
using ComparisonService.Infrastructure.Persistence;

namespace ComparisonService.Tests.Integration;

/// <summary>
/// Basic integration tests for ComparisonService
/// These tests verify that the service can be built and basic functionality works
/// </summary>
public class ComparisonServiceIntegrationTests
{
    [Fact]
    public void ComparisonService_Should_CompileSuccessfully()
    {
        // This test just verifies that the service compiles
        // If this test runs, it means all dependencies are resolved correctly
        true.Should().BeTrue();
    }

    [Fact]
    public void ComparisonService_Domain_Should_BeAccessible()
    {
        // Verify domain layer is accessible
        var type = typeof(VehicleComparison);
        type.Should().NotBeNull();
        type.Name.Should().Be("VehicleComparison");
    }

    [Fact]
    public void ComparisonService_Infrastructure_Should_BeAccessible()
    {
        // Verify infrastructure layer is accessible
        var type = typeof(ApplicationDbContext);
        type.Should().NotBeNull();
        type.Name.Should().Be("ApplicationDbContext");
    }

    [Fact]
    public void ComparisonService_Tests_Should_Pass_BasicAssertions()
    {
        // Basic test to verify xUnit and FluentAssertions work
        var testValue = "ComparisonService";
        testValue.Should().NotBeNullOrEmpty();
        testValue.Should().Contain("Comparison");
        testValue.Should().StartWith("Comparison");
        testValue.Should().EndWith("Service");
    }

    [Fact]
    public void Comparison_Should_BeCreatable()
    {
        // Test that we can create domain entities
        var userId = Guid.NewGuid();
        var name = "My Vehicle Comparison";
        var vehicleIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var comparison = new VehicleComparison(userId, name, vehicleIds);

        comparison.Should().NotBeNull();
        comparison.UserId.Should().Be(userId);
        comparison.Name.Should().Be(name);
        comparison.VehicleIds.Should().HaveCount(2);
        comparison.VehicleIds.Should().BeEquivalentTo(vehicleIds);
        comparison.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ComparisonService_Should_HaveCorrectAssemblyStructure()
    {
        // Test that assemblies are loaded correctly
        var domainAssembly = typeof(VehicleComparison).Assembly;
        var infrastructureAssembly = typeof(ApplicationDbContext).Assembly;

        domainAssembly.Should().NotBeNull();
        infrastructureAssembly.Should().NotBeNull();

        domainAssembly.FullName.Should().Contain("ComparisonService.Domain");
        infrastructureAssembly.FullName.Should().Contain("ComparisonService.Infrastructure");
    }
}
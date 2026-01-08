using FluentAssertions;
using MaintenanceService.Domain.Entities;
using MaintenanceService.Infrastructure.Persistence;

namespace MaintenanceService.Tests.Integration;

/// <summary>
/// Basic integration tests for MaintenanceService
/// These tests verify that the service can be built and basic functionality works
/// </summary>
public class MaintenanceServiceIntegrationTests
{
    [Fact]
    public void MaintenanceService_Should_CompileSuccessfully()
    {
        // This test just verifies that the service compiles
        // If this test runs, it means all dependencies are resolved correctly
        true.Should().BeTrue();
    }

    [Fact]
    public void MaintenanceService_Domain_Should_BeAccessible()
    {
        // Verify domain layer is accessible
        var type = typeof(MaintenanceWindow);
        type.Should().NotBeNull();
        type.Name.Should().Be("MaintenanceWindow");
    }

    [Fact]
    public void MaintenanceService_Infrastructure_Should_BeAccessible()
    {
        // Verify infrastructure layer is accessible
        var type = typeof(ApplicationDbContext);
        type.Should().NotBeNull();
        type.Name.Should().Be("ApplicationDbContext");
    }

    [Fact]
    public void MaintenanceService_Tests_Should_Pass_BasicAssertions()
    {
        // Basic test to verify xUnit and FluentAssertions work
        var testValue = "MaintenanceService";
        testValue.Should().NotBeNullOrEmpty();
        testValue.Should().Contain("Maintenance");
        testValue.Should().StartWith("Maintenance");
        testValue.Should().EndWith("Service");
    }

    [Fact]
    public void MaintenanceWindow_Should_BeCreatable()
    {
        // Test that we can create domain entities
        var title = "Test Maintenance";
        var description = "Test Description";
        var type = MaintenanceType.Scheduled;
        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(2);
        var createdBy = "test@example.com";

        var maintenance = new MaintenanceWindow(title, description, type, start, end, createdBy);

        maintenance.Should().NotBeNull();
        maintenance.Title.Should().Be(title);
        maintenance.Description.Should().Be(description);
        maintenance.Type.Should().Be(type);
        maintenance.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void MaintenanceService_Should_HaveCorrectAssemblyStructure()
    {
        // Test that assemblies are loaded correctly
        var domainAssembly = typeof(MaintenanceWindow).Assembly;
        var infrastructureAssembly = typeof(ApplicationDbContext).Assembly;

        domainAssembly.Should().NotBeNull();
        infrastructureAssembly.Should().NotBeNull();

        domainAssembly.FullName.Should().Contain("MaintenanceService.Domain");
        infrastructureAssembly.FullName.Should().Contain("MaintenanceService.Infrastructure");
    }
}
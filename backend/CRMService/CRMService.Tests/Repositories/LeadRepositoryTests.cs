using CRMService.Domain.Entities;
using CRMService.Infrastructure.Persistence;
using CRMService.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using CarDealer.Shared.MultiTenancy;
using Xunit;

namespace CRMService.Tests.Repositories;

public class LeadRepositoryTests
{
    private static readonly Guid TestDealerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private CRMDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CRMDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.Setup(t => t.CurrentDealerId).Returns(TestDealerId);
        tenantContext.Setup(t => t.HasDealerContext).Returns(true);

        return new CRMDbContext(options, tenantContext.Object);
    }

    private Lead CreateTestLead(string firstName, string lastName, string email)
    {
        return new Lead(
            TestDealerId,
            firstName,
            lastName,
            email,
            LeadSource.Website
        );
    }

    [Fact]
    public async Task AddAsync_ShouldAddLead()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new LeadRepository(context);
        var lead = CreateTestLead("John", "Doe", "john.doe@test.com");

        // Act
        var result = await repository.AddAsync(lead);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        context.Leads.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnLead()
    {
        // Arrange
        using var context = CreateContext();
        var lead = CreateTestLead("Jane", "Smith", "jane.smith@test.com");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var repository = new LeadRepository(context);

        // Act
        var result = await repository.GetByIdAsync(lead.Id);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnLeadsWithStatus()
    {
        // Arrange
        using var context = CreateContext();
        var lead1 = CreateTestLead("A", "B", "a.b@test.com");
        var lead2 = CreateTestLead("C", "D", "c.d@test.com");
        lead2.UpdateStatus(LeadStatus.Qualified);
        var lead3 = CreateTestLead("E", "F", "e.f@test.com");

        context.Leads.AddRange(lead1, lead2, lead3);
        await context.SaveChangesAsync();

        var repository = new LeadRepository(context);

        // Act
        var result = await repository.GetByStatusAsync(LeadStatus.New);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingLeads()
    {
        // Arrange
        using var context = CreateContext();
        var lead1 = CreateTestLead("John", "Doe", "john.doe@test.com");
        var lead2 = CreateTestLead("Jane", "Doe", "jane.doe@test.com");
        var lead3 = CreateTestLead("Bob", "Smith", "bob.smith@test.com");

        context.Leads.AddRange(lead1, lead2, lead3);
        await context.SaveChangesAsync();

        var repository = new LeadRepository(context);

        // Act
        var result = await repository.SearchAsync("Doe");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveLead()
    {
        // Arrange
        using var context = CreateContext();
        var lead = CreateTestLead("Delete", "Me", "delete.me@test.com");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var repository = new LeadRepository(context);

        // Act
        await repository.DeleteAsync(lead.Id);

        // Assert
        context.Leads.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllLeads()
    {
        // Arrange
        using var context = CreateContext();
        var lead1 = CreateTestLead("Lead1", "Test", "lead1@test.com");
        var lead2 = CreateTestLead("Lead2", "Test", "lead2@test.com");
        var lead3 = CreateTestLead("Lead3", "Test", "lead3@test.com");

        context.Leads.AddRange(lead1, lead2, lead3);
        await context.SaveChangesAsync();

        var repository = new LeadRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateLead()
    {
        // Arrange
        using var context = CreateContext();
        var lead = CreateTestLead("Original", "Name", "original@test.com");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var repository = new LeadRepository(context);

        // Act
        lead.UpdateContactInfo("Updated", "Name", "updated@test.com", "555-1234", "Updated Company", "Manager");
        await repository.UpdateAsync(lead);

        // Assert
        var updated = await context.Leads.FirstOrDefaultAsync(l => l.Id == lead.Id);
        updated.Should().NotBeNull();
        updated!.FirstName.Should().Be("Updated");
        updated.Email.Should().Be("updated@test.com");
    }

    [Fact]
    public async Task GetByAssignedUserAsync_ShouldReturnAssignedLeads()
    {
        // Arrange
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var lead1 = CreateTestLead("Assigned1", "User", "assigned1@test.com");
        lead1.AssignTo(userId);
        var lead2 = CreateTestLead("Unassigned", "User", "unassigned@test.com");
        var lead3 = CreateTestLead("Assigned2", "User", "assigned2@test.com");
        lead3.AssignTo(userId);

        context.Leads.AddRange(lead1, lead2, lead3);
        await context.SaveChangesAsync();

        var repository = new LeadRepository(context);

        // Act
        var result = await repository.GetByAssignedUserAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(l => l.AssignedToUserId.Should().Be(userId));
    }
}

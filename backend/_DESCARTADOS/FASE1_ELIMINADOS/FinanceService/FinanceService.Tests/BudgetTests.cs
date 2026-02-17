using FluentAssertions;
using FinanceService.Domain.Entities;
using Xunit;

namespace FinanceService.Tests;

public class BudgetTests
{
    [Fact]
    public void Create_Budget_Should_Set_Properties_Correctly()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var name = "Marketing Budget";
        var period = BudgetPeriod.Monthly;
        var year = 2024;
        var totalBudget = 50000m;
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        var createdBy = Guid.NewGuid();

        // Act
        var budget = new Budget(dealerId, name, period, year, totalBudget, startDate, endDate, createdBy, 1);

        // Assert
        budget.DealerId.Should().Be(dealerId);
        budget.Name.Should().Be(name);
        budget.Period.Should().Be(period);
        budget.Year.Should().Be(year);
        budget.TotalBudget.Should().Be(totalBudget);
        budget.TotalSpent.Should().Be(0);
        budget.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RecordSpending_Should_Increase_TotalSpent()
    {
        // Arrange
        var budget = CreateTestBudget();
        var spending = 1000m;

        // Act
        budget.RecordSpending(spending);

        // Assert
        budget.TotalSpent.Should().Be(spending);
        budget.Remaining.Should().Be(49000m);
    }

    [Fact]
    public void Update_Budget_Should_Change_Name_And_TotalBudget()
    {
        // Arrange
        var budget = CreateTestBudget();
        var newName = "Updated Budget";
        var newDescription = "Updated Description";
        var newTotal = 60000m;

        // Act
        budget.Update(newName, newDescription, newTotal);

        // Assert
        budget.Name.Should().Be(newName);
        budget.Description.Should().Be(newDescription);
        budget.TotalBudget.Should().Be(newTotal);
    }

    [Fact]
    public void Deactivate_Budget_Should_Set_IsActive_To_False()
    {
        // Arrange
        var budget = CreateTestBudget();

        // Act
        budget.Deactivate();

        // Assert
        budget.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsOverBudget_Should_Return_True_When_Spent_Exceeds_Budget()
    {
        // Arrange
        var budget = CreateTestBudget();
        budget.RecordSpending(51000m);

        // Act & Assert
        budget.IsOverBudget.Should().BeTrue();
    }

    private static Budget CreateTestBudget()
    {
        return new Budget(
            Guid.NewGuid(),
            "Test Budget",
            BudgetPeriod.Monthly,
            2024,
            50000m,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 31),
            Guid.NewGuid(),
            1
        );
    }
}

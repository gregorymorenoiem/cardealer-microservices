using FluentAssertions;
using FinanceService.Domain.Entities;
using Xunit;

namespace FinanceService.Tests;

public class ExpenseTests
{
    [Fact]
    public void Create_Expense_Should_Set_Properties_Correctly()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var expenseNumber = "EXP-001";
        var category = ExpenseCategory.Utilities;
        var description = "Electric bill";
        var amount = 500m;
        var currency = "MXN";
        var expenseDate = DateTime.UtcNow;
        var createdBy = Guid.NewGuid();

        // Act
        var expense = new Expense(dealerId, expenseNumber, category, description, amount, currency, expenseDate, createdBy);

        // Assert
        expense.DealerId.Should().Be(dealerId);
        expense.ExpenseNumber.Should().Be(expenseNumber);
        expense.Category.Should().Be(category);
        expense.Description.Should().Be(description);
        expense.Amount.Should().Be(amount);
        expense.Status.Should().Be(ExpenseStatus.Draft);
    }

    [Fact]
    public void Submit_Expense_Should_Change_Status_To_Submitted()
    {
        // Arrange
        var expense = CreateTestExpense();

        // Act
        expense.Submit();

        // Assert
        expense.Status.Should().Be(ExpenseStatus.Submitted);
    }

    [Fact]
    public void Approve_Expense_Should_Change_Status_To_Approved()
    {
        // Arrange
        var expense = CreateTestExpense();
        expense.Submit();
        var approverId = Guid.NewGuid();

        // Act
        expense.Approve(approverId);

        // Assert
        expense.Status.Should().Be(ExpenseStatus.Approved);
        expense.ApprovedBy.Should().Be(approverId);
        expense.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_Expense_Should_Change_Status_To_Rejected()
    {
        // Arrange
        var expense = CreateTestExpense();
        expense.Submit();

        // Act
        expense.Reject("Budget exceeded");

        // Assert
        expense.Status.Should().Be(ExpenseStatus.Rejected);
    }

    [Fact]
    public void MarkAsPaid_Should_Change_Status_To_Paid()
    {
        // Arrange
        var expense = CreateTestExpense();
        expense.Submit();
        expense.Approve(Guid.NewGuid());
        var paidDate = DateTime.UtcNow;

        // Act
        expense.MarkAsPaid(paidDate);

        // Assert
        expense.Status.Should().Be(ExpenseStatus.Paid);
        expense.PaidDate.Should().Be(paidDate);
    }

    private static Expense CreateTestExpense()
    {
        return new Expense(
            Guid.NewGuid(),
            "EXP-001",
            ExpenseCategory.Utilities,
            "Test expense",
            500m,
            "MXN",
            DateTime.UtcNow,
            Guid.NewGuid()
        );
    }
}

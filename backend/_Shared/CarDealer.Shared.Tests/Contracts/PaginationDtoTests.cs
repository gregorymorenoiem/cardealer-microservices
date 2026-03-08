using CarDealer.Contracts.DTOs.Common;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Contracts;

public class PaginationDtoTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var pagination = new PaginationDto();

        pagination.PageNumber.Should().Be(1);
        pagination.PageSize.Should().Be(10);
        pagination.TotalItems.Should().Be(0);
    }

    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        var pagination = new PaginationDto { TotalItems = 25, PageSize = 10 };
        pagination.TotalPages.Should().Be(3);
    }

    [Fact]
    public void TotalPages_ExactDivision_ShouldNotAddExtraPage()
    {
        var pagination = new PaginationDto { TotalItems = 20, PageSize = 10 };
        pagination.TotalPages.Should().Be(2);
    }

    [Fact]
    public void TotalPages_ZeroItems_ShouldBeZero()
    {
        var pagination = new PaginationDto { TotalItems = 0, PageSize = 10 };
        pagination.TotalPages.Should().Be(0);
    }

    [Fact]
    public void TotalPages_SingleItem_ShouldBeOne()
    {
        var pagination = new PaginationDto { TotalItems = 1, PageSize = 10 };
        pagination.TotalPages.Should().Be(1);
    }

    [Fact]
    public void HasPreviousPage_FirstPage_ShouldBeFalse()
    {
        var pagination = new PaginationDto { PageNumber = 1 };
        pagination.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_SecondPage_ShouldBeTrue()
    {
        var pagination = new PaginationDto { PageNumber = 2 };
        pagination.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_LastPage_ShouldBeFalse()
    {
        var pagination = new PaginationDto { PageNumber = 3, TotalItems = 30, PageSize = 10 };
        pagination.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_MiddlePage_ShouldBeTrue()
    {
        var pagination = new PaginationDto { PageNumber = 2, TotalItems = 30, PageSize = 10 };
        pagination.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_PageBeyondTotal_ShouldBeFalse()
    {
        var pagination = new PaginationDto { PageNumber = 5, TotalItems = 30, PageSize = 10 };
        pagination.HasNextPage.Should().BeFalse();
    }
}

using CarDealer.Contracts.DTOs.Common;
using FluentAssertions;

namespace CarDealer.Contracts.Tests.DTOs;

public class CommonDtosTests
{
    [Fact]
    public void PaginationDto_Should_Calculate_TotalPages_Correctly()
    {
        // Arrange
        var pagination = new PaginationDto
        {
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 95
        };

        // Act & Assert
        pagination.TotalPages.Should().Be(10); // 95 / 10 = 9.5 -> 10 pages
    }

    [Fact]
    public void PaginationDto_Should_Calculate_HasNextPage_Correctly()
    {
        // Arrange
        var pagination = new PaginationDto
        {
            PageNumber = 5,
            PageSize = 10,
            TotalItems = 100
        };

        // Act & Assert
        pagination.HasNextPage.Should().BeTrue(); // Page 5 of 10
        pagination.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void PaginationDto_Should_Handle_Last_Page()
    {
        // Arrange
        var pagination = new PaginationDto
        {
            PageNumber = 10,
            PageSize = 10,
            TotalItems = 100
        };

        // Act & Assert
        pagination.HasNextPage.Should().BeFalse(); // Last page
        pagination.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void ApiResponse_Should_Create_Success_Response()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };

        // Act
        var response = ApiResponse<object>.SuccessResponse(data, "Operation successful");

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().Be("Operation successful");
        response.Error.Should().BeNull();
        response.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ApiResponse_Should_Create_Error_Response()
    {
        // Arrange
        var errorDetails = new ErrorDetailsDto
        {
            ErrorCode = "ERR_001",
            ErrorMessage = "Something went wrong",
            StatusCode = 500
        };

        // Act
        var response = ApiResponse<object>.ErrorResponse("Operation failed", errorDetails);

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be("Operation failed");
        response.Error.Should().NotBeNull();
        response.Error!.ErrorCode.Should().Be("ERR_001");
        response.Error.StatusCode.Should().Be(500);
    }

    [Fact]
    public void ErrorDetailsDto_Should_Store_Validation_Errors()
    {
        // Arrange
        var errorDetails = new ErrorDetailsDto
        {
            ErrorCode = "VALIDATION_ERROR",
            ErrorMessage = "Validation failed",
            StatusCode = 400,
            ValidationErrors = new Dictionary<string, string[]>
            {
                { "Email", new[] { "Email is required", "Email format is invalid" } },
                { "Password", new[] { "Password must be at least 8 characters" } }
            }
        };

        // Act & Assert
        errorDetails.ValidationErrors.Should().NotBeNull();
        errorDetails.ValidationErrors!.Should().ContainKey("Email");
        errorDetails.ValidationErrors["Email"].Should().HaveCount(2);
        errorDetails.ValidationErrors["Password"].Should().HaveCount(1);
    }

    [Fact]
    public void ErrorDetailsDto_Should_Include_Timestamp()
    {
        // Arrange & Act
        var errorDetails = new ErrorDetailsDto
        {
            ErrorCode = "ERR_TEST",
            ErrorMessage = "Test error",
            StatusCode = 500
        };

        // Assert
        errorDetails.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}

using CarDealer.Contracts.DTOs.Common;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Contracts;

public class ApiResponseTests
{
    [Fact]
    public void SuccessResponse_ShouldSetSuccessTrue()
    {
        var response = ApiResponse<string>.SuccessResponse("data");

        response.Success.Should().BeTrue();
        response.Data.Should().Be("data");
        response.Message.Should().BeNull();
        response.Error.Should().BeNull();
    }

    [Fact]
    public void SuccessResponse_WithMessage_ShouldSetMessage()
    {
        var response = ApiResponse<int>.SuccessResponse(42, "Created");

        response.Success.Should().BeTrue();
        response.Data.Should().Be(42);
        response.Message.Should().Be("Created");
    }

    [Fact]
    public void ErrorResponse_ShouldSetSuccessFalse()
    {
        var response = ApiResponse<string>.ErrorResponse("Something went wrong");

        response.Success.Should().BeFalse();
        response.Message.Should().Be("Something went wrong");
        response.Data.Should().BeNull();
        response.Error.Should().BeNull();
    }

    [Fact]
    public void ErrorResponse_WithErrorDetails_ShouldSetError()
    {
        var error = new ErrorDetailsDto { ErrorCode = "E001", StatusCode = 400 };
        var response = ApiResponse<string>.ErrorResponse("Bad request", error);

        response.Success.Should().BeFalse();
        response.Error.Should().NotBeNull();
        response.Error!.ErrorCode.Should().Be("E001");
        response.Error.StatusCode.Should().Be(400);
    }

    [Fact]
    public void Timestamp_ShouldDefaultToUtcNow()
    {
        var before = DateTime.UtcNow;
        var response = new ApiResponse<string>();
        var after = DateTime.UtcNow;

        response.Timestamp.Should().BeOnOrAfter(before);
        response.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Default_ShouldHaveSuccessFalse()
    {
        var response = new ApiResponse<string>();

        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().BeNull();
    }

    [Fact]
    public void SuccessResponse_WithComplexType_ShouldWork()
    {
        var data = new List<int> { 1, 2, 3 };
        var response = ApiResponse<List<int>>.SuccessResponse(data);

        response.Data.Should().HaveCount(3);
        response.Data.Should().ContainInOrder(1, 2, 3);
    }
}

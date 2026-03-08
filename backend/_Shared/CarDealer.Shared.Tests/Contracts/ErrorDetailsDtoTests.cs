using CarDealer.Contracts.DTOs.Common;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Contracts;

public class ErrorDetailsDtoTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var error = new ErrorDetailsDto();

        error.ErrorCode.Should().BeEmpty();
        error.ErrorMessage.Should().BeEmpty();
        error.StatusCode.Should().Be(0);
        error.StackTrace.Should().BeNull();
        error.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void Timestamp_ShouldDefaultToUtcNow()
    {
        var before = DateTime.UtcNow;
        var error = new ErrorDetailsDto();
        var after = DateTime.UtcNow;

        error.Timestamp.Should().BeOnOrAfter(before);
        error.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void ValidationErrors_ShouldBeSettable()
    {
        var error = new ErrorDetailsDto
        {
            ValidationErrors = new Dictionary<string, string[]>
            {
                ["Email"] = ["Email is required", "Email is invalid"],
                ["Name"] = ["Name is required"]
            }
        };

        error.ValidationErrors.Should().HaveCount(2);
        error.ValidationErrors["Email"].Should().HaveCount(2);
    }

    [Fact]
    public void AllProperties_ShouldBeSettable()
    {
        var error = new ErrorDetailsDto
        {
            ErrorCode = "VALIDATION_ERROR",
            ErrorMessage = "Invalid input",
            StatusCode = 422,
            StackTrace = "at Test.Method()"
        };

        error.ErrorCode.Should().Be("VALIDATION_ERROR");
        error.ErrorMessage.Should().Be("Invalid input");
        error.StatusCode.Should().Be(422);
        error.StackTrace.Should().Be("at Test.Method()");
    }
}

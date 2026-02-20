using AdvertisingService.Application.Validators;
using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace AdvertisingService.Tests.Application;

public class SecurityValidatorTests
{
    private readonly TestValidator _validator = new();

    private class TestModel
    {
        public string Input { get; set; } = string.Empty;
    }

    private class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Input).NoSqlInjection().NoXss();
        }
    }

    [Theory]
    [InlineData("Normal text")]
    [InlineData("Hello World")]
    [InlineData("Toyota Corolla 2024")]
    [InlineData("Precio: RD$1,500,000")]
    public void Validate_WithCleanInput_ShouldPass(string input)
    {
        var model = new TestModel { Input = input };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Input);
    }

    [Theory]
    [InlineData("'; DROP TABLE users;--")]
    [InlineData("1 OR 1=1")]
    [InlineData("UNION SELECT * FROM users")]
    [InlineData("'; EXEC xp_cmdshell('dir');--")]
    public void Validate_WithSqlInjection_ShouldFail(string input)
    {
        var model = new TestModel { Input = input };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Input);
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("javascript:alert(1)")]
    [InlineData("<img onerror=alert(1)>")]
    [InlineData("<iframe src='evil.com'></iframe>")]
    public void Validate_WithXss_ShouldFail(string input)
    {
        var model = new TestModel { Input = input };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Input);
    }
}

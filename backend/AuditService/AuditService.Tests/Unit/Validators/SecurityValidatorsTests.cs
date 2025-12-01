using AuditService.Application.Validators;
using FluentAssertions;
using Xunit;

namespace AuditService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for SecurityValidators (SQL Injection and XSS detection)
/// </summary>
public class SecurityValidatorsTests
{
    #region SQL Injection Tests

    [Theory]
    [InlineData("SELECT * FROM Users")]
    [InlineData("DROP TABLE Customers")]
    [InlineData("INSERT INTO Admin VALUES")]
    [InlineData("1' OR '1'='1")]
    [InlineData("admin'--")]
    [InlineData("UNION SELECT password FROM Users")]
    [InlineData("xp_cmdshell 'dir'")]
    [InlineData("EXEC sp_executesql")]
    [InlineData("0x48656C6C6F")]
    [InlineData("char(65)")]
    public void SqlInjectionValidator_WithSqlKeywords_ShouldDetect(string maliciousInput)
    {
        // Act
        var containsSql = SqlInjectionValidator.ContainsSqlKeywords(maliciousInput);

        // Assert
        containsSql.Should().BeTrue($"{maliciousInput} should be detected as SQL injection");
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData("user@example.com")]
    [InlineData("Normal text without SQL")]
    [InlineData("12345")]
    [InlineData("This is a safe comment")]
    public void SqlInjectionValidator_WithSafeInput_ShouldNotDetect(string safeInput)
    {
        // Act
        var containsSql = SqlInjectionValidator.ContainsSqlKeywords(safeInput);

        // Assert
        containsSql.Should().BeFalse($"{safeInput} should be safe from SQL injection");
    }

    [Fact]
    public void SqlInjectionValidator_WithNullOrEmpty_ShouldReturnFalse()
    {
        // Act & Assert
        SqlInjectionValidator.ContainsSqlKeywords(null).Should().BeFalse();
        SqlInjectionValidator.ContainsSqlKeywords("").Should().BeFalse();
        SqlInjectionValidator.ContainsSqlKeywords("   ").Should().BeFalse();
    }

    #endregion

    #region XSS Basic Tests

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("<img src=x onerror=alert(1)>")]
    [InlineData("javascript:alert('XSS')")]
    [InlineData("<body onload=alert('XSS')>")]
    [InlineData("<iframe src='malicious.com'></iframe>")]
    [InlineData("<img src='javascript:alert(1)'>")]
    [InlineData("document.cookie")]
    [InlineData("eval('malicious code')")]
    public void XssValidator_WithXssPatterns_ShouldDetect(string maliciousInput)
    {
        // Act
        var containsXss = XssValidator.ContainsXssPatterns(maliciousInput);

        // Assert
        containsXss.Should().BeTrue($"{maliciousInput} should be detected as XSS");
    }

    [Theory]
    [InlineData("Normal text")]
    [InlineData("Safe HTML: <b>bold</b>")]
    [InlineData("Email: test@example.com")]
    [InlineData("URL: https://example.com")]
    public void XssValidator_WithSafeInput_ShouldNotDetect(string safeInput)
    {
        // Act
        var containsXss = XssValidator.ContainsXssPatterns(safeInput);

        // Assert
        containsXss.Should().BeFalse($"{safeInput} should be safe from XSS");
    }

    #endregion

    #region XSS Advanced Tests

    [Theory]
    [InlineData("< script>alert(1)</script>")]
    [InlineData("<   script >alert(1)</script>")]
    [InlineData("onclick = 'malicious()'")]
    [InlineData("onerror = 'alert(1)'")]
    [InlineData("javascript  : void(0)")]
    [InlineData("<  iframe src='evil.com'></iframe>")]
    [InlineData("<img  src = 'x' onerror='alert(1)'>")]
    public void XssValidator_Advanced_WithObfuscatedXss_ShouldDetect(string maliciousInput)
    {
        // Act
        var containsXss = XssValidator.ContainsXssPatternsAdvanced(maliciousInput);

        // Assert
        containsXss.Should().BeTrue($"{maliciousInput} should be detected as advanced XSS");
    }

    [Theory]
    [InlineData("Normal text")]
    [InlineData("This is a description")]
    [InlineData("Some data")]
    public void XssValidator_Advanced_WithSafeInput_ShouldNotDetect(string safeInput)
    {
        // Act
        var containsXss = XssValidator.ContainsXssPatternsAdvanced(safeInput);

        // Assert
        containsXss.Should().BeFalse($"{safeInput} should be safe from advanced XSS");
    }

    #endregion

    #region Combined Security Tests

    [Theory]
    [InlineData("SELECT * FROM Users", false)]
    [InlineData("<script>alert(1)</script>", false)]
    [InlineData("Normal safe text", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("DROP TABLE Users", false)]
    public void SecurityValidator_IsSecure_ShouldValidateCorrectly(string input, bool expectedIsSecure)
    {
        // Act
        var isSecure = SecurityValidator.IsSecure(input);

        // Assert
        isSecure.Should().Be(expectedIsSecure);
    }

    [Fact]
    public void SecurityValidator_WithCombinedAttack_ShouldDetect()
    {
        // Arrange - SQL + XSS
        var combinedAttack = "SELECT * FROM Users WHERE username='<script>alert(1)</script>'";

        // Act
        var isSecure = SecurityValidator.IsSecure(combinedAttack);

        // Assert
        isSecure.Should().BeFalse("Combined SQL+XSS attack should be detected");
    }

    #endregion
}

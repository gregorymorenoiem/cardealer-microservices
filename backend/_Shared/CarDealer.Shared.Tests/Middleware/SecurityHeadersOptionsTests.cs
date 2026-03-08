using CarDealer.Shared.Middleware;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Middleware;

public class SecurityHeadersOptionsTests
{
    [Fact]
    public void SecurityHeadersOptions_DefaultValues_ShouldBeCorrect()
    {
        var options = new SecurityHeadersOptions();

        options.XFrameOptions.Should().Be("DENY");
        options.EnableHsts.Should().BeTrue();
        options.HstsMaxAgeSeconds.Should().Be(31536000); // 1 year
        options.ContentSecurityPolicy.Should().Be("default-src 'none'; frame-ancestors 'none'");
        options.ContentSecurityPolicyReportOnly.Should().BeNull();
        options.ReferrerPolicy.Should().Be("strict-origin-when-cross-origin");
        options.PermissionsPolicy.Should().Be("camera=(), microphone=(), geolocation=(), payment=()");
    }

    [Fact]
    public void SecurityHeadersOptions_CustomXFrameOptions_ShouldBeRetained()
    {
        var options = new SecurityHeadersOptions
        {
            XFrameOptions = "SAMEORIGIN"
        };

        options.XFrameOptions.Should().Be("SAMEORIGIN");
    }

    [Fact]
    public void SecurityHeadersOptions_DisableHsts_ShouldWork()
    {
        var options = new SecurityHeadersOptions
        {
            EnableHsts = false
        };

        options.EnableHsts.Should().BeFalse();
    }

    [Fact]
    public void SecurityHeadersOptions_CustomHstsMaxAge_ShouldBeRetained()
    {
        var options = new SecurityHeadersOptions
        {
            HstsMaxAgeSeconds = 86400 // 1 day
        };

        options.HstsMaxAgeSeconds.Should().Be(86400);
    }

    [Fact]
    public void SecurityHeadersOptions_CustomCSP_ShouldBeRetained()
    {
        var csp = "default-src 'self'; script-src 'self' cdn.example.com";
        var options = new SecurityHeadersOptions
        {
            ContentSecurityPolicy = csp
        };

        options.ContentSecurityPolicy.Should().Be(csp);
    }

    [Fact]
    public void SecurityHeadersOptions_CSPReportOnly_ShouldBeSettable()
    {
        var reportOnly = "default-src 'self'; report-uri /csp-report";
        var options = new SecurityHeadersOptions
        {
            ContentSecurityPolicyReportOnly = reportOnly
        };

        options.ContentSecurityPolicyReportOnly.Should().Be(reportOnly);
    }

    [Fact]
    public void SecurityHeadersOptions_CustomReferrerPolicy_ShouldBeRetained()
    {
        var options = new SecurityHeadersOptions
        {
            ReferrerPolicy = "no-referrer"
        };

        options.ReferrerPolicy.Should().Be("no-referrer");
    }

    [Fact]
    public void SecurityHeadersOptions_CustomPermissionsPolicy_ShouldBeRetained()
    {
        var options = new SecurityHeadersOptions
        {
            PermissionsPolicy = "camera=(self), microphone=()"
        };

        options.PermissionsPolicy.Should().Be("camera=(self), microphone=()");
    }

    [Fact]
    public void SecurityHeadersOptions_AllCustomValues_ShouldBeRetained()
    {
        var options = new SecurityHeadersOptions
        {
            XFrameOptions = "ALLOW-FROM https://example.com",
            EnableHsts = false,
            HstsMaxAgeSeconds = 0,
            ContentSecurityPolicy = "default-src *",
            ContentSecurityPolicyReportOnly = "report-uri /csp",
            ReferrerPolicy = "origin",
            PermissionsPolicy = "fullscreen=(self)"
        };

        options.XFrameOptions.Should().Be("ALLOW-FROM https://example.com");
        options.EnableHsts.Should().BeFalse();
        options.HstsMaxAgeSeconds.Should().Be(0);
        options.ContentSecurityPolicy.Should().Be("default-src *");
        options.ContentSecurityPolicyReportOnly.Should().Be("report-uri /csp");
        options.ReferrerPolicy.Should().Be("origin");
        options.PermissionsPolicy.Should().Be("fullscreen=(self)");
    }

    [Fact]
    public void SecurityHeadersOptions_DefaultHstsMaxAge_ShouldBeOneYear()
    {
        var options = new SecurityHeadersOptions();
        var oneYearInSeconds = 365 * 24 * 60 * 60;

        options.HstsMaxAgeSeconds.Should().Be(oneYearInSeconds);
    }
}

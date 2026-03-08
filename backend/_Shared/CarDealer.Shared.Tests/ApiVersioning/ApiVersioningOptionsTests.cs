using CarDealer.Shared.ApiVersioning.Attributes;
using Asp.Versioning;
using FluentAssertions;
using VersioningOptions = CarDealer.Shared.ApiVersioning.Configuration.ApiVersioningOptions;
using CarDealer.Shared.ApiVersioning.Configuration;

namespace CarDealer.Shared.Tests.ApiVersioning;

public class ApiVersioningOptionsTests
{
    // ── SectionName ──────────────────────────────────────────────────
    [Fact]
    public void SectionName_ShouldBe_ApiVersioning()
    {
        VersioningOptions.SectionName.Should().Be("ApiVersioning");
    }

    // ── Top-level defaults ───────────────────────────────────────────
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var opts = new VersioningOptions();

        opts.Enabled.Should().BeTrue();
        opts.DefaultVersion.Should().Be("1.0");
        opts.AssumeDefaultVersionWhenUnspecified.Should().BeTrue();
        opts.ReportApiVersions.Should().BeTrue();
        opts.VersionReader.Should().NotBeNull();
        opts.Swagger.Should().NotBeNull();
    }

    // ── VersionReaderOptions defaults ────────────────────────────────
    [Fact]
    public void VersionReaderOptions_DefaultValues_ShouldBeCorrect()
    {
        var reader = new VersionReaderOptions();

        reader.ReadFromQueryString.Should().BeTrue();
        reader.QueryStringParameter.Should().Be("api-version");
        reader.ReadFromHeader.Should().BeTrue();
        reader.HeaderName.Should().Be("X-Api-Version");
        reader.ReadFromUrl.Should().BeTrue();
        reader.ReadFromMediaType.Should().BeFalse();
        reader.MediaTypeParameter.Should().Be("v");
    }

    // ── SwaggerVersionOptions defaults ───────────────────────────────
    [Fact]
    public void SwaggerVersionOptions_DefaultValues_ShouldBeCorrect()
    {
        var swagger = new SwaggerVersionOptions();

        swagger.EnableMultipleDocuments.Should().BeTrue();
        swagger.Title.Should().Be("CarDealer API");
        swagger.Description.Should().Be("API de microservicios CarDealer");
        swagger.Contact.Should().BeNull();
        swagger.License.Should().BeNull();
    }

    // ── ContactInfo / LicenseInfo ────────────────────────────────────
    [Fact]
    public void ContactInfo_ShouldStoreValues()
    {
        var contact = new ContactInfo
        {
            Name = "Dev Team",
            Email = "dev@example.com",
            Url = "https://example.com"
        };

        contact.Name.Should().Be("Dev Team");
        contact.Email.Should().Be("dev@example.com");
        contact.Url.Should().Be("https://example.com");
    }

    [Fact]
    public void LicenseInfo_ShouldStoreValues()
    {
        var license = new LicenseInfo
        {
            Name = "MIT",
            Url = "https://opensource.org/licenses/MIT"
        };

        license.Name.Should().Be("MIT");
        license.Url.Should().Be("https://opensource.org/licenses/MIT");
    }
}

public class VersionAttributeTests
{
    // ── ApiV1Attribute ───────────────────────────────────────────────
    [Fact]
    public void ApiV1Attribute_ShouldRepresentVersion1()
    {
        var attr = new ApiV1Attribute();
        attr.Versions.Should().Contain(new ApiVersion(1, 0));
    }

    // ── ApiV2Attribute ───────────────────────────────────────────────
    [Fact]
    public void ApiV2Attribute_ShouldRepresentVersion2()
    {
        var attr = new ApiV2Attribute();
        attr.Versions.Should().Contain(new ApiVersion(2, 0));
    }

    // ── ApiV3Attribute ───────────────────────────────────────────────
    [Fact]
    public void ApiV3Attribute_ShouldRepresentVersion3()
    {
        var attr = new ApiV3Attribute();
        attr.Versions.Should().Contain(new ApiVersion(3, 0));
    }

    // ── ApiVersionDeprecatedAttribute ────────────────────────────────
    [Fact]
    public void ApiVersionDeprecatedAttribute_WithDate_ShouldStoreSunsetDate()
    {
        var attr = new ApiVersionDeprecatedAttribute(2026, 12, 31, "Use v2");

        attr.SunsetDate.Should().Be(new DateTime(2026, 12, 31));
        attr.Message.Should().Be("Use v2");
    }

    [Fact]
    public void ApiVersionDeprecatedAttribute_WithoutDate_ShouldHaveNullSunsetDate()
    {
        var attr = new ApiVersionDeprecatedAttribute("Deprecated");

        attr.SunsetDate.Should().BeNull();
        attr.Message.Should().Be("Deprecated");
    }

    [Fact]
    public void ApiVersionDeprecatedAttribute_RecommendedVersion_ShouldBeSettable()
    {
        var attr = new ApiVersionDeprecatedAttribute
        {
            RecommendedVersion = "3.0"
        };

        attr.RecommendedVersion.Should().Be("3.0");
    }
}

using CarDealer.Contracts.Enums;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Contracts;

public class ServiceNamesTests
{
    [Fact]
    public void ServiceNames_ShouldHave27Values()
    {
        var values = Enum.GetValues<ServiceNames>();
        values.Should().HaveCount(27);
    }

    [Fact]
    public void ServiceNames_ShouldHaveNoDuplicateValues()
    {
        var values = Enum.GetValues<ServiceNames>();
        var intValues = values.Select(v => (int)v).ToList();
        intValues.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(ServiceNames.Gateway)]
    [InlineData(ServiceNames.AuthService)]
    [InlineData(ServiceNames.VehiclesSaleService)]
    [InlineData(ServiceNames.MediaService)]
    [InlineData(ServiceNames.NotificationService)]
    [InlineData(ServiceNames.ErrorService)]
    [InlineData(ServiceNames.AuditService)]
    [InlineData(ServiceNames.AdminService)]
    [InlineData(ServiceNames.ContactService)]
    [InlineData(ServiceNames.AIProcessingService)]
    [InlineData(ServiceNames.BillingService)]
    [InlineData(ServiceNames.ChatbotService)]
    [InlineData(ServiceNames.ComparisonService)]
    [InlineData(ServiceNames.CRMService)]
    [InlineData(ServiceNames.DealerAnalyticsService)]
    [InlineData(ServiceNames.KYCService)]
    [InlineData(ServiceNames.RecoAgent)]
    [InlineData(ServiceNames.RecommendationService)]
    [InlineData(ServiceNames.ReportsService)]
    [InlineData(ServiceNames.ReviewService)]
    [InlineData(ServiceNames.RoleService)]
    [InlineData(ServiceNames.SearchAgent)]
    [InlineData(ServiceNames.SupportAgent)]
    [InlineData(ServiceNames.UserService)]
    [InlineData(ServiceNames.VehicleIntelligenceService)]
    public void ServiceNames_ShouldContainExpectedValue(ServiceNames serviceName)
    {
        Enum.IsDefined(serviceName).Should().BeTrue();
    }
}

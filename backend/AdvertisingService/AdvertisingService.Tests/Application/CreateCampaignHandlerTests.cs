using AdvertisingService.Application.Clients;
using AdvertisingService.Application.Features.Campaigns.Commands.CreateCampaign;
using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;
using AdvertisingService.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdvertisingService.Tests.Application;

public class CreateCampaignHandlerTests
{
    private readonly Mock<IAdCampaignRepository> _campaignRepo;
    private readonly Mock<VehicleServiceClient> _vehicleClient;
    private readonly Mock<AuditServiceClient> _auditClient;
    private readonly Mock<ILogger<CreateCampaignCommandHandler>> _logger;

    public CreateCampaignHandlerTests()
    {
        _campaignRepo = new Mock<IAdCampaignRepository>();
        _vehicleClient = new Mock<VehicleServiceClient>(
            MockBehavior.Loose, new HttpClient(), new Mock<ILogger<VehicleServiceClient>>().Object);
        _auditClient = new Mock<AuditServiceClient>(
            MockBehavior.Loose, new HttpClient(), new Mock<ILogger<AuditServiceClient>>().Object);
        _logger = new Mock<ILogger<CreateCampaignCommandHandler>>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCampaign()
    {
        var command = new CreateCampaignCommand(
            OwnerId: Guid.NewGuid(),
            OwnerType: "Individual",
            VehicleId: Guid.NewGuid(),
            PlacementType: AdPlacementType.FeaturedSpot,
            PricingModel: CampaignPricingModel.PerView,
            TotalBudget: 500m,
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(31));

        _campaignRepo.Setup(r => r.AddAsync(It.IsAny<AdCampaign>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateCampaignCommandHandler(
            _campaignRepo.Object, _vehicleClient.Object,
            _auditClient.Object, _logger.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.OwnerId.Should().Be(command.OwnerId);
        result.PlacementType.Should().Be(AdPlacementType.FeaturedSpot);
        result.Status.Should().Be(CampaignStatus.PendingPayment);
        result.TotalBudget.Should().Be(500m);

        _campaignRepo.Verify(r => r.AddAsync(It.IsAny<AdCampaign>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCompleteWithoutError_AndPersistCampaign()
    {
        // AuditServiceClient.LogActionAsync is not virtual, so Moq cannot intercept it.
        // Instead we verify the handler completes successfully and persists the campaign.
        var command = new CreateCampaignCommand(
            OwnerId: Guid.NewGuid(),
            OwnerType: "Dealer",
            VehicleId: Guid.NewGuid(),
            PlacementType: AdPlacementType.PremiumSpot,
            PricingModel: CampaignPricingModel.FixedDaily,
            TotalBudget: 1000m,
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(30));

        AdCampaign? capturedCampaign = null;
        _campaignRepo.Setup(r => r.AddAsync(It.IsAny<AdCampaign>(), It.IsAny<CancellationToken>()))
            .Callback<AdCampaign, CancellationToken>((c, _) => capturedCampaign = c)
            .Returns(Task.CompletedTask);

        var handler = new CreateCampaignCommandHandler(
            _campaignRepo.Object, _vehicleClient.Object,
            _auditClient.Object, _logger.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.OwnerType.Should().Be("Dealer");
        result.PlacementType.Should().Be(AdPlacementType.PremiumSpot);
        capturedCampaign.Should().NotBeNull();
        capturedCampaign!.TotalBudget.Should().Be(1000m);
    }
}

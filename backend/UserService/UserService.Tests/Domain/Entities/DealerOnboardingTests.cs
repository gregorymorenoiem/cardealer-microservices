using FluentAssertions;
using UserService.Domain.Entities;
using Xunit;

namespace UserService.Tests.Domain.Entities;

public class DealerOnboardingTests
{
    [Fact]
    public void DealerOnboarding_ShouldInitializeWithPendingStatus()
    {
        // Arrange & Act
        var onboarding = new DealerOnboarding();

        // Assert
        onboarding.Status.Should().Be(DealerOnboardingStatus.Pending);
        onboarding.Type.Should().Be(DealerOnboardingType.Independent);
        onboarding.RequestedPlan.Should().Be(DealerOnboardingPlan.Starter);
    }

    [Fact]
    public void DealerOnboarding_IsCompleted_ShouldReturnTrueOnlyWhenActive()
    {
        // Arrange
        var onboarding = new DealerOnboarding();

        // Act & Assert
        onboarding.Status = DealerOnboardingStatus.Pending;
        onboarding.IsCompleted.Should().BeFalse();

        onboarding.Status = DealerOnboardingStatus.Active;
        onboarding.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void DealerOnboarding_IsRejected_ShouldReturnTrueOnlyWhenRejected()
    {
        // Arrange
        var onboarding = new DealerOnboarding();

        // Act & Assert
        onboarding.Status = DealerOnboardingStatus.Approved;
        onboarding.IsRejected.Should().BeFalse();

        onboarding.Status = DealerOnboardingStatus.Rejected;
        onboarding.IsRejected.Should().BeTrue();
    }

    [Fact]
    public void DealerOnboarding_CanUploadDocuments_ShouldReturnTrueOnlyWhenEmailVerified()
    {
        // Arrange
        var onboarding = new DealerOnboarding();

        // Act & Assert
        onboarding.Status = DealerOnboardingStatus.Pending;
        onboarding.CanUploadDocuments.Should().BeFalse();

        onboarding.Status = DealerOnboardingStatus.EmailVerified;
        onboarding.CanUploadDocuments.Should().BeTrue();

        onboarding.Status = DealerOnboardingStatus.DocumentsSubmitted;
        onboarding.CanUploadDocuments.Should().BeFalse();
    }

    [Fact]
    public void DealerOnboarding_IsPendingReview_ShouldReturnTrueForDocumentsSubmittedOrUnderReview()
    {
        // Arrange
        var onboarding = new DealerOnboarding();

        // Act & Assert
        onboarding.Status = DealerOnboardingStatus.EmailVerified;
        onboarding.IsPendingReview.Should().BeFalse();

        onboarding.Status = DealerOnboardingStatus.DocumentsSubmitted;
        onboarding.IsPendingReview.Should().BeTrue();

        onboarding.Status = DealerOnboardingStatus.UnderReview;
        onboarding.IsPendingReview.Should().BeTrue();

        onboarding.Status = DealerOnboardingStatus.Approved;
        onboarding.IsPendingReview.Should().BeFalse();
    }

    [Fact]
    public void DealerOnboarding_CanSetupPayment_ShouldReturnTrueOnlyWhenApproved()
    {
        // Arrange
        var onboarding = new DealerOnboarding();

        // Act & Assert
        onboarding.Status = DealerOnboardingStatus.UnderReview;
        onboarding.CanSetupPayment.Should().BeFalse();

        onboarding.Status = DealerOnboardingStatus.Approved;
        onboarding.CanSetupPayment.Should().BeTrue();

        onboarding.Status = DealerOnboardingStatus.PaymentSetup;
        onboarding.CanSetupPayment.Should().BeFalse();
    }

    [Fact]
    public void DealerOnboarding_HasAllRequiredDocuments_ShouldCheckAllMandatoryDocs()
    {
        // Arrange
        var onboarding = new DealerOnboarding();

        // Act & Assert - Sin documentos
        onboarding.HasAllRequiredDocuments.Should().BeFalse();

        // Agregar documentos parcialmente
        onboarding.RncDocumentId = Guid.NewGuid();
        onboarding.HasAllRequiredDocuments.Should().BeFalse();

        onboarding.BusinessLicenseDocumentId = Guid.NewGuid();
        onboarding.HasAllRequiredDocuments.Should().BeFalse();

        onboarding.LegalRepCedulaDocumentId = Guid.NewGuid();
        onboarding.HasAllRequiredDocuments.Should().BeFalse();

        // Completar documentos obligatorios
        onboarding.AddressProofDocumentId = Guid.NewGuid();
        onboarding.HasAllRequiredDocuments.Should().BeTrue();
    }

    [Theory]
    [InlineData("2026-01-01", true)]   // Antes del deadline
    [InlineData("2026-01-31", true)]   // En el deadline
    [InlineData("2026-02-01", false)]  // Después del deadline
    [InlineData("2025-12-15", true)]   // Mucho antes
    [InlineData("2026-03-15", false)]  // Mucho después
    public void CalculateEarlyBirdEligibility_ShouldReturnCorrectly(string dateStr, bool expected)
    {
        // Arrange
        var date = DateTime.Parse(dateStr);

        // Act
        var result = DealerOnboarding.CalculateEarlyBirdEligibility(date);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void DealerOnboarding_AdvanceToNextStatus_ShouldProgressCorrectly()
    {
        // Arrange
        var onboarding = new DealerOnboarding();
        onboarding.Status.Should().Be(DealerOnboardingStatus.Pending);

        // Act & Assert - Avanzar por todos los estados
        onboarding.AdvanceToNextStatus();
        onboarding.Status.Should().Be(DealerOnboardingStatus.EmailVerified);

        onboarding.AdvanceToNextStatus();
        onboarding.Status.Should().Be(DealerOnboardingStatus.DocumentsSubmitted);

        onboarding.AdvanceToNextStatus();
        onboarding.Status.Should().Be(DealerOnboardingStatus.UnderReview);

        onboarding.AdvanceToNextStatus();
        onboarding.Status.Should().Be(DealerOnboardingStatus.Approved);

        onboarding.AdvanceToNextStatus();
        onboarding.Status.Should().Be(DealerOnboardingStatus.PaymentSetup);

        onboarding.AdvanceToNextStatus();
        onboarding.Status.Should().Be(DealerOnboardingStatus.Active);

        // No debería avanzar más allá de Active
        onboarding.AdvanceToNextStatus();
        onboarding.Status.Should().Be(DealerOnboardingStatus.Active);
    }

    [Fact]
    public void DealerOnboarding_AdvanceToNextStatus_ShouldUpdateTimestamp()
    {
        // Arrange
        var onboarding = new DealerOnboarding();
        onboarding.UpdatedAt.Should().BeNull();

        // Act
        onboarding.AdvanceToNextStatus();

        // Assert
        onboarding.UpdatedAt.Should().NotBeNull();
        onboarding.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}

public class DealerOnboardingStatusEnumTests
{
    [Fact]
    public void DealerOnboardingStatus_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)DealerOnboardingStatus.Pending).Should().Be(0);
        ((int)DealerOnboardingStatus.EmailVerified).Should().Be(1);
        ((int)DealerOnboardingStatus.DocumentsSubmitted).Should().Be(2);
        ((int)DealerOnboardingStatus.UnderReview).Should().Be(3);
        ((int)DealerOnboardingStatus.Approved).Should().Be(4);
        ((int)DealerOnboardingStatus.PaymentSetup).Should().Be(5);
        ((int)DealerOnboardingStatus.Active).Should().Be(6);
        ((int)DealerOnboardingStatus.Rejected).Should().Be(7);
        ((int)DealerOnboardingStatus.Suspended).Should().Be(8);
    }

    [Fact]
    public void DealerOnboardingStatus_ShouldHave9Values()
    {
        // Assert
        Enum.GetValues<DealerOnboardingStatus>().Should().HaveCount(9);
    }
}

public class DealerOnboardingPlanEnumTests
{
    [Fact]
    public void DealerOnboardingPlan_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)DealerOnboardingPlan.None).Should().Be(0);
        ((int)DealerOnboardingPlan.Starter).Should().Be(1);
        ((int)DealerOnboardingPlan.Pro).Should().Be(2);
        ((int)DealerOnboardingPlan.Enterprise).Should().Be(3);
    }
}

public class DealerOnboardingTypeEnumTests
{
    [Fact]
    public void DealerOnboardingType_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)DealerOnboardingType.Independent).Should().Be(0);
        ((int)DealerOnboardingType.Chain).Should().Be(1);
        ((int)DealerOnboardingType.MultipleStore).Should().Be(2);
        ((int)DealerOnboardingType.Franchise).Should().Be(3);
    }
}

public class DealerOnboardingEventsTests
{
    [Fact]
    public void DealerRegisteredEvent_ShouldCreateCorrectly()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var businessName = "Test Dealer";
        var email = "test@dealer.com";
        var rnc = "123456789";
        var plan = "Pro";

        // Act
        var @event = UserService.Domain.Events.DealerRegisteredEvent.Create(
            dealerId, userId, businessName, email, rnc, plan, true);

        // Assert
        @event.EventType.Should().Be("dealer.registered");
        @event.DealerId.Should().Be(dealerId);
        @event.UserId.Should().Be(userId);
        @event.BusinessName.Should().Be(businessName);
        @event.Email.Should().Be(email);
        @event.RNC.Should().Be(rnc);
        @event.RequestedPlan.Should().Be(plan);
        @event.IsEarlyBirdEligible.Should().BeTrue();
        @event.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public void DealerApprovedEvent_ShouldCreateCorrectly()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var approvedAt = DateTime.UtcNow;

        // Act
        var @event = UserService.Domain.Events.DealerApprovedEvent.Create(
            dealerId, "Test Dealer", "test@dealer.com", adminId, approvedAt, "Enterprise");

        // Assert
        @event.EventType.Should().Be("dealer.approved");
        @event.DealerId.Should().Be(dealerId);
        @event.ApprovedBy.Should().Be(adminId);
        @event.ApprovedAt.Should().Be(approvedAt);
        @event.RequestedPlan.Should().Be("Enterprise");
    }

    [Fact]
    public void DealerActivatedEvent_ShouldCreateCorrectly()
    {
        // Arrange
        var onboardingId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var activatedAt = DateTime.UtcNow;

        // Act
        var @event = UserService.Domain.Events.DealerActivatedEvent.Create(
            onboardingId, dealerId, subscriptionId, "Test Dealer", "test@dealer.com", "Pro", true, activatedAt);

        // Assert
        @event.EventType.Should().Be("dealer.activated");
        @event.OnboardingId.Should().Be(onboardingId);
        @event.DealerId.Should().Be(dealerId);
        @event.SubscriptionId.Should().Be(subscriptionId);
        @event.Plan.Should().Be("Pro");
        @event.IsEarlyBird.Should().BeTrue();
        @event.ActivatedAt.Should().Be(activatedAt);
    }
}

/**
 * PrivacyControllerTests - Tests para el controlador de privacidad ARCO
 * 
 * Tests para verificar el correcto funcionamiento de los endpoints de privacidad
 * según la Ley 172-13 de Protección de Datos Personales de RD.
 * 
 * Total Tests: 15
 * 
 * @version 2.0.0
 * @since Enero 26, 2026
 */

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.Api.Controllers;
using UserService.Application.DTOs.Privacy;
using UserService.Application.UseCases.Privacy.GetUserDataSummary;
using UserService.Application.UseCases.Privacy.GetUserFullData;
using UserService.Application.UseCases.Privacy.GetCommunicationPreferences;
using UserService.Application.UseCases.Privacy.UpdateCommunicationPreferences;
using UserService.Application.UseCases.Privacy.RequestDataExport;
using UserService.Application.UseCases.Privacy.GetExportStatus;
using UserService.Application.UseCases.Privacy.RequestAccountDeletion;
using UserService.Application.UseCases.Privacy.ConfirmAccountDeletion;
using UserService.Application.UseCases.Privacy.CancelAccountDeletion;
using UserService.Application.UseCases.Privacy.GetAccountDeletionStatus;
using UserService.Application.UseCases.Privacy.GetPrivacyRequestHistory;
using UserService.Domain.Entities.Privacy;
using Xunit;

namespace UserService.Tests.Controllers;

public class PrivacyControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly PrivacyController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public PrivacyControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new PrivacyController(_mediatorMock.Object);
        
        // Setup mock HTTP context with user claims
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
                new Claim("sub", _testUserId.ToString())
            }, "TestAuth")
        );
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        _controller.ControllerContext = new ControllerContext 
        { 
            HttpContext = httpContext 
        };
    }

    #region ARCO-ACCESS Tests (3 tests)

    [Fact]
    public async Task GetMyDataSummary_ShouldReturnUserDataSummary()
    {
        // Arrange
        var expectedSummary = new UserDataSummaryDto(
            Profile: new ProfileSummaryDto(
                FullName: "Test User",
                Email: "test@example.com",
                Phone: "+18091234567",
                City: "Santo Domingo",
                Province: "DN",
                AccountType: "Individual",
                MemberSince: DateTime.UtcNow.AddYears(-1),
                EmailVerified: true
            ),
            Activity: new ActivitySummaryDto(
                TotalSearches: 10,
                TotalVehicleViews: 25,
                TotalFavorites: 5,
                TotalAlerts: 2,
                TotalMessages: 10,
                LastActivity: DateTime.UtcNow
            ),
            Transactions: new TransactionsSummaryDto(
                TotalPayments: 2,
                TotalSpent: 1500,
                ActiveSubscription: null,
                TotalInvoices: 2
            ),
            Privacy: new PrivacySettingsSummaryDto(
                MarketingOptIn: true,
                AnalyticsOptIn: true,
                ThirdPartyOptIn: false,
                LastUpdated: DateTime.UtcNow
            ),
            GeneratedAt: DateTime.UtcNow
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserDataSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _controller.GetMyDataSummary();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var summary = okResult.Value.Should().BeOfType<UserDataSummaryDto>().Subject;
        summary.Profile.Email.Should().Be("test@example.com");
        summary.Activity.TotalVehicleViews.Should().Be(25);
    }

    [Fact]
    public async Task GetMyFullData_ShouldReturnCompleteUserData()
    {
        // Arrange
        var expectedData = new UserFullDataDto(
            ExportDate: DateTime.UtcNow,
            ExportVersion: "1.0.0",
            Profile: new UserProfileExportDto(
                Id: Guid.NewGuid(),
                Email: "test@example.com",
                FirstName: "Test",
                LastName: "User",
                PhoneNumber: "+18091234567",
                ProfilePicture: null,
                City: "Santo Domingo",
                Province: "DN",
                BusinessName: null,
                BusinessPhone: null,
                BusinessAddress: null,
                RNC: null,
                AccountType: "Individual",
                CreatedAt: DateTime.UtcNow.AddYears(-1),
                LastLoginAt: DateTime.UtcNow,
                EmailVerified: true
            ),
            Activity: new UserActivityExportDto(
                SearchHistory: new List<SearchHistoryExportDto>(),
                VehicleViews: new List<VehicleViewExportDto>(),
                Favorites: new List<FavoriteExportDto>(),
                Alerts: new List<AlertExportDto>(),
                Sessions: new List<SessionExportDto>()
            ),
            Transactions: new UserTransactionsExportDto(
                Payments: new List<PaymentExportDto>(),
                Invoices: new List<InvoiceExportDto>(),
                Subscriptions: new List<SubscriptionExportDto>()
            ),
            Messages: new UserMessagesExportDto(
                TotalConversations: 0,
                TotalMessages: 0,
                Conversations: new List<ConversationExportDto>()
            ),
            Settings: new UserSettingsExportDto(
                CommunicationPreferences: new CommunicationSettingsExportDto(
                    EmailActivityNotifications: true,
                    EmailListingUpdates: true,
                    EmailNewsletter: false,
                    EmailPromotions: false,
                    SmsAlerts: true,
                    SmsPromotions: false,
                    PushNewMessages: true,
                    PushPriceChanges: true,
                    PushRecommendations: false
                ),
                PrivacyPreferences: new PrivacySettingsExportDto(
                    AllowProfiling: false,
                    AllowThirdPartySharing: false,
                    AllowAnalytics: true,
                    AllowRetargeting: false
                )
            ),
            DataSharing: new DataSharingInfoDto(
                Description: "No data shared",
                ThirdParties: new List<ThirdPartyShareDto>()
            )
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserFullDataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.GetMyFullData();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<UserFullDataDto>().Subject;
        data.Profile.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void GetARCORightsInfo_ShouldReturnPublicInfo()
    {
        // Act
        var result = _controller.GetARCORightsInfo();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    #endregion

    #region ARCO-OPPOSITION Tests (3 tests)

    [Fact]
    public async Task GetCommunicationPreferences_ShouldReturnPreferences()
    {
        // Arrange
        var expectedPrefs = new CommunicationPreferencesDto(
            Email: new EmailPreferencesDto(
                ActivityNotifications: true,
                ListingUpdates: true,
                Newsletter: false,
                Promotions: false,
                PriceAlerts: true
            ),
            Sms: new SmsPreferencesDto(
                VerificationCodes: true,
                PriceAlerts: false,
                Promotions: false
            ),
            Push: new PushPreferencesDto(
                NewMessages: true,
                PriceChanges: true,
                Recommendations: false
            ),
            Privacy: new PrivacyPreferencesDto(
                AllowProfiling: false,
                AllowThirdPartySharing: false,
                AllowAnalytics: true,
                AllowRetargeting: false
            ),
            LastUpdated: DateTime.UtcNow
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCommunicationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPrefs);

        // Act
        var result = await _controller.GetCommunicationPreferences();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var prefs = okResult.Value.Should().BeOfType<CommunicationPreferencesDto>().Subject;
        prefs.Email.ActivityNotifications.Should().BeTrue();
        prefs.Privacy.AllowProfiling.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCommunicationPreferences_ShouldUpdateAndReturnPreferences()
    {
        // Arrange
        var updateDto = new UpdateCommunicationPreferencesDto(
            EmailNewsletter: false,
            AllowProfiling: false
        );

        var expectedPrefs = new CommunicationPreferencesDto(
            Email: new EmailPreferencesDto(
                ActivityNotifications: true,
                ListingUpdates: true,
                Newsletter: false,
                Promotions: false,
                PriceAlerts: true
            ),
            Sms: new SmsPreferencesDto(
                VerificationCodes: true,
                PriceAlerts: true,
                Promotions: false
            ),
            Push: new PushPreferencesDto(
                NewMessages: true,
                PriceChanges: true,
                Recommendations: false
            ),
            Privacy: new PrivacyPreferencesDto(
                AllowProfiling: false,
                AllowThirdPartySharing: false,
                AllowAnalytics: true,
                AllowRetargeting: false
            ),
            LastUpdated: DateTime.UtcNow
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateCommunicationPreferencesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPrefs);

        // Act
        var result = await _controller.UpdateCommunicationPreferences(updateDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var prefs = okResult.Value.Should().BeOfType<CommunicationPreferencesDto>().Subject;
        prefs.Email.Newsletter.Should().BeFalse();
        prefs.Privacy.AllowProfiling.Should().BeFalse();
    }

    [Fact]
    public async Task UnsubscribeFromAllMarketing_ShouldReturnSuccess()
    {
        // Arrange
        var expectedPrefs = new CommunicationPreferencesDto(
            Email: new EmailPreferencesDto(
                ActivityNotifications: true,
                ListingUpdates: true,
                Newsletter: false,
                Promotions: false,
                PriceAlerts: true
            ),
            Sms: new SmsPreferencesDto(
                VerificationCodes: true,
                PriceAlerts: false,
                Promotions: false
            ),
            Push: new PushPreferencesDto(
                NewMessages: true,
                PriceChanges: true,
                Recommendations: false
            ),
            Privacy: new PrivacyPreferencesDto(
                AllowProfiling: false,
                AllowThirdPartySharing: false,
                AllowAnalytics: false,
                AllowRetargeting: false
            ),
            LastUpdated: DateTime.UtcNow
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateCommunicationPreferencesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPrefs);

        // Act
        var result = await _controller.UnsubscribeFromAllMarketing();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region ARCO-PORTABILITY Tests (3 tests)

    [Fact]
    public async Task RequestDataExport_ShouldReturnExportResponse()
    {
        // Arrange
        var request = new RequestDataExportDto(
            Format: ExportFormat.Json,
            IncludeProfile: true,
            IncludeActivity: true,
            IncludeTransactions: true,
            IncludeMessages: false
        );

        var expectedResponse = new DataExportRequestResponseDto(
            RequestId: Guid.NewGuid(),
            Status: "Processing",
            Message: "Tu solicitud de exportación está siendo procesada",
            EstimatedCompletionTime: DateTime.UtcNow.AddMinutes(15)
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RequestDataExportCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.RequestDataExport(request);

        // Assert
        var acceptedResult = result.Result.Should().BeOfType<AcceptedResult>().Subject;
        var response = acceptedResult.Value.Should().BeOfType<DataExportRequestResponseDto>().Subject;
        response.Status.Should().Be("Processing");
    }

    [Fact]
    public async Task GetExportStatus_ShouldReturnCurrentStatus()
    {
        // Arrange
        var expectedStatus = new DataExportStatusDto(
            RequestId: Guid.NewGuid(),
            Status: "Completed",
            RequestedAt: DateTime.UtcNow.AddMinutes(-10),
            ReadyAt: DateTime.UtcNow,
            ExpiresAt: DateTime.UtcNow.AddDays(7),
            DownloadToken: "abc123",
            FileSize: "15KB",
            Format: "JSON"
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetExportStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.GetExportStatus();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var status = okResult.Value.Should().BeOfType<DataExportStatusDto>().Subject;
        status.Status.Should().Be("Completed");
        status.DownloadToken.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DownloadExport_WithValidToken_ShouldReturnFile()
    {
        // Arrange
        var token = "valid-export-token";

        // Act
        var result = await _controller.DownloadExport(token);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("application/json");
        fileResult.FileDownloadName.Should().Contain("mis-datos-okla");
    }

    #endregion

    #region ARCO-CANCELLATION Tests (4 tests)

    [Fact]
    public async Task RequestAccountDeletion_ShouldReturnDeletionResponse()
    {
        // Arrange
        var request = new RequestAccountDeletionDto(
            Reason: DeletionReason.NoLongerNeeded,
            OtherReason: null,
            Feedback: "Just trying something else"
        );

        var expectedResponse = new AccountDeletionResponseDto(
            RequestId: Guid.NewGuid(),
            Status: "PendingConfirmation",
            Message: "Solicitud recibida. Revisa tu email para confirmar.",
            GracePeriodEndsAt: DateTime.UtcNow.AddDays(30),
            ConfirmationEmailSentTo: "test@example.com"
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RequestAccountDeletionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.RequestAccountDeletion(request);

        // Assert
        var acceptedResult = result.Result.Should().BeOfType<AcceptedResult>().Subject;
        var response = acceptedResult.Value.Should().BeOfType<AccountDeletionResponseDto>().Subject;
        response.Status.Should().Be("PendingConfirmation");
    }

    [Fact]
    public async Task ConfirmAccountDeletion_ShouldReturnUpdatedStatus()
    {
        // Arrange
        var confirmation = new ConfirmAccountDeletionDto(
            ConfirmationCode: "ABC123",
            Password: "SecurePassword123!"
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmAccountDeletionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ConfirmAccountDeletion(confirmation);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CancelAccountDeletion_ShouldReturnSuccess()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelAccountDeletionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CancelAccountDeletion();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAccountDeletionStatus_ShouldReturnStatus()
    {
        // Arrange
        var expectedStatus = new AccountDeletionStatusDto(
            RequestId: Guid.NewGuid(),
            Status: "Confirmed",
            RequestedAt: DateTime.UtcNow.AddDays(-5),
            GracePeriodEndsAt: DateTime.UtcNow.AddDays(25),
            CanCancel: true,
            DaysRemaining: 25,
            Reason: "NoLongerNeeded"
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAccountDeletionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.GetAccountDeletionStatus();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var status = okResult.Value.Should().BeOfType<AccountDeletionStatusDto>().Subject;
        status.CanCancel.Should().BeTrue();
        status.DaysRemaining.Should().Be(25);
    }

    #endregion

    #region Request History Tests (2 tests)

    [Fact]
    public async Task GetPrivacyRequestHistory_ShouldReturnRequestList()
    {
        // Arrange
        var expectedHistory = new PrivacyRequestsListDto(
            Requests: new List<PrivacyRequestHistoryDto>
            {
                new(
                    Id: Guid.NewGuid(),
                    Type: "Export",
                    Status: "Completed",
                    Description: "Data export requested",
                    CreatedAt: DateTime.UtcNow.AddDays(-10),
                    CompletedAt: DateTime.UtcNow.AddDays(-10)
                ),
                new(
                    Id: Guid.NewGuid(),
                    Type: "Access",
                    Status: "Completed",
                    Description: "Data access requested",
                    CreatedAt: DateTime.UtcNow.AddDays(-5),
                    CompletedAt: DateTime.UtcNow.AddDays(-5)
                )
            },
            TotalCount: 2,
            Page: 1,
            PageSize: 10
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPrivacyRequestHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHistory);

        // Act
        var result = await _controller.GetPrivacyRequestHistory();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var history = okResult.Value.Should().BeOfType<PrivacyRequestsListDto>().Subject;
        history.TotalCount.Should().Be(2);
        history.Requests.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPrivacyRequestHistory_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        var expectedHistory = new PrivacyRequestsListDto(
            Requests: new List<PrivacyRequestHistoryDto>
            {
                new(
                    Id: Guid.NewGuid(),
                    Type: "Export",
                    Status: "Completed",
                    Description: "Data export",
                    CreatedAt: DateTime.UtcNow.AddDays(-1),
                    CompletedAt: DateTime.UtcNow.AddDays(-1)
                )
            },
            TotalCount: 15,
            Page: 2,
            PageSize: 5
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPrivacyRequestHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHistory);

        // Act
        var result = await _controller.GetPrivacyRequestHistory(page: 2, pageSize: 5);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var history = okResult.Value.Should().BeOfType<PrivacyRequestsListDto>().Subject;
        history.Page.Should().Be(2);
        history.PageSize.Should().Be(5);
        history.TotalCount.Should().Be(15);
    }

    #endregion
}

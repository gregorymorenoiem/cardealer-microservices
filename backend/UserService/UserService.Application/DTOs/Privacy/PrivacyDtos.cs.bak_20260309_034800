using System;
using System.Collections.Generic;
using UserService.Domain.Entities.Privacy;

namespace UserService.Application.DTOs.Privacy;

#region Data Summary DTOs

/// <summary>
/// Resumen de datos del usuario (vista rápida)
/// </summary>
public record UserDataSummaryDto(
    ProfileSummaryDto Profile,
    ActivitySummaryDto Activity,
    TransactionsSummaryDto Transactions,
    PrivacySettingsSummaryDto Privacy,
    DateTime GeneratedAt
);

public record ProfileSummaryDto(
    string FullName,
    string Email,
    string? Phone,
    string? City,
    string? Province,
    string AccountType,
    DateTime MemberSince,
    bool EmailVerified
);

public record ActivitySummaryDto(
    int TotalSearches,
    int TotalVehicleViews,
    int TotalFavorites,
    int TotalAlerts,
    int TotalMessages,
    DateTime? LastActivity
);

public record TransactionsSummaryDto(
    int TotalPayments,
    decimal TotalSpent,
    string? ActiveSubscription,
    int TotalInvoices
);

public record PrivacySettingsSummaryDto(
    bool MarketingOptIn,
    bool AnalyticsOptIn,
    bool ThirdPartyOptIn,
    DateTime LastUpdated
);

#endregion

#region Data Export DTOs

/// <summary>
/// Solicitud de exportación de datos
/// </summary>
public record RequestDataExportDto(
    ExportFormat Format,
    bool IncludeProfile = true,
    bool IncludeActivity = true,
    bool IncludeMessages = true,
    bool IncludeFavorites = true,
    bool IncludeTransactions = true
);

/// <summary>
/// Respuesta a solicitud de exportación
/// </summary>
public record DataExportRequestResponseDto(
    Guid RequestId,
    string Status,
    string Message,
    DateTime? EstimatedCompletionTime
);

/// <summary>
/// Estado de exportación
/// </summary>
public record DataExportStatusDto(
    Guid RequestId,
    string Status,
    DateTime RequestedAt,
    DateTime? ReadyAt,
    DateTime? ExpiresAt,
    string? DownloadToken,
    string? FileSize,
    string Format
);

#endregion

#region Account Deletion DTOs

/// <summary>
/// Razones para eliminar cuenta
/// </summary>
public enum DeletionReason
{
    PrivacyConcerns,
    NoLongerNeeded,
    FoundAlternative,
    BadExperience,
    TooManyEmails,
    Other
}

/// <summary>
/// Solicitud de eliminación de cuenta
/// </summary>
public record RequestAccountDeletionDto(
    DeletionReason Reason,
    string? OtherReason = null,
    string? Feedback = null
);

/// <summary>
/// Confirmación de eliminación
/// </summary>
public record ConfirmAccountDeletionDto(
    string ConfirmationCode,
    string Password
);

/// <summary>
/// Estado de solicitud de eliminación
/// </summary>
public record AccountDeletionStatusDto(
    Guid RequestId,
    string Status,
    DateTime RequestedAt,
    DateTime GracePeriodEndsAt,
    bool CanCancel,
    int DaysRemaining,
    string? Reason
);

/// <summary>
/// Respuesta a solicitud de eliminación
/// </summary>
public record AccountDeletionResponseDto(
    Guid RequestId,
    string Status,
    string Message,
    DateTime GracePeriodEndsAt,
    string ConfirmationEmailSentTo
);

#endregion

#region Communication Preferences DTOs

/// <summary>
/// Preferencias de comunicación completas
/// </summary>
public record CommunicationPreferencesDto(
    EmailPreferencesDto Email,
    SmsPreferencesDto Sms,
    PushPreferencesDto Push,
    PrivacyPreferencesDto Privacy,
    DateTime LastUpdated
);

public record EmailPreferencesDto(
    bool ActivityNotifications,
    bool ListingUpdates,
    bool Newsletter,
    bool Promotions,
    bool PriceAlerts
);

public record SmsPreferencesDto(
    bool VerificationCodes, // Always true (read-only)
    bool PriceAlerts,
    bool Promotions
);

public record PushPreferencesDto(
    bool NewMessages,
    bool PriceChanges,
    bool Recommendations
);

public record PrivacyPreferencesDto(
    bool AllowProfiling,
    bool AllowThirdPartySharing,
    bool AllowAnalytics,
    bool AllowRetargeting
);

/// <summary>
/// Actualizar preferencias de comunicación
/// </summary>
public record UpdateCommunicationPreferencesDto(
    // Email
    bool? EmailActivityNotifications = null,
    bool? EmailListingUpdates = null,
    bool? EmailNewsletter = null,
    bool? EmailPromotions = null,
    bool? EmailPriceAlerts = null,
    // SMS
    bool? SmsPriceAlerts = null,
    bool? SmsPromotions = null,
    // Push
    bool? PushNewMessages = null,
    bool? PushPriceChanges = null,
    bool? PushRecommendations = null,
    // Privacy
    bool? AllowProfiling = null,
    bool? AllowThirdPartySharing = null,
    bool? AllowAnalytics = null,
    bool? AllowRetargeting = null
);

#endregion

#region Privacy Request History DTOs

/// <summary>
/// Historial de solicitudes ARCO
/// </summary>
public record PrivacyRequestHistoryDto(
    Guid Id,
    string Type,
    string Status,
    string Description,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

/// <summary>
/// Lista paginada de solicitudes
/// </summary>
public record PrivacyRequestsListDto(
    List<PrivacyRequestHistoryDto> Requests,
    int TotalCount,
    int Page,
    int PageSize
);

#endregion

#region Full Data Export DTO

/// <summary>
/// Datos completos del usuario para exportación
/// </summary>
public record UserFullDataDto(
    DateTime ExportDate,
    string ExportVersion,
    UserProfileExportDto Profile,
    UserActivityExportDto Activity,
    UserTransactionsExportDto Transactions,
    UserMessagesExportDto Messages,
    UserSettingsExportDto Settings,
    DataSharingInfoDto DataSharing
);

public record UserProfileExportDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? ProfilePicture,
    string? City,
    string? Province,
    string? BusinessName,
    string? BusinessPhone,
    string? BusinessAddress,
    string? RNC,
    string AccountType,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    bool EmailVerified
);

public record UserActivityExportDto(
    List<SearchHistoryExportDto> SearchHistory,
    List<VehicleViewExportDto> VehicleViews,
    List<FavoriteExportDto> Favorites,
    List<AlertExportDto> Alerts,
    List<SessionExportDto> Sessions
);

public record SearchHistoryExportDto(string? Query, string? Filters, DateTime SearchedAt);
public record VehicleViewExportDto(Guid VehicleId, string? VehicleTitle, DateTime ViewedAt);
public record FavoriteExportDto(Guid VehicleId, string? VehicleTitle, DateTime SavedAt, string? Notes);
public record AlertExportDto(Guid AlertId, string? Type, string? Criteria, DateTime CreatedAt);
public record SessionExportDto(string? IpAddress, string? Device, string? Location, DateTime LoginAt);

public record UserTransactionsExportDto(
    List<PaymentExportDto> Payments,
    List<InvoiceExportDto> Invoices,
    List<SubscriptionExportDto> Subscriptions
);

public record PaymentExportDto(string? PaymentId, decimal Amount, string? Currency, string? Status, string? Method, DateTime PaidAt);
public record InvoiceExportDto(string? InvoiceNumber, decimal Amount, string? Status, DateTime IssuedAt);
public record SubscriptionExportDto(string? Plan, string? Status, DateTime StartedAt, DateTime? EndsAt);

public record UserMessagesExportDto(
    int TotalConversations,
    int TotalMessages,
    List<ConversationExportDto> Conversations
);

public record ConversationExportDto(Guid ConversationId, string? WithUser, string? Subject, int MessageCount, DateTime LastMessageAt);

public record UserSettingsExportDto(
    CommunicationSettingsExportDto CommunicationPreferences,
    PrivacySettingsExportDto PrivacyPreferences
);

public record CommunicationSettingsExportDto(
    bool EmailActivityNotifications,
    bool EmailListingUpdates,
    bool EmailNewsletter,
    bool EmailPromotions,
    bool SmsAlerts,
    bool SmsPromotions,
    bool PushNewMessages,
    bool PushPriceChanges,
    bool PushRecommendations
);

public record PrivacySettingsExportDto(
    bool AllowProfiling,
    bool AllowThirdPartySharing,
    bool AllowAnalytics,
    bool AllowRetargeting
);

public record DataSharingInfoDto(
    string Description,
    List<ThirdPartyShareDto> ThirdParties
);

public record ThirdPartyShareDto(string? Name, string? Purpose, string? DataShared, DateTime? SharedAt);

#endregion

#region Export Download DTO

/// <summary>
/// Resultado de descarga de exportación
/// </summary>
public record ExportDownloadDto(
    byte[] FileContent,
    string FileName,
    string ContentType
);

#endregion

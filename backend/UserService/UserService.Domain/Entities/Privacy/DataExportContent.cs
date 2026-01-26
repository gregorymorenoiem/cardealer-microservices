using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities.Privacy;

/// <summary>
/// Modelo para el contenido exportado de datos del usuario
/// </summary>
public class DataExportContent
{
    public DateTime ExportDate { get; set; } = DateTime.UtcNow;
    public string ExportVersion { get; set; } = "1.0";
    public UserProfileData Profile { get; set; } = new();
    public UserActivityData Activity { get; set; } = new();
    public UserTransactionsData Transactions { get; set; } = new();
    public UserMessagesData Messages { get; set; } = new();
    public UserSettingsData Settings { get; set; } = new();
    public DataSharingInfo DataSharing { get; set; } = new();
}

public class UserProfileData
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePicture { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? BusinessName { get; set; }
    public string? BusinessPhone { get; set; }
    public string? BusinessAddress { get; set; }
    public string? RNC { get; set; }
    public string? AccountType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
}

public class UserActivityData
{
    public List<SearchHistoryItem> SearchHistory { get; set; } = new();
    public List<VehicleViewItem> VehicleViews { get; set; } = new();
    public List<FavoriteItem> Favorites { get; set; } = new();
    public List<AlertItem> Alerts { get; set; } = new();
    public List<SessionItem> Sessions { get; set; } = new();
}

public class SearchHistoryItem
{
    public string? Query { get; set; }
    public string? Filters { get; set; }
    public DateTime SearchedAt { get; set; }
}

public class VehicleViewItem
{
    public Guid VehicleId { get; set; }
    public string? VehicleTitle { get; set; }
    public DateTime ViewedAt { get; set; }
}

public class FavoriteItem
{
    public Guid VehicleId { get; set; }
    public string? VehicleTitle { get; set; }
    public DateTime SavedAt { get; set; }
    public string? Notes { get; set; }
}

public class AlertItem
{
    public Guid AlertId { get; set; }
    public string? Type { get; set; }
    public string? Criteria { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SessionItem
{
    public string? IpAddress { get; set; }
    public string? Device { get; set; }
    public string? Location { get; set; }
    public DateTime LoginAt { get; set; }
}

public class UserTransactionsData
{
    public List<PaymentItem> Payments { get; set; } = new();
    public List<InvoiceItem> Invoices { get; set; } = new();
    public List<SubscriptionItem> Subscriptions { get; set; } = new();
}

public class PaymentItem
{
    public string? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? Status { get; set; }
    public string? Method { get; set; }
    public DateTime PaidAt { get; set; }
}

public class InvoiceItem
{
    public string? InvoiceNumber { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public DateTime IssuedAt { get; set; }
}

public class SubscriptionItem
{
    public string? Plan { get; set; }
    public string? Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndsAt { get; set; }
}

public class UserMessagesData
{
    public int TotalConversations { get; set; }
    public int TotalMessages { get; set; }
    public List<ConversationSummary> Conversations { get; set; } = new();
}

public class ConversationSummary
{
    public Guid ConversationId { get; set; }
    public string? WithUser { get; set; }
    public string? Subject { get; set; }
    public int MessageCount { get; set; }
    public DateTime LastMessageAt { get; set; }
}

public class UserSettingsData
{
    public CommunicationPreferencesExport? CommunicationPreferences { get; set; }
    public PrivacyPreferencesExport? PrivacyPreferences { get; set; }
}

public class CommunicationPreferencesExport
{
    public bool EmailActivityNotifications { get; set; }
    public bool EmailListingUpdates { get; set; }
    public bool EmailNewsletter { get; set; }
    public bool EmailPromotions { get; set; }
    public bool SmsAlerts { get; set; }
    public bool SmsPromotions { get; set; }
    public bool PushNewMessages { get; set; }
    public bool PushPriceChanges { get; set; }
    public bool PushRecommendations { get; set; }
}

public class PrivacyPreferencesExport
{
    public bool AllowProfiling { get; set; }
    public bool AllowThirdPartySharing { get; set; }
    public bool AllowAnalytics { get; set; }
    public bool AllowRetargeting { get; set; }
}

public class DataSharingInfo
{
    public string Description { get; set; } = "Información sobre cómo se comparten tus datos";
    public List<ThirdPartyShare> ThirdParties { get; set; } = new();
}

public class ThirdPartyShare
{
    public string? Name { get; set; }
    public string? Purpose { get; set; }
    public string? DataShared { get; set; }
    public DateTime? SharedAt { get; set; }
}

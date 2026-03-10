using System;
using System.Threading;
using System.Threading.Tasks;
using CarDealer.Contracts.Events.User;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Privacy;
using UserService.Domain.Entities.Privacy;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.Privacy.UpdateCommunicationPreferences;

/// <summary>
/// Command para actualizar preferencias de comunicación
/// </summary>
public record UpdateCommunicationPreferencesCommand(
    Guid UserId,
    UpdateCommunicationPreferencesDto Preferences
) : IRequest<CommunicationPreferencesDto>;

/// <summary>
/// Handler para UpdateCommunicationPreferencesCommand
/// </summary>
public class UpdateCommunicationPreferencesCommandHandler : IRequestHandler<UpdateCommunicationPreferencesCommand, CommunicationPreferencesDto>
{
    private readonly ICommunicationPreferenceRepository _repository;
    private readonly IConsentRecordRepository _consentRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UpdateCommunicationPreferencesCommandHandler> _logger;

    public UpdateCommunicationPreferencesCommandHandler(
        ICommunicationPreferenceRepository repository,
        IConsentRecordRepository consentRepository,
        IEventPublisher eventPublisher,
        ILogger<UpdateCommunicationPreferencesCommandHandler> logger)
    {
        _repository = repository;
        _consentRepository = consentRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<CommunicationPreferencesDto> Handle(UpdateCommunicationPreferencesCommand request, CancellationToken cancellationToken)
    {
        var prefs = request.Preferences;

        // Load existing or create with defaults
        var existing = await _repository.GetByUserIdAsync(request.UserId);
        var entity = existing ?? new CommunicationPreference { UserId = request.UserId };

        // Apply only non-null values (partial update)
        if (prefs.EmailActivityNotifications.HasValue) entity.EmailActivityNotifications = prefs.EmailActivityNotifications.Value;
        if (prefs.EmailListingUpdates.HasValue) entity.EmailListingUpdates = prefs.EmailListingUpdates.Value;
        if (prefs.EmailNewsletter.HasValue) entity.EmailNewsletter = prefs.EmailNewsletter.Value;
        if (prefs.EmailPromotions.HasValue) entity.EmailPromotions = prefs.EmailPromotions.Value;
        if (prefs.EmailPriceAlerts.HasValue) entity.EmailPriceAlerts = prefs.EmailPriceAlerts.Value;
        if (prefs.SmsPriceAlerts.HasValue) entity.SmsPriceAlerts = prefs.SmsPriceAlerts.Value;
        if (prefs.SmsPromotions.HasValue) entity.SmsPromotions = prefs.SmsPromotions.Value;
        if (prefs.PushNewMessages.HasValue) entity.PushNewMessages = prefs.PushNewMessages.Value;
        if (prefs.PushPriceChanges.HasValue) entity.PushPriceChanges = prefs.PushPriceChanges.Value;
        if (prefs.PushRecommendations.HasValue) entity.PushRecommendations = prefs.PushRecommendations.Value;
        if (prefs.WhatsAppTransactional.HasValue) entity.WhatsAppTransactional = prefs.WhatsAppTransactional.Value;
        if (prefs.WhatsAppMarketing.HasValue) entity.WhatsAppMarketing = prefs.WhatsAppMarketing.Value;
        if (prefs.WhatsAppPriceAlerts.HasValue) entity.WhatsAppPriceAlerts = prefs.WhatsAppPriceAlerts.Value;
        if (prefs.AllowProfiling.HasValue) entity.AllowProfiling = prefs.AllowProfiling.Value;
        if (prefs.AllowThirdPartySharing.HasValue) entity.AllowThirdPartySharing = prefs.AllowThirdPartySharing.Value;
        if (prefs.AllowAnalytics.HasValue) entity.AllowAnalytics = prefs.AllowAnalytics.Value;
        if (prefs.AllowRetargeting.HasValue) entity.AllowRetargeting = prefs.AllowRetargeting.Value;

        var saved = await _repository.UpsertAsync(entity);

        // ════════════════════════════════════════════════════════════════
        // CONSENT AUDIT — Ley 172-13 Art. 27 compliance
        // Record each marketing-related preference change with timestamp
        // ════════════════════════════════════════════════════════════════
        var consentChanges = new List<(string Type, bool? Value)>
        {
            ("EmailNewsletter", prefs.EmailNewsletter),
            ("EmailPromotions", prefs.EmailPromotions),
            ("SmsPromotions", prefs.SmsPromotions),
            ("WhatsAppMarketing", prefs.WhatsAppMarketing),
            ("WhatsAppPriceAlerts", prefs.WhatsAppPriceAlerts),
            ("PushRecommendations", prefs.PushRecommendations),
            ("AllowRetargeting", prefs.AllowRetargeting),
            ("AllowThirdPartySharing", prefs.AllowThirdPartySharing),
        };

        foreach (var (type, value) in consentChanges)
        {
            if (value.HasValue)
            {
                await _consentRepository.AddAsync(new ConsentRecord
                {
                    UserId = request.UserId,
                    ConsentType = type,
                    Granted = value.Value,
                    Source = "web_settings"
                });
            }
        }

        _logger.LogInformation("Updated communication preferences for UserId={UserId}", request.UserId);

        // Publish event so NotificationService can create an in-app confirmation
        try
        {
            await _eventPublisher.PublishAsync(new UserSettingsChangedEvent
            {
                UserId = request.UserId,
                ChangeType = "preferences"
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            // Non-critical: preferences were saved; event failure should not break the response
            _logger.LogWarning(ex, "Failed to publish UserSettingsChangedEvent for UserId={UserId}", request.UserId);
        }

        return new CommunicationPreferencesDto(
            Email: new EmailPreferencesDto(
                ActivityNotifications: saved.EmailActivityNotifications,
                ListingUpdates: saved.EmailListingUpdates,
                Newsletter: saved.EmailNewsletter,
                Promotions: saved.EmailPromotions,
                PriceAlerts: saved.EmailPriceAlerts
            ),
            Sms: new SmsPreferencesDto(
                VerificationCodes: true,
                PriceAlerts: saved.SmsPriceAlerts,
                Promotions: saved.SmsPromotions
            ),
            Push: new PushPreferencesDto(
                NewMessages: saved.PushNewMessages,
                PriceChanges: saved.PushPriceChanges,
                Recommendations: saved.PushRecommendations
            ),
            WhatsApp: new WhatsAppPreferencesDto(
                Transactional: saved.WhatsAppTransactional,
                Marketing: saved.WhatsAppMarketing,
                PriceAlerts: saved.WhatsAppPriceAlerts
            ),
            Privacy: new PrivacyPreferencesDto(
                AllowProfiling: saved.AllowProfiling,
                AllowThirdPartySharing: saved.AllowThirdPartySharing,
                AllowAnalytics: saved.AllowAnalytics,
                AllowRetargeting: saved.AllowRetargeting
            ),
            LastUpdated: saved.UpdatedAt
        );
    }
}

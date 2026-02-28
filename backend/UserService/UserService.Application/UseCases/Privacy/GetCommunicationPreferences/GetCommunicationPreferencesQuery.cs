using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UserService.Application.DTOs.Privacy;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.Privacy.GetCommunicationPreferences;

/// <summary>
/// Query para obtener preferencias de comunicación (Oposición ARCO)
/// </summary>
public record GetCommunicationPreferencesQuery(Guid UserId) : IRequest<CommunicationPreferencesDto>;

/// <summary>
/// Handler para GetCommunicationPreferencesQuery
/// </summary>
public class GetCommunicationPreferencesQueryHandler : IRequestHandler<GetCommunicationPreferencesQuery, CommunicationPreferencesDto>
{
    private readonly ICommunicationPreferenceRepository _repository;

    public GetCommunicationPreferencesQueryHandler(ICommunicationPreferenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<CommunicationPreferencesDto> Handle(GetCommunicationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var prefs = await _repository.GetByUserIdAsync(request.UserId);

        // If no record yet, return sensible defaults (same as entity defaults)
        return new CommunicationPreferencesDto(
            Email: new EmailPreferencesDto(
                ActivityNotifications: prefs?.EmailActivityNotifications ?? true,
                ListingUpdates: prefs?.EmailListingUpdates ?? true,
                Newsletter: prefs?.EmailNewsletter ?? false,
                Promotions: prefs?.EmailPromotions ?? false,
                PriceAlerts: prefs?.EmailPriceAlerts ?? true
            ),
            Sms: new SmsPreferencesDto(
                VerificationCodes: true, // Always true (mandatory)
                PriceAlerts: prefs?.SmsPriceAlerts ?? false,
                Promotions: prefs?.SmsPromotions ?? false
            ),
            Push: new PushPreferencesDto(
                NewMessages: prefs?.PushNewMessages ?? true,
                PriceChanges: prefs?.PushPriceChanges ?? true,
                Recommendations: prefs?.PushRecommendations ?? false
            ),
            Privacy: new PrivacyPreferencesDto(
                AllowProfiling: prefs?.AllowProfiling ?? true,
                AllowThirdPartySharing: prefs?.AllowThirdPartySharing ?? false,
                AllowAnalytics: prefs?.AllowAnalytics ?? true,
                AllowRetargeting: prefs?.AllowRetargeting ?? false
            ),
            LastUpdated: prefs?.UpdatedAt ?? DateTime.UtcNow
        );
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UserService.Application.DTOs.Privacy;

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
    public async Task<CommunicationPreferencesDto> Handle(UpdateCommunicationPreferencesCommand request, CancellationToken cancellationToken)
    {
        // TODO: Implementar actualización en DB
        // 1. Buscar o crear CommunicationPreference
        // 2. Actualizar solo los campos que no son null
        // 3. Guardar en DB
        // 4. Publicar evento de cambio de preferencias
        
        await Task.CompletedTask;
        
        var prefs = request.Preferences;
        
        return new CommunicationPreferencesDto(
            Email: new EmailPreferencesDto(
                ActivityNotifications: prefs.EmailActivityNotifications ?? true,
                ListingUpdates: prefs.EmailListingUpdates ?? true,
                Newsletter: prefs.EmailNewsletter ?? false,
                Promotions: prefs.EmailPromotions ?? false,
                PriceAlerts: prefs.EmailPriceAlerts ?? true
            ),
            Sms: new SmsPreferencesDto(
                VerificationCodes: true,
                PriceAlerts: prefs.SmsPriceAlerts ?? false,
                Promotions: prefs.SmsPromotions ?? false
            ),
            Push: new PushPreferencesDto(
                NewMessages: prefs.PushNewMessages ?? true,
                PriceChanges: prefs.PushPriceChanges ?? true,
                Recommendations: prefs.PushRecommendations ?? false
            ),
            Privacy: new PrivacyPreferencesDto(
                AllowProfiling: prefs.AllowProfiling ?? true,
                AllowThirdPartySharing: prefs.AllowThirdPartySharing ?? false,
                AllowAnalytics: prefs.AllowAnalytics ?? true,
                AllowRetargeting: prefs.AllowRetargeting ?? false
            ),
            LastUpdated: DateTime.UtcNow
        );
    }
}

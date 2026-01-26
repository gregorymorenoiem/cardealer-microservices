using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UserService.Application.DTOs.Privacy;

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
    public async Task<CommunicationPreferencesDto> Handle(GetCommunicationPreferencesQuery request, CancellationToken cancellationToken)
    {
        // TODO: Buscar preferencias del usuario en DB
        await Task.CompletedTask;
        
        return new CommunicationPreferencesDto(
            Email: new EmailPreferencesDto(
                ActivityNotifications: true,
                ListingUpdates: true,
                Newsletter: false,
                Promotions: false,
                PriceAlerts: true
            ),
            Sms: new SmsPreferencesDto(
                VerificationCodes: true, // Siempre true
                PriceAlerts: false,
                Promotions: false
            ),
            Push: new PushPreferencesDto(
                NewMessages: true,
                PriceChanges: true,
                Recommendations: false
            ),
            Privacy: new PrivacyPreferencesDto(
                AllowProfiling: true,
                AllowThirdPartySharing: false,
                AllowAnalytics: true,
                AllowRetargeting: false
            ),
            LastUpdated: DateTime.UtcNow.AddDays(-30)
        );
    }
}

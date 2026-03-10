namespace NotificationService.Application.Interfaces;

/// <summary>
/// Client para verificar consentimiento del usuario antes de enviar notificaciones de marketing.
/// Llama al UserService vía HTTP para obtener las preferencias de comunicación.
/// Cumplimiento: Ley 172-13 Art. 27 + Meta Business Policy (WhatsApp opt-in).
/// </summary>
public interface IUserConsentClient
{
    /// <summary>
    /// Verifica si el usuario ha otorgado consentimiento para recibir emails de marketing.
    /// </summary>
    Task<bool> IsEmailMarketingAllowedAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Verifica si el usuario ha otorgado consentimiento para recibir WhatsApp de marketing.
    /// </summary>
    Task<bool> IsWhatsAppMarketingAllowedAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Verifica si el usuario ha otorgado consentimiento para recibir SMS de marketing.
    /// </summary>
    Task<bool> IsSmsMarketingAllowedAsync(Guid userId, CancellationToken ct = default);
}

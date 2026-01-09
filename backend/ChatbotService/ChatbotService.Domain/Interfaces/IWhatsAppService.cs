using ChatbotService.Domain.Entities;

namespace ChatbotService.Domain.Interfaces;

/// <summary>
/// Servicio para integración con WhatsApp Business API (Twilio)
/// </summary>
public interface IWhatsAppService
{
    /// <summary>
    /// Envía mensaje de handoff al dealer por WhatsApp
    /// </summary>
    Task<WhatsAppHandoff> SendHandoffMessageAsync(
        string toPhoneNumber,
        string dealerName,
        string userName,
        string userPhone,
        string vehicleDetails,
        string conversationSummary,
        int leadScore,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene estado de mensaje enviado
    /// </summary>
    Task<WhatsAppStatus> GetMessageStatusAsync(
        string messageId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Valida formato de número de teléfono WhatsApp
    /// </summary>
    bool IsValidWhatsAppNumber(string phoneNumber);
    
    /// <summary>
    /// Formatea número a formato WhatsApp (E.164)
    /// </summary>
    string FormatWhatsAppNumber(string phoneNumber);
}

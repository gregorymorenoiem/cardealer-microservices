using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StaffService.Application.Clients;

namespace StaffService.Infrastructure.Clients;

public class NotificationClient : INotificationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationClient> _logger;
    private readonly string _appBaseUrl;

    public NotificationClient(HttpClient httpClient, ILogger<NotificationClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _appBaseUrl = configuration["AppBaseUrl"] ?? "http://localhost:3000";
    }

    public async Task SendInvitationEmailAsync(
        string email,
        string invitationToken,
        string inviterName,
        string role,
        string? personalMessage,
        CancellationToken ct = default)
    {
        try
        {
            var invitationLink = $"{_appBaseUrl}/admin/equipo/invitacion/aceptar?token={invitationToken}";
            var messageSection = !string.IsNullOrWhiteSpace(personalMessage)
                ? $@"<tr><td style=""padding:16px 24px;background:#f0f9ff;border-radius:8px;margin:16px 0"">
                        <p style=""margin:0;color:#0369a1;font-style:italic"">""{ System.Net.WebUtility.HtmlEncode(personalMessage)}""</p>
                        <p style=""margin:8px 0 0;color:#64748b;font-size:13px"">— {System.Net.WebUtility.HtmlEncode(inviterName)}</p>
                      </td></tr>"
                : "";

            var subject = $"Invitación al equipo OKLA — Rol: {role}";
            var body = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""margin:0;padding:0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;background:#f8fafc"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;margin:0 auto;background:#ffffff;border-radius:12px;overflow:hidden;margin-top:24px;box-shadow:0 1px 3px rgba(0,0,0,0.1)"">
  <tr><td style=""background:linear-gradient(135deg,#1e40af,#3b82f6);padding:32px 24px;text-align:center"">
    <h1 style=""margin:0;color:#ffffff;font-size:24px;font-weight:700"">OKLA</h1>
    <p style=""margin:8px 0 0;color:#bfdbfe;font-size:14px"">Plataforma de Vehículos</p>
  </td></tr>
  <tr><td style=""padding:32px 24px"">
    <h2 style=""margin:0 0 16px;color:#1e293b;font-size:20px"">¡Has sido invitado al equipo!</h2>
    <p style=""color:#475569;line-height:1.6;margin:0 0 16px"">
      <strong>{System.Net.WebUtility.HtmlEncode(inviterName)}</strong> te ha invitado a unirte al equipo de OKLA con el rol de <strong>{System.Net.WebUtility.HtmlEncode(role)}</strong>.
    </p>
  </td></tr>
  {messageSection}
  <tr><td style=""padding:8px 24px 32px;text-align:center"">
    <a href=""{invitationLink}"" style=""display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;padding:14px 32px;border-radius:8px;font-weight:600;font-size:16px"">
      Aceptar Invitación
    </a>
  </td></tr>
  <tr><td style=""padding:0 24px 24px"">
    <p style=""color:#94a3b8;font-size:13px;text-align:center;margin:0"">
      Esta invitación expira en <strong>7 días</strong>. Si no esperabas esta invitación, puedes ignorar este email.
    </p>
  </td></tr>
  <tr><td style=""background:#f1f5f9;padding:16px 24px;text-align:center"">
    <p style=""margin:0;color:#94a3b8;font-size:12px"">© {DateTime.UtcNow.Year} OKLA — okla.com.do</p>
  </td></tr>
</table>
</body>
</html>";

            var response = await _httpClient.PostAsJsonAsync("/api/notifications/email", new
            {
                to = email,
                subject,
                body,
                isHtml = true,
                metadata = new Dictionary<string, object>
                {
                    ["type"] = "staff_invitation",
                    ["role"] = role,
                    ["inviterName"] = inviterName
                }
            }, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Invitation email sent successfully to {Email}", email);
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Failed to send invitation email to {Email}. Status: {Status}. Response: {Response}",
                    email, response.StatusCode, errorBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invitation email to {Email}", email);
        }
    }

    public async Task SendWelcomeEmailAsync(
        string email,
        string staffName,
        string role,
        CancellationToken ct = default)
    {
        try
        {
            var loginUrl = $"{_appBaseUrl}/login";
            var subject = $"¡Bienvenido al equipo OKLA, {staffName}!";
            var body = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""margin:0;padding:0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;background:#f8fafc"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;margin:0 auto;background:#ffffff;border-radius:12px;overflow:hidden;margin-top:24px;box-shadow:0 1px 3px rgba(0,0,0,0.1)"">
  <tr><td style=""background:linear-gradient(135deg,#059669,#10b981);padding:32px 24px;text-align:center"">
    <h1 style=""margin:0;color:#ffffff;font-size:24px;font-weight:700"">OKLA</h1>
    <p style=""margin:8px 0 0;color:#a7f3d0;font-size:14px"">¡Bienvenido al equipo!</p>
  </td></tr>
  <tr><td style=""padding:32px 24px"">
    <h2 style=""margin:0 0 16px;color:#1e293b;font-size:20px"">Hola {System.Net.WebUtility.HtmlEncode(staffName)} 👋</h2>
    <p style=""color:#475569;line-height:1.6;margin:0 0 16px"">
      Tu cuenta ha sido creada exitosamente. Ahora eres parte del equipo OKLA con el rol de <strong>{System.Net.WebUtility.HtmlEncode(role)}</strong>.
    </p>
    <p style=""color:#475569;line-height:1.6;margin:0 0 24px"">Ya puedes acceder al panel de administración.</p>
  </td></tr>
  <tr><td style=""padding:0 24px 32px;text-align:center"">
    <a href=""{loginUrl}"" style=""display:inline-block;background:#059669;color:#ffffff;text-decoration:none;padding:14px 32px;border-radius:8px;font-weight:600;font-size:16px"">
      Iniciar Sesión
    </a>
  </td></tr>
  <tr><td style=""background:#f1f5f9;padding:16px 24px;text-align:center"">
    <p style=""margin:0;color:#94a3b8;font-size:12px"">© {DateTime.UtcNow.Year} OKLA — okla.com.do</p>
  </td></tr>
</table>
</body>
</html>";

            var response = await _httpClient.PostAsJsonAsync("/api/notifications/email", new
            {
                to = email,
                subject,
                body,
                isHtml = true,
                metadata = new Dictionary<string, object>
                {
                    ["type"] = "staff_welcome",
                    ["role"] = role
                }
            }, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to send welcome email to {Email}", email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", email);
        }
    }

    public async Task SendStatusChangeEmailAsync(
        string email,
        string staffName,
        string oldStatus,
        string newStatus,
        string? reason,
        CancellationToken ct = default)
    {
        try
        {
            var subject = $"OKLA — Tu estado ha cambiado a {newStatus}";
            var reasonText = !string.IsNullOrWhiteSpace(reason) ? reason : "No se proporcionó una razón";
            var body = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""margin:0;padding:0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;background:#f8fafc"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;margin:0 auto;background:#ffffff;border-radius:12px;overflow:hidden;margin-top:24px;box-shadow:0 1px 3px rgba(0,0,0,0.1)"">
  <tr><td style=""background:linear-gradient(135deg,#d97706,#f59e0b);padding:32px 24px;text-align:center"">
    <h1 style=""margin:0;color:#ffffff;font-size:24px;font-weight:700"">OKLA</h1>
    <p style=""margin:8px 0 0;color:#fef3c7;font-size:14px"">Actualización de estado</p>
  </td></tr>
  <tr><td style=""padding:32px 24px"">
    <h2 style=""margin:0 0 16px;color:#1e293b;font-size:20px"">Hola {System.Net.WebUtility.HtmlEncode(staffName)}</h2>
    <p style=""color:#475569;line-height:1.6;margin:0 0 16px"">Tu estado en el equipo ha sido actualizado:</p>
    <table style=""width:100%;border-collapse:collapse;margin:16px 0"">
      <tr>
        <td style=""padding:12px 16px;background:#fef2f2;border-radius:8px 0 0 8px;text-align:center"">
          <p style=""margin:0;color:#94a3b8;font-size:12px"">Anterior</p>
          <p style=""margin:4px 0 0;color:#ef4444;font-weight:600"">{System.Net.WebUtility.HtmlEncode(oldStatus)}</p>
        </td>
        <td style=""padding:12px 8px;text-align:center;color:#94a3b8"">→</td>
        <td style=""padding:12px 16px;background:#f0fdf4;border-radius:0 8px 8px 0;text-align:center"">
          <p style=""margin:0;color:#94a3b8;font-size:12px"">Nuevo</p>
          <p style=""margin:4px 0 0;color:#22c55e;font-weight:600"">{System.Net.WebUtility.HtmlEncode(newStatus)}</p>
        </td>
      </tr>
    </table>
    <p style=""color:#475569;line-height:1.6;margin:16px 0 0""><strong>Razón:</strong> {System.Net.WebUtility.HtmlEncode(reasonText)}</p>
  </td></tr>
  <tr><td style=""background:#f1f5f9;padding:16px 24px;text-align:center"">
    <p style=""margin:0;color:#94a3b8;font-size:12px"">© {DateTime.UtcNow.Year} OKLA — okla.com.do</p>
  </td></tr>
</table>
</body>
</html>";

            var response = await _httpClient.PostAsJsonAsync("/api/notifications/email", new
            {
                to = email,
                subject,
                body,
                isHtml = true,
                metadata = new Dictionary<string, object>
                {
                    ["type"] = "staff_status_change",
                    ["oldStatus"] = oldStatus,
                    ["newStatus"] = newStatus
                }
            }, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to send status change email to {Email}", email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending status change email to {Email}", email);
        }
    }
}

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces;
using ServiceDiscovery.Application.Interfaces;

namespace UserService.Infrastructure.External
{
    public class NotificationServiceClient : INotificationServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationServiceClient> _logger;
        private readonly IServiceDiscovery _serviceDiscovery;

        public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger, IServiceDiscovery serviceDiscovery)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serviceDiscovery = serviceDiscovery;
        }

        private async Task<string> GetServiceUrlAsync()
        {
            try
            {
                var instance = await _serviceDiscovery.FindServiceInstanceAsync("NotificationService");
                return instance != null ? $"http://{instance.Host}:{instance.Port}" : "http://notificationservice:80";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error resolving NotificationService from Consul, using fallback");
                return "http://notificationservice:80";
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName, string lastName)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var notification = new
                {
                    To = email,
                    Subject = "Welcome to CarDealer",
                    Body = $"Hello {firstName} {lastName},\n\nWelcome to CarDealer! Your account has been successfully created.",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Welcome email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send welcome email to {Email}", email);
            }
        }

        public async Task SendRoleAssignedNotificationAsync(string email, string roleName)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var notification = new
                {
                    To = email,
                    Subject = "Role Assigned",
                    Body = $"A new role '{roleName}' has been assigned to your account.",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Role assignment notification sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send role assignment notification to {Email}", email);
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var notification = new
                {
                    To = email,
                    Subject = "Password Reset Request",
                    Body = $"Click the following link to reset your password: https://cardealer.com/reset-password?token={resetToken}",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Password reset email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send password reset email to {Email}", email);
            }
        }
        
        // ========================================
        // DEALER ONBOARDING EMAILS
        // ========================================
        
        public async Task SendDealerVerificationEmailAsync(
            string email, 
            string businessName, 
            string verificationToken,
            DateTime tokenExpiry)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var verificationUrl = $"https://okla.com.do/dealer/verify-email?token={verificationToken}";
                var expiryFormatted = tokenExpiry.ToString("dd/MM/yyyy HH:mm");
                
                var notification = new
                {
                    To = email,
                    Subject = "OKLA - Verifica tu email para continuar con el registro",
                    Body = $@"Hola {businessName},

¡Gracias por registrarte como dealer en OKLA!

Para continuar con tu registro, por favor verifica tu email haciendo clic en el siguiente enlace:

{verificationUrl}

Este enlace expira el {expiryFormatted} (UTC).

Si no solicitaste este registro, puedes ignorar este email.

Saludos,
El equipo de OKLA",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Dealer verification email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send dealer verification email to {Email}", email);
            }
        }
        
        public async Task NotifyAdminsNewDealerApplicationAsync(
            string businessName,
            string rnc,
            string email,
            Guid dealerId)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var reviewUrl = $"https://okla.com.do/admin/dealers/{dealerId}";
                
                var notification = new
                {
                    To = "admin@okla.com.do", // Lista de admins
                    Subject = $"OKLA - Nueva solicitud de dealer: {businessName}",
                    Body = $@"Se ha recibido una nueva solicitud de dealer:

Negocio: {businessName}
RNC: {rnc}
Email: {email}

Los documentos han sido subidos y están pendientes de revisión.

Revisar solicitud: {reviewUrl}

Este es un mensaje automático del sistema OKLA.",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Admin notification sent for dealer {DealerId}", dealerId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify admins about new dealer application {DealerId}", dealerId);
            }
        }
        
        public async Task SendDealerApprovalEmailAsync(
            string email, 
            string businessName,
            string requestedPlan)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var checkoutUrl = "https://okla.com.do/dealer/checkout";
                
                var notification = new
                {
                    To = email,
                    Subject = "OKLA - ¡Tu solicitud ha sido aprobada!",
                    Body = $@"¡Felicitaciones {businessName}!

Tu solicitud para convertirte en dealer de OKLA ha sido aprobada.

Plan solicitado: {requestedPlan}

El siguiente paso es configurar tu método de pago para activar tu cuenta:

{checkoutUrl}

Una vez configurado el pago, tu cuenta será activada automáticamente y podrás comenzar a publicar vehículos.

¡Bienvenido a la familia OKLA!

Saludos,
El equipo de OKLA",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Dealer approval email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send dealer approval email to {Email}", email);
            }
        }
        
        public async Task SendDealerRejectionEmailAsync(
            string email, 
            string businessName,
            string rejectionReason)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                
                var notification = new
                {
                    To = email,
                    Subject = "OKLA - Actualización sobre tu solicitud de dealer",
                    Body = $@"Estimado {businessName},

Lamentamos informarte que tu solicitud para convertirte en dealer en OKLA no ha sido aprobada en esta ocasión.

Razón: {rejectionReason}

Si crees que esto es un error o tienes documentación adicional que pueda ayudar, por favor contáctanos en soporte@okla.com.do

Gracias por tu interés en OKLA.

Saludos,
El equipo de OKLA",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Dealer rejection email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send dealer rejection email to {Email}", email);
            }
        }
        
        public async Task SendDealerWelcomeEmailAsync(
            string email, 
            string businessName,
            string plan,
            bool isEarlyBird)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var dashboardUrl = "https://okla.com.do/dealer/dashboard";
                
                var earlyBirdMessage = isEarlyBird 
                    ? @"

🎉 ¡BENEFICIOS EARLY BIRD ACTIVADOS!
- 90 días de prueba gratuita
- 20% de descuento de por vida
- Badge de 'Miembro Fundador'
" 
                    : "";
                
                var notification = new
                {
                    To = email,
                    Subject = "OKLA - ¡Tu cuenta de dealer está activa!",
                    Body = $@"¡Bienvenido a OKLA, {businessName}!

Tu cuenta de dealer ha sido activada exitosamente.

Plan activo: {plan}
{earlyBirdMessage}
Ya puedes comenzar a:
- Publicar tus vehículos
- Gestionar tu inventario
- Recibir contactos de compradores
- Ver tus estadísticas

Accede a tu dashboard: {dashboardUrl}

¿Necesitas ayuda? Nuestro equipo de soporte está disponible en soporte@okla.com.do

¡Éxito en tus ventas!

El equipo de OKLA",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Dealer welcome email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send dealer welcome email to {Email}", email);
            }
        }

        // ========================================
        // ARCO PRIVACY EMAILS (Ley 172-13)
        // ========================================

        public async Task SendAccountDeletionConfirmationCodeAsync(
            string email,
            string firstName,
            string confirmationCode,
            DateTime gracePeriodEndsAt)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var gracePeriodFormatted = gracePeriodEndsAt.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-DO"));
                
                var notification = new
                {
                    To = email,
                    Subject = "OKLA - Código de confirmación para eliminar tu cuenta",
                    Body = $@"Hola {firstName},

Recibimos tu solicitud para eliminar tu cuenta en OKLA.

Tu código de confirmación es: {confirmationCode}

⚠️ IMPORTANTE:
- Este código expirará en 24 horas
- Tu cuenta será eliminada permanentemente el {gracePeriodFormatted} (15 días de gracia según Ley 172-13)
- Puedes cancelar esta solicitud en cualquier momento antes de esa fecha

Para confirmar la eliminación:
1. Ingresa a tu cuenta en OKLA
2. Ve a Configuración > Seguridad
3. Ingresa el código de confirmación: {confirmationCode}
4. Confirma con tu contraseña

Si no solicitaste eliminar tu cuenta, por favor ignora este correo o contacta a soporte inmediatamente.

Atentamente,
El equipo de OKLA",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Account deletion confirmation code sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send account deletion confirmation code to {Email}", email);
            }
        }

        public async Task SendAccountDeletionReminderAsync(
            string email,
            string firstName,
            DateTime deletionDate,
            int daysRemaining)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var deletionDateFormatted = deletionDate.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-DO"));
                
                var notification = new
                {
                    To = email,
                    Subject = $"OKLA - Tu cuenta será eliminada en {daysRemaining} día(s)",
                    Body = $@"Hola {firstName},

Te recordamos que tu cuenta en OKLA será eliminada permanentemente el {deletionDateFormatted}.

Faltan {daysRemaining} día(s) para la eliminación.

Si deseas conservar tu cuenta, puedes cancelar la solicitud de eliminación:
1. Ingresa a tu cuenta en OKLA
2. Ve a Configuración > Seguridad  
3. Haz clic en 'Cancelar solicitud de eliminación'

Una vez eliminada, no podrás recuperar:
- Tu historial de búsquedas
- Tus vehículos favoritos
- Tus alertas de precios
- Tus conversaciones con vendedores

Atentamente,
El equipo de OKLA",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Account deletion reminder sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send account deletion reminder to {Email}", email);
            }
        }

        public async Task SendAccountDeletedConfirmationAsync(
            string email,
            string firstName)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                
                var notification = new
                {
                    To = email,
                    Subject = "OKLA - Tu cuenta ha sido eliminada",
                    Body = $@"Hola {firstName},

Tu cuenta en OKLA ha sido eliminada exitosamente según tu solicitud.

Todos tus datos personales han sido eliminados de nuestros sistemas de acuerdo con la Ley 172-13 de Protección de Datos Personales de República Dominicana.

Si en el futuro deseas volver a utilizar OKLA, puedes crear una nueva cuenta.

Gracias por haber sido parte de nuestra comunidad.

Atentamente,
El equipo de OKLA",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Account deleted confirmation sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send account deleted confirmation to {Email}", email);
            }
        }
    }
}

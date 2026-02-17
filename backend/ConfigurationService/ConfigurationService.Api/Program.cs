using CarDealer.Shared.Middleware;
using ConfigurationService.Application.Interfaces;
using ConfigurationService.Infrastructure.Data;
using ConfigurationService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient for health checks
builder.Services.AddHttpClient();

// Database
builder.Services.AddDbContext<ConfigurationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg =>

// SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ConfigurationService.Application.Behaviors.ValidationBehavior<,>));
{
    cfg.RegisterServicesFromAssembly(typeof(ConfigurationService.Application.Commands.CreateConfigurationCommand).Assembly);
});

// Encryption — key MUST be provided via env var or config in production
var encryptionKey = builder.Configuration["Encryption:Key"]
    ?? Environment.GetEnvironmentVariable("ENCRYPTION_KEY")
    ?? throw new InvalidOperationException(
        "Encryption key not configured. Set Encryption:Key in appsettings or ENCRYPTION_KEY env var.");
builder.Services.AddSingleton<IEncryptionService>(new AesEncryptionService(encryptionKey));

// JWT Authentication — required for protected endpoints
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? throw new InvalidOperationException("JWT Key must be configured via environment/settings. Do NOT use hardcoded keys.");

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// Application Services
builder.Services.AddScoped<ConfigurationService.Application.Interfaces.IConfigurationManager, ConfigurationService.Infrastructure.Services.ConfigurationManager>();
builder.Services.AddScoped<ISecretManager, SecretManager>();
builder.Services.AddScoped<IFeatureFlagManager, FeatureFlagManager>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(, policy =>
    {
        var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        if (isDev)
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(
                    "https://okla.com.do",
                    "https://www.okla.com.do",
                    "https://api.okla.com.do")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
    db.Database.Migrate();
    await SeedDefaultConfigurations(db);

    var secretManager = scope.ServiceProvider.GetRequiredService<ISecretManager>();
    await SeedDefaultSecrets(secretManager);
}

// Configure the HTTP request pipeline
// OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "ConfigurationService" }));

app.MapControllers();

app.Run();

// Seed default platform configurations
static async Task SeedDefaultConfigurations(ConfigurationDbContext db)
{
    // Only seed if no configurations exist
    if (db.ConfigurationItems.Any())
        return;

    var env = "Development";
    var now = DateTime.UtcNow;
    var createdBy = "system";

    var defaults = new Dictionary<string, (string Value, string Description)>
    {
        // General - Identidad del sitio
        ["general.site_name"] = ("OKLA", "Nombre del sitio"),
        ["general.site_url"] = ("https://okla.com.do", "URL principal del sitio"),
        ["general.site_description"] = ("El marketplace de vehículos #1 de República Dominicana. Compra y vende con confianza.", "Descripción del sitio"),
        ["general.contact_email"] = ("info@okla.com.do", "Email de contacto principal"),
        ["general.support_email"] = ("soporte@okla.com.do", "Email de soporte técnico"),
        ["general.noreply_email"] = ("noreply@okla.com.do", "Email de no-reply para notificaciones"),
        ["general.legal_email"] = ("legal@okla.com.do", "Email para asuntos legales"),
        ["general.privacy_email"] = ("privacidad@okla.com.do", "Email para asuntos de privacidad"),
        ["general.support_phone"] = ("+1 (809) 555-1234", "Teléfono de soporte"),
        ["general.whatsapp_number"] = ("18095551234", "Número de WhatsApp (sin +)"),
        ["general.address"] = ("Av. Winston Churchill #1099, Torre Acrópolis, Piso 10, Santo Domingo, RD", "Dirección física"),
        ["general.business_hours"] = ("Lunes a Viernes: 8:00 AM - 6:00 PM | Sábados: 9:00 AM - 1:00 PM", "Horario de atención"),
        // General - Redes sociales
        ["general.social_facebook"] = ("https://facebook.com/okla", "Facebook URL"),
        ["general.social_instagram"] = ("https://instagram.com/okla", "Instagram URL"),
        ["general.social_twitter"] = ("https://twitter.com/okla", "Twitter/X URL"),
        ["general.social_youtube"] = ("https://youtube.com/okla", "YouTube URL"),

        // Pricing - Publicaciones
        ["pricing.basic_listing"] = ("0", "Precio publicación básica (DOP)"),
        ["pricing.featured_listing"] = ("1499", "Precio publicación destacada (DOP)"),
        ["pricing.premium_listing"] = ("2999", "Precio publicación premium (DOP)"),
        ["pricing.seller_premium_price"] = ("1699", "Precio publicación individual vendedor (DOP) ~$29 USD"),
        // Pricing - Planes Dealer
        ["pricing.dealer_starter"] = ("2899", "Plan Dealer Starter mensual (DOP) ~$49 USD"),
        ["pricing.dealer_pro"] = ("7499", "Plan Dealer Pro mensual (DOP) ~$129 USD"),
        ["pricing.dealer_enterprise"] = ("17499", "Plan Dealer Enterprise mensual (DOP) ~$299 USD"),
        // Pricing - Boosts
        ["pricing.boost_basic_price"] = ("499", "Precio boost básico (DOP) ~$8.50 USD"),
        ["pricing.boost_basic_days"] = ("3", "Duración boost básico en días"),
        ["pricing.boost_pro_price"] = ("999", "Precio boost pro (DOP) ~$17 USD"),
        ["pricing.boost_pro_days"] = ("7", "Duración boost pro en días"),
        ["pricing.boost_premium_price"] = ("1999", "Precio boost premium (DOP) ~$34 USD"),
        ["pricing.boost_premium_days"] = ("14", "Duración boost premium en días"),
        // Pricing - Duraciones
        ["pricing.basic_listing_days"] = ("30", "Duración publicación gratuita en días"),
        ["pricing.individual_listing_days"] = ("45", "Duración publicación individual pagada en días"),
        // Pricing - Límites por plan
        ["pricing.starter_max_vehicles"] = ("20", "Máximo de vehículos plan Starter"),
        ["pricing.pro_max_vehicles"] = ("75", "Máximo de vehículos plan Pro"),
        ["pricing.free_max_photos"] = ("10", "Fotos máximas publicación gratuita"),
        ["pricing.starter_max_photos"] = ("25", "Fotos máximas por vehículo plan Starter"),
        ["pricing.pro_max_photos"] = ("40", "Fotos máximas por vehículo plan Pro"),
        ["pricing.enterprise_max_photos"] = ("50", "Fotos máximas por vehículo plan Enterprise"),
        // Pricing - Comisiones e impuestos
        ["pricing.platform_commission"] = ("2.5", "Porcentaje de comisión sobre ventas"),
        ["pricing.itbis_percentage"] = ("18", "ITBIS porcentaje"),
        ["pricing.currency"] = ("DOP", "Moneda principal"),
        // Pricing - Early Bird
        ["pricing.early_bird_discount"] = ("25", "Porcentaje descuento Early Bird"),
        ["pricing.early_bird_deadline"] = ("2026-12-31", "Fecha límite oferta Early Bird (YYYY-MM-DD)"),
        ["pricing.early_bird_free_months"] = ("2", "Meses gratis Early Bird"),

        // =========================================================
        // Email (Resend / SendGrid / SMTP)
        // =========================================================
        ["email.provider"] = ("resend", "Proveedor de email activo (resend/sendgrid/smtp)"),
        // Resend — valores reales de Docker/appsettings
        // API Key es un secret cifrado, se seedea abajo en SeedDefaultSecrets
        ["email.resend_from_email"] = ("noreply@okla.com.do", "Resend - Email remitente"),
        ["email.resend_from_name"] = ("OKLA", "Resend - Nombre remitente"),
        // SendGrid
        // API Key es un secret cifrado
        ["email.sendgrid_from_email"] = ("notificaciones@shiftwaysolutions.com.do", "SendGrid - Email remitente"),
        ["email.sendgrid_from_name"] = ("CarDealer Dev", "SendGrid - Nombre remitente"),
        ["email.sendgrid_enable_tracking"] = ("false", "SendGrid - Habilitar tracking de apertura"),
        // SMTP genérico
        ["email.smtp_server"] = ("smtp.sendgrid.net", "Servidor SMTP"),
        ["email.smtp_port"] = ("587", "Puerto SMTP (587 TLS / 465 SSL)"),
        ["email.smtp_user"] = ("apikey", "Usuario SMTP"),
        // SMTP password es un secret cifrado
        ["email.smtp_use_tls"] = ("true", "Usar TLS para SMTP"),
        // General
        ["email.sender_email"] = ("no-reply@okla.com.do", "Email del remitente (general)"),
        ["email.sender_name"] = ("OKLA", "Nombre del remitente (general)"),
        ["email.reply_to_email"] = ("soporte@okla.com.do", "Email de Reply-To"),
        ["email.templates_path"] = ("./Templates", "Ruta a los templates de email"),
        ["email.daily_send_limit"] = ("0", "Límite diario de envíos (0 = sin límite)"),

        // =========================================================
        // SMS (Twilio)
        // =========================================================
        ["sms.enabled"] = ("false", "SMS habilitado"),
        ["sms.provider"] = ("twilio", "Proveedor SMS activo"),
        // Twilio credentials son secrets cifrados
        ["sms.twilio_from_number"] = ("+13476622382", "Número Twilio de envío"),
        // Twilio messaging service SID es un secret cifrado
        ["sms.daily_limit_per_user"] = ("10", "Límite diario de SMS por usuario"),
        ["sms.daily_global_limit"] = ("1000", "Límite diario global de SMS"),
        ["sms.default_country_code"] = ("+1-809", "Código de país por defecto (RD)"),
        ["sms.enable_verification_codes"] = ("true", "Enviar códigos de verificación por SMS"),
        ["sms.enable_payment_alerts"] = ("true", "Alertas de pago por SMS"),
        ["sms.enable_listing_alerts"] = ("false", "Alertas de listings por SMS"),
        ["sms.enable_marketing"] = ("false", "SMS de marketing"),

        // =========================================================
        // Push Notifications (Firebase)
        // =========================================================
        ["push.enabled"] = ("false", "Push notifications habilitadas"),
        ["push.provider"] = ("firebase", "Proveedor Push activo"),
        ["push.firebase_project_id"] = ("cargurus-dev", "Firebase Project ID"),
        ["push.firebase_service_account_path"] = ("/app/secrets/firebase-service-account.json", "Ruta al archivo de service account Firebase"),
        // Firebase server key es un secret cifrado
        ["push.default_ttl_seconds"] = ("86400", "TTL por defecto de push (segundos)"),
        ["push.default_priority"] = ("high", "Prioridad por defecto (high/normal)"),
        ["push.enable_new_messages"] = ("true", "Push para nuevos mensajes"),
        ["push.enable_price_alerts"] = ("true", "Push para alertas de precio"),
        ["push.enable_listing_updates"] = ("true", "Push para actualizaciones de listings"),
        ["push.enable_payment_status"] = ("true", "Push para estado de pagos"),

        // =========================================================
        // WhatsApp Business
        // =========================================================
        ["whatsapp.enabled"] = ("false", "WhatsApp habilitado"),
        ["whatsapp.provider"] = ("twilio", "Proveedor WhatsApp (twilio/meta)"),
        ["whatsapp.business_number"] = ("18095551234", "Número WhatsApp Business"),
        ["whatsapp.twilio_whatsapp_number"] = ("", "Número Twilio WhatsApp (whatsapp:+1...)"),
        ["whatsapp.meta_phone_number_id"] = ("", "Meta - Phone Number ID"),
        // Meta access token es un secret cifrado
        ["whatsapp.meta_business_account_id"] = ("", "Meta - Business Account ID"),
        ["whatsapp.welcome_template"] = ("okla_bienvenida", "Template de bienvenida"),
        ["whatsapp.verification_template"] = ("okla_verificacion", "Template de verificación"),
        ["whatsapp.payment_template"] = ("okla_pago_confirmado", "Template de pago confirmado"),
        ["whatsapp.enable_welcome_message"] = ("true", "Enviar mensaje de bienvenida"),
        ["whatsapp.enable_verification_codes"] = ("true", "Enviar códigos de verificación"),
        ["whatsapp.enable_payment_confirmations"] = ("true", "Enviar confirmaciones de pago"),
        ["whatsapp.enable_listing_notifications"] = ("false", "Enviar notificaciones de listings"),

        // =========================================================
        // Notifications (Admin toggles & channels)
        // =========================================================
        ["notifications.new_user_registered"] = ("true", "Notificar cuando se registra un nuevo usuario"),
        ["notifications.new_listing_pending"] = ("true", "Notificar cuando hay una nueva publicación pendiente"),
        ["notifications.new_dealer_registered"] = ("true", "Notificar cuando se registra un nuevo dealer"),
        ["notifications.user_report"] = ("true", "Notificar cuando se reporta un usuario"),
        ["notifications.payment_failed"] = ("true", "Notificar cuando un pago falla"),
        ["notifications.daily_summary"] = ("false", "Enviar resumen diario"),
        ["notifications.kyc_pending_review"] = ("true", "Notificar KYC pendiente de revisión"),
        ["notifications.system_errors"] = ("true", "Notificar errores del sistema"),
        ["notifications.admin_channel"] = ("email", "Canal principal de admin (email/sms/slack/teams)"),
        ["notifications.admin_email"] = ("admin@okla.com.do", "Email de administrador para alertas"),
        ["notifications.admin_phone"] = ("+18095551234", "Teléfono de admin para SMS críticos"),
        // Teams / Slack webhook URLs son secrets cifrados

        // Security
        ["security.max_login_attempts"] = ("5", "Intentos de login máximos"),
        ["security.lockout_duration_minutes"] = ("15", "Tiempo de bloqueo en minutos"),
        ["security.session_expiration_hours"] = ("24", "Expiración de sesión en horas"),
        ["security.min_password_length"] = ("8", "Largo mínimo de contraseña"),
        ["security.require_email_verification"] = ("true", "Requerir verificación de email"),
        ["security.allow_2fa"] = ("true", "Permitir autenticación de dos factores"),
        ["security.force_https"] = ("true", "Forzar HTTPS"),
        ["security.jwt_expires_minutes"] = ("60", "Expiración JWT en minutos"),
        ["security.refresh_token_days"] = ("7", "Días de vida del refresh token"),

        // Vehicles
        ["vehicles.max_images_per_listing"] = ("30", "Máximo de imágenes por publicación"),
        ["vehicles.listing_expiration_days"] = ("90", "Días antes de expirar una publicación"),
        ["vehicles.featured_duration_days"] = ("30", "Duración publicación destacada (días)"),
        ["vehicles.max_price_dop"] = ("100000000", "Precio máximo de publicación (DOP)"),
        ["vehicles.require_kyc_to_sell"] = ("true", "Requerir KYC para vender"),
        ["vehicles.pagination_default"] = ("20", "Resultados por página por defecto"),

        // KYC
        ["kyc.max_verification_attempts"] = ("3", "Intentos máximos de verificación"),
        ["kyc.verification_timeout_minutes"] = ("30", "Timeout de verificación (minutos)"),
        ["kyc.document_expiration_days"] = ("365", "Días de validez de documentos"),
        ["kyc.require_liveness_check"] = ("true", "Requerir prueba de vida"),
        ["kyc.auto_approve_high_confidence"] = ("false", "Auto-aprobar alta confianza"),
        ["kyc.high_confidence_threshold"] = ("95", "Umbral de auto-aprobación (0-100)"),
        ["kyc.face_match_threshold"] = ("80", "Umbral de coincidencia facial (0-100)"),

        // Media
        ["media.storage_provider"] = ("local", "Proveedor de almacenamiento (local/S3)"),
        ["media.max_upload_size_mb"] = ("100", "Tamaño máximo de carga (MB)"),
        ["media.allowed_content_types"] = ("jpg,jpeg,png,gif,webp,mp4,pdf", "Tipos de archivo permitidos"),
        ["media.cdn_base_url"] = ("", "URL base del CDN"),

        // Cache
        ["cache.default_expiration_minutes"] = ("30", "Expiración de cache por defecto (minutos)"),
        ["cache.user_cache_minutes"] = ("15", "Cache de usuarios (minutos)"),
        ["cache.enable_distributed_cache"] = ("true", "Habilitar cache distribuido (Redis)"),

        // Rate Limiting (unificado bajo security)
        ["security.ratelimit_enabled"] = ("true", "Rate limiting habilitado"),
        ["security.ratelimit_requests_per_minute"] = ("100", "Límite global de requests por minuto por IP/usuario"),
        ["security.ratelimit_login_attempts_per_hour"] = ("10", "Límite HTTP de intentos de login por hora"),

        // Billing (Payment Providers - 6 pasarelas)
        ["billing.stripe_trial_days"] = ("14", "Días de prueba gratis para dealers"),
        // Azul (Banco Popular RD)
        ["billing.azul_enabled"] = ("true", "Azul habilitado"),
        ["billing.azul_environment"] = ("Test", "Ambiente de Azul (Test/Prod)"),
        ["billing.azul_merchant_name"] = ("OKLA Marketplace", "Nombre de comercio en Azul"),
        ["billing.azul_merchant_type"] = ("eCommerce", "Tipo de comercio Azul"),
        ["billing.azul_currency_code"] = ("$", "Moneda para transacciones Azul"),
        // CardNET (Bancaria RD)
        ["billing.cardnet_enabled"] = ("false", "CardNET habilitado"),
        ["billing.cardnet_environment"] = ("Test", "Ambiente de CardNET (Test/Prod)"),
        // PixelPay (Fintech - comisiones más bajas)
        ["billing.pixelpay_enabled"] = ("true", "PixelPay habilitado"),
        ["billing.pixelpay_environment"] = ("Test", "Ambiente de PixelPay (Test/Prod)"),
        // Fygaro (Agregador)
        ["billing.fygaro_enabled"] = ("false", "Fygaro habilitado"),
        ["billing.fygaro_environment"] = ("Test", "Ambiente de Fygaro (Test/Prod)"),
        ["billing.fygaro_enable_subscriptions"] = ("true", "Habilitar módulo de suscripciones Fygaro"),
        // Stripe (Internacional + Suscripciones)
        ["billing.stripe_enabled"] = ("true", "Stripe habilitado"),
        ["billing.stripe_environment"] = ("Test", "Ambiente de Stripe (Test/Prod)"),
        // PayPal (Internacional)
        ["billing.paypal_enabled"] = ("false", "PayPal habilitado"),
        ["billing.paypal_environment"] = ("sandbox", "Ambiente de PayPal (sandbox/live)"),
        // General billing
        ["billing.invoice_prefix"] = ("OKLA-INV", "Prefijo para facturas"),
        ["billing.invoice_ncf_enabled"] = ("false", "NCF habilitado (DGII)"),
        ["billing.auto_retry_failed_payments"] = ("true", "Reintentar pagos fallidos automáticamente"),
        ["billing.max_payment_retries"] = ("3", "Máximo reintentos de pago"),
        ["billing.payment_retry_interval_hours"] = ("24", "Horas entre reintentos de pago"),

        // Pricing - Publicación individual (antes en billing)
        ["pricing.individual_listing_price"] = ("29", "Precio por publicación individual"),
    };

    var items = defaults.Select(kvp => new ConfigurationService.Domain.Entities.ConfigurationItem
    {
        Id = Guid.NewGuid(),
        Key = kvp.Key,
        Value = kvp.Value.Value,
        Description = kvp.Value.Description,
        Environment = env,
        CreatedAt = now,
        CreatedBy = createdBy,
        IsActive = true,
        Version = 1
    }).ToList();

    db.ConfigurationItems.AddRange(items);

    // Seed default feature flags
    // NOTE: Flags for 2FA, KYC Auto-Approve, Stripe/Azul Payments, Push/SMS Notifications
    // are NOT included here — they are already managed as toggles in their respective
    // configuration category cards (Security, KYC, Billing, Push, SMS).
    var featureFlags = new[]
    {
        new ConfigurationService.Domain.Entities.FeatureFlag { Id = Guid.NewGuid(), Name = "Elasticsearch", Key = "feature.enable_elasticsearch", IsEnabled = false, Description = "Habilitar búsqueda con Elasticsearch", Environment = env, CreatedAt = now, CreatedBy = createdBy, RolloutPercentage = 100 },
        new ConfigurationService.Domain.Entities.FeatureFlag { Id = Guid.NewGuid(), Name = "AI Processing", Key = "feature.enable_ai_processing", IsEnabled = false, Description = "Procesamiento de imágenes con IA", Environment = env, CreatedAt = now, CreatedBy = createdBy, RolloutPercentage = 100 },
        new ConfigurationService.Domain.Entities.FeatureFlag { Id = Guid.NewGuid(), Name = "Vehicle 360°", Key = "feature.enable_vehicle_360", IsEnabled = false, Description = "Vista 360° de vehículos", Environment = env, CreatedAt = now, CreatedBy = createdBy, RolloutPercentage = 100 },
        new ConfigurationService.Domain.Entities.FeatureFlag { Id = Guid.NewGuid(), Name = "Chatbot", Key = "feature.enable_chatbot", IsEnabled = false, Description = "Chatbot IA", Environment = env, CreatedAt = now, CreatedBy = createdBy, RolloutPercentage = 100 },
        new ConfigurationService.Domain.Entities.FeatureFlag { Id = Guid.NewGuid(), Name = "Dealer Analytics", Key = "feature.enable_dealer_analytics", IsEnabled = true, Description = "Dashboard de analytics para dealers", Environment = env, CreatedAt = now, CreatedBy = createdBy, RolloutPercentage = 100 },
        new ConfigurationService.Domain.Entities.FeatureFlag { Id = Guid.NewGuid(), Name = "Vehicle Comparison", Key = "feature.enable_comparison", IsEnabled = true, Description = "Comparador de vehículos", Environment = env, CreatedAt = now, CreatedBy = createdBy, RolloutPercentage = 100 },
        new ConfigurationService.Domain.Entities.FeatureFlag { Id = Guid.NewGuid(), Name = "Price Alerts", Key = "feature.enable_price_alerts", IsEnabled = true, Description = "Alertas de precio", Environment = env, CreatedAt = now, CreatedBy = createdBy, RolloutPercentage = 100 },
        new ConfigurationService.Domain.Entities.FeatureFlag { Id = Guid.NewGuid(), Name = "reCAPTCHA", Key = "feature.enable_recaptcha", IsEnabled = true, Description = "Verificación reCAPTCHA", Environment = env, CreatedAt = now, CreatedBy = createdBy, RolloutPercentage = 100 },
    };

    db.FeatureFlags.AddRange(featureFlags);
    await db.SaveChangesAsync();
}

// Seed encrypted secrets with actual development values from Docker/appsettings.
// These are the same values from appsettings.Development.json / compose.yaml.
// In production these would be replaced via the admin UI or env vars.
static async Task SeedDefaultSecrets(ISecretManager secretManager)
{
    var env = "Development";
    var createdBy = "system-seed";

    // Only seed if no secrets exist yet
    var existing = await secretManager.GetAllSecretsAsync(env);
    if (existing.Any())
        return;

    var secrets = new Dictionary<string, (string Value, string Description)>
    {
        // Email — Resend (primary provider from appsettings.Docker.json)
        ["email.resend_api_key"] = ("re_Bi3rubbH_LTnrn4UDrKQqUsLiajeJimvi", "Resend API Key (dev)"),

        // Email — SendGrid (from appsettings.Development.json)
        ["email.sendgrid_api_key"] = ("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", "SendGrid API Key (dev)"),

        // Email — SMTP password (uses SendGrid apikey auth)
        ["email.smtp_password"] = ("", "SMTP Password"),

        // SMS — Twilio (from appsettings.Development.json)
        ["sms.twilio_account_sid"] = ("AC19fec9dd3df70a34f6252c9ef649a532", "Twilio Account SID (dev)"),
        ["sms.twilio_auth_token"] = ("2221beebc69b7251062f2b10d7ed75e6", "Twilio Auth Token (dev)"),
        ["sms.twilio_messaging_service_sid"] = ("", "Twilio Messaging Service SID"),

        // Push — Firebase (from appsettings.Development.json)
        ["push.firebase_server_key"] = ("", "Firebase Server Key (legacy)"),

        // WhatsApp — Meta
        ["whatsapp.meta_access_token"] = ("", "Meta WhatsApp Access Token"),

        // Notifications — Webhooks
        ["notifications.teams_webhook_url"] = ("", "Microsoft Teams Webhook URL"),
        ["notifications.slack_webhook_url"] = ("", "Slack Webhook URL"),
    };

    foreach (var (key, (value, description)) in secrets)
    {
        // Skip empty values — they'll show as "Sin configurar" in the UI
        // (the UI checks if the key exists in the secrets response)
        if (string.IsNullOrWhiteSpace(value))
            continue;

        await secretManager.CreateSecretAsync(key, value, env, createdBy, description);
    }
}

// Make the implicit Program class public for integration tests
public partial class Program { }

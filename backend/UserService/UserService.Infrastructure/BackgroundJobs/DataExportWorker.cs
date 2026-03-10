using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.Domain.Entities.Privacy;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.BackgroundJobs;

// ═══════════════════════════════════════════════════════════════════════════════
// DATA EXPORT WORKER — LEY 172-13 ART. 5 DERECHO DE ACCESO Y PORTABILIDAD
//
// Processes pending data export requests asynchronously.
// SLA: Generate ZIP file within 10 minutes.
// Download link: Valid for 24 hours (secure token).
//
// Flow:
//   1. Poll for PrivacyRequest with Status=Pending, Type=Portability/Access
//   2. Aggregate user data from local DB (profile, preferences)
//   3. Serialize to JSON with readable formatting
//   4. Package into ZIP file
//   5. Generate secure download token (cryptographic)
//   6. Update PrivacyRequest: Status=Completed, FilePath, DownloadToken, ExpiresAt
//   7. (Future) Notify user via NotificationService
//
// Also handles expired export cleanup (removes files + marks expired).
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class DataExportWorker : BackgroundService
{
    private readonly ILogger<DataExportWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _pollInterval;
    private readonly string _exportBasePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public DataExportWorker(
        ILogger<DataExportWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _pollInterval = TimeSpan.FromSeconds(
            _configuration.GetValue<int>("Privacy:ExportPollIntervalSeconds", 30));
        _exportBasePath = _configuration["Privacy:ExportBasePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "exports");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[DataExportWorker] Started — Polling every {Interval}s, exports at {Path}",
            _pollInterval.TotalSeconds, _exportBasePath);

        // Ensure export directory exists
        Directory.CreateDirectory(_exportBasePath);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingExportsAsync(stoppingToken);
                await CleanupExpiredExportsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "[DataExportWorker] Error in processing cycle");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingExportsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var privacyRepo = scope.ServiceProvider.GetRequiredService<IPrivacyRequestRepository>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var commPrefRepo = scope.ServiceProvider.GetRequiredService<ICommunicationPreferenceRepository>();

        var pendingRequests = await privacyRepo.GetPendingExportRequestsAsync();

        foreach (var request in pendingRequests)
        {
            try
            {
                _logger.LogInformation(
                    "[DataExportWorker] Processing export: RequestId={Id}, UserId={UserId}",
                    request.Id, request.UserId);

                // Mark as processing
                request.Status = PrivacyRequestStatus.Processing;
                request.ProcessedAt = DateTime.UtcNow;
                await privacyRepo.UpdateAsync(request);

                // Aggregate user data
                var exportData = await AggregateUserDataAsync(request, userRepo, commPrefRepo, ct);

                // Generate ZIP file
                var (filePath, fileSize) = await GenerateExportZipAsync(request, exportData, ct);

                // Generate secure download token (24h expiry)
                var downloadToken = GenerateSecureToken();

                // Update request with completion info
                request.Status = PrivacyRequestStatus.Completed;
                request.CompletedAt = DateTime.UtcNow;
                request.FilePath = filePath;
                request.FileSizeBytes = fileSize;
                request.DownloadToken = downloadToken;
                request.DownloadTokenExpiresAt = DateTime.UtcNow.AddHours(24);
                await privacyRepo.UpdateAsync(request);

                var elapsed = (request.CompletedAt.Value - request.ProcessedAt.Value).TotalSeconds;
                _logger.LogInformation(
                    "[DataExportWorker] Export completed: RequestId={Id}, Size={Size}B, Token expires {Expiry}, Generated in {Elapsed:F1}s",
                    request.Id, fileSize, request.DownloadTokenExpiresAt, elapsed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[DataExportWorker] Failed to process export: RequestId={Id}, UserId={UserId}",
                    request.Id, request.UserId);

                request.Status = PrivacyRequestStatus.Pending; // Reset for retry
                request.AdminNotes = $"Export failed at {DateTime.UtcNow:u}: {ex.Message}";
                await privacyRepo.UpdateAsync(request);
            }
        }
    }

    private async Task<Dictionary<string, object>> AggregateUserDataAsync(
        PrivacyRequest request, IUserRepository userRepo, ICommunicationPreferenceRepository commPrefRepo, CancellationToken ct)
    {
        var data = new Dictionary<string, object>();

        // ── Metadata section (always included) ──
        data["_metadata"] = new
        {
            exportDate = DateTime.UtcNow.ToString("o"),
            requestId = request.Id,
            format = request.ExportFormat?.ToString() ?? "JSON",
            ley = "Ley 172-13 — Derecho de Acceso y Portabilidad (Art. 5)",
            description = "Exportación completa de datos personales del usuario conforme a la Ley 172-13 de Protección de Datos Personales de la República Dominicana.",
            sections = request.Description,
        };

        // ── User profile ──
        var user = await userRepo.GetByIdAsync(request.UserId);
        if (user != null)
        {
            data["perfil"] = new
            {
                userId = user.Id,
                nombre = user.FirstName,
                apellido = user.LastName,
                email = user.Email,
                telefono = user.PhoneNumber,
                ciudad = user.City,
                provincia = user.Province,
                verificado = user.EmailConfirmed,
                creadoEn = user.CreatedAt.ToString("o"),
                ultimoAcceso = user.LastLoginAt?.ToString("o"),
                contactoPreferido = user.PreferredContactMethod,
            };
        }

        // ── Communication preferences ──
        var preferences = await commPrefRepo.GetByUserIdAsync(request.UserId);
        if (preferences != null)
        {
            data["preferencias_comunicacion"] = new
            {
                emailActividad = preferences.EmailActivityNotifications,
                emailAnuncios = preferences.EmailListingUpdates,
                emailNewsletter = preferences.EmailNewsletter,
                emailPromociones = preferences.EmailPromotions,
                emailAlertasPrecios = preferences.EmailPriceAlerts,
                smsAlertasPrecios = preferences.SmsPriceAlerts,
                smsPromociones = preferences.SmsPromotions,
                pushMensajes = preferences.PushNewMessages,
                pushCambiosPrecios = preferences.PushPriceChanges,
                pushRecomendaciones = preferences.PushRecommendations,
                permitirPerfil = preferences.AllowProfiling,
                permitirTerceros = preferences.AllowThirdPartySharing,
                permitirAnalytics = preferences.AllowAnalytics,
                actualizadoEn = preferences.UpdatedAt.ToString("o"),
            };
        }

        // ── Privacy request history ──
        var privacyHistory = await GetPrivacyRequestHistoryAsync(request.UserId);
        if (privacyHistory.Any())
        {
            data["historial_solicitudes_privacidad"] = privacyHistory.Select(pr => new
            {
                id = pr.Id,
                tipo = pr.Type.ToString(),
                estado = pr.Status.ToString(),
                creadoEn = pr.CreatedAt.ToString("o"),
                completadoEn = pr.CompletedAt?.ToString("o"),
            });
        }

        // ── Cross-service data placeholder ──
        // In a full implementation, the worker would call Gateway HTTP endpoints to collect:
        // - ChatbotService: /api/chat/sessions (conversation history)
        // - NotificationService: /api/saved-searches, /api/price-alerts
        // - ContactService: /api/contact-requests (by buyer)
        // - AuthService: /api/sessions (login history)
        //
        // These are aggregated here as a section indicating data availability.
        // Each service should expose a /api/privacy/export/{userId} endpoint.
        data["datos_servicios_externos"] = new
        {
            nota = "Los datos de los siguientes servicios están disponibles bajo solicitud individual:",
            servicios = new[]
            {
                new { servicio = "ChatbotService", descripcion = "Historial de conversaciones con el agente de chat", endpoint = "/api/chat/history" },
                new { servicio = "NotificationService", descripcion = "Búsquedas guardadas y alertas de precio", endpoint = "/api/notifications/my-data" },
                new { servicio = "ContactService", descripcion = "Solicitudes de contacto enviadas", endpoint = "/api/contacts/my-requests" },
                new { servicio = "AuthService", descripcion = "Historial de sesiones y dispositivos", endpoint = "/api/auth/my-sessions" },
            },
        };

        return data;
    }

    private async Task<IEnumerable<PrivacyRequest>> GetPrivacyRequestHistoryAsync(Guid userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var privacyRepo = scope.ServiceProvider.GetRequiredService<IPrivacyRequestRepository>();
        return await privacyRepo.GetByUserIdAsync(userId);
    }

    private async Task<(string filePath, long fileSize)> GenerateExportZipAsync(
        PrivacyRequest request, Dictionary<string, object> data, CancellationToken ct)
    {
        var userId = request.UserId.ToString("N")[..8]; // Short ID for filename
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"okla_datos_{userId}_{timestamp}.zip";
        var filePath = Path.Combine(_exportBasePath, fileName);

        using (var zipStream = new FileStream(filePath, FileMode.Create))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: false))
        {
            // Main data file
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(data, JsonOptions);
            var entry = archive.CreateEntry("datos_personales.json", CompressionLevel.Optimal);
            using (var entryStream = entry.Open())
            {
                await entryStream.WriteAsync(jsonBytes, ct);
            }

            // README with legal context
            var readme = GenerateReadme(request);
            var readmeEntry = archive.CreateEntry("LEAME.txt", CompressionLevel.Optimal);
            using (var readmeStream = readmeEntry.Open())
            {
                var readmeBytes = Encoding.UTF8.GetBytes(readme);
                await readmeStream.WriteAsync(readmeBytes, ct);
            }
        }

        var fileInfo = new FileInfo(filePath);
        return (filePath, fileInfo.Length);
    }

    private static string GenerateReadme(PrivacyRequest request)
    {
        return $"""
        ════════════════════════════════════════════════════════════════
        EXPORTACIÓN DE DATOS PERSONALES — OKLA (okla.com.do)
        Ley 172-13 sobre Protección Integral de los Datos Personales
        ════════════════════════════════════════════════════════════════

        Fecha de exportación:  {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
        ID de solicitud:       {request.Id}
        Formato:               {request.ExportFormat?.ToString() ?? "JSON"}

        ────────────────────────────────────────────────────────────────
        CONTENIDO DEL ARCHIVO
        ────────────────────────────────────────────────────────────────

        • datos_personales.json  — Tus datos personales en formato JSON
        • LEAME.txt              — Este archivo con información legal

        ────────────────────────────────────────────────────────────────
        DERECHOS ARCO (Ley 172-13)
        ────────────────────────────────────────────────────────────────

        Tienes derecho a:
          A — Acceso:        Consultar tus datos (este archivo)
          R — Rectificación: Corregir datos incorrectos
          C — Cancelación:   Solicitar eliminación de tus datos
          O — Oposición:     Oponerte al procesamiento de tus datos

        Para ejercer cualquier derecho, visita:
          https://okla.com.do/perfil/privacidad

        ────────────────────────────────────────────────────────────────
        CONTACTO
        ────────────────────────────────────────────────────────────────

        Responsable de tratamiento: OKLA SRL
        Email: privacidad@okla.com.do
        Dirección: Santo Domingo, República Dominicana

        Este archivo es confidencial y fue generado exclusivamente para
        el titular de los datos. Enlace de descarga válido por 24 horas.
        ════════════════════════════════════════════════════════════════
        """;
    }

    private static string GenerateSecureToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private async Task CleanupExpiredExportsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var privacyRepo = scope.ServiceProvider.GetRequiredService<IPrivacyRequestRepository>();

        var expiredRequests = await privacyRepo.GetExpiredExportRequestsAsync();

        foreach (var request in expiredRequests)
        {
            try
            {
                // Delete the file from disk
                if (!string.IsNullOrEmpty(request.FilePath) && File.Exists(request.FilePath))
                {
                    File.Delete(request.FilePath);
                    _logger.LogInformation(
                        "[DataExportWorker] Deleted expired export file: {Path}", request.FilePath);
                }

                // Mark as expired
                request.Status = PrivacyRequestStatus.Expired;
                request.DownloadToken = null;
                request.FilePath = null;
                await privacyRepo.UpdateAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[DataExportWorker] Failed to cleanup expired export: RequestId={Id}", request.Id);
            }
        }
    }
}

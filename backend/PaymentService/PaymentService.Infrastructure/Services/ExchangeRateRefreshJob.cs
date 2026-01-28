using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Services.Settings;

namespace PaymentService.Infrastructure.Services;

/// <summary>
/// Background service que actualiza las tasas de cambio diariamente desde el BCRD
/// Se ejecuta a las 8:30 AM hora local (después de que el BCRD publica las tasas)
/// </summary>
public class ExchangeRateRefreshJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly BancoCentralSettings _settings;
    private readonly ILogger<ExchangeRateRefreshJob> _logger;
    
    // Zona horaria de RD (AST - Atlantic Standard Time, UTC-4)
    private static readonly TimeZoneInfo _rdTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");

    public ExchangeRateRefreshJob(
        IServiceProvider serviceProvider,
        IOptions<BancoCentralSettings> settings,
        ILogger<ExchangeRateRefreshJob> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ExchangeRateRefreshJob iniciado. Programado para ejecutar diariamente a las {Hour}:{Minute} (hora RD)",
            _settings.RefreshHour, _settings.RefreshMinute.ToString("D2"));

        // Ejecutar inmediatamente al iniciar el servicio
        await RefreshRatesAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = CalculateDelayUntilNextRun();
                _logger.LogDebug("Próxima actualización de tasas en {Delay}", delay);
                
                await Task.Delay(delay, stoppingToken);
                await RefreshRatesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ExchangeRateRefreshJob cancelado");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ExchangeRateRefreshJob, reintentando en 1 hora");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task RefreshRatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando actualización de tasas de cambio desde BCRD...");

            using var scope = _serviceProvider.CreateScope();
            var exchangeRateService = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();

            var success = await exchangeRateService.RefreshRatesFromBcrdAsync(cancellationToken);

            if (success)
            {
                _logger.LogInformation("✅ Tasas de cambio actualizadas exitosamente");
            }
            else
            {
                _logger.LogWarning("⚠️ No se pudieron actualizar las tasas de cambio");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error actualizando tasas de cambio");
        }
    }

    private TimeSpan CalculateDelayUntilNextRun()
    {
        // Obtener hora actual en zona horaria de RD
        var nowUtc = DateTime.UtcNow;
        var nowRd = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _rdTimeZone);

        // Hora programada para hoy
        var scheduledTimeToday = new DateTime(
            nowRd.Year, nowRd.Month, nowRd.Day,
            _settings.RefreshHour, _settings.RefreshMinute, 0);

        // Si ya pasó la hora de hoy, programar para mañana
        if (nowRd >= scheduledTimeToday)
        {
            scheduledTimeToday = scheduledTimeToday.AddDays(1);
        }

        // Convertir de vuelta a UTC para calcular delay
        var scheduledUtc = TimeZoneInfo.ConvertTimeToUtc(scheduledTimeToday, _rdTimeZone);
        var delay = scheduledUtc - nowUtc;

        // Mínimo 1 minuto de delay
        if (delay < TimeSpan.FromMinutes(1))
        {
            delay = TimeSpan.FromMinutes(1);
        }

        return delay;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ExchangeRateRefreshJob detenido");
        await base.StopAsync(cancellationToken);
    }
}

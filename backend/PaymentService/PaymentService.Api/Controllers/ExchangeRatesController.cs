using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Api.Controllers;

/// <summary>
/// Controller para tasas de cambio del Banco Central RD
/// Fuente oficial requerida por DGII para transacciones en moneda extranjera
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;
    private readonly IExchangeRateRepository _rateRepository;
    private readonly ICurrencyConversionRepository _conversionRepository;
    private readonly ILogger<ExchangeRatesController> _logger;

    public ExchangeRatesController(
        IExchangeRateService exchangeRateService,
        IExchangeRateRepository rateRepository,
        ICurrencyConversionRepository conversionRepository,
        ILogger<ExchangeRatesController> logger)
    {
        _exchangeRateService = exchangeRateService;
        _rateRepository = rateRepository;
        _conversionRepository = conversionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene la tasa de cambio actual para una moneda
    /// </summary>
    /// <param name="currency">Código de moneda (USD, EUR)</param>
    /// <returns>Tasa de cambio actual del Banco Central</returns>
    [HttpGet("current/{currency}")]
    [ProducesResponseType(typeof(ExchangeRateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentRate(string currency, CancellationToken cancellationToken)
    {
        try
        {
            var rate = await _exchangeRateService.GetCurrentRateAsync(currency.ToUpperInvariant(), cancellationToken);

            return Ok(new ExchangeRateResponse
            {
                Currency = rate.SourceCurrency,
                Date = rate.RateDate.ToString("yyyy-MM-dd"),
                BuyRate = rate.BuyRate,
                SellRate = rate.SellRate,
                Source = rate.Source.ToString(),
                FetchedAt = rate.FetchedAt
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tasa no disponible para {Currency}", currency);
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene las tasas de cambio para todas las monedas soportadas
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(AllRatesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCurrentRates(CancellationToken cancellationToken)
    {
        var currencies = await _rateRepository.GetAvailableCurrenciesAsync(cancellationToken);
        var rates = new List<ExchangeRateResponse>();

        foreach (var currency in currencies)
        {
            try
            {
                var rate = await _exchangeRateService.GetCurrentRateAsync(currency, cancellationToken);
                rates.Add(new ExchangeRateResponse
                {
                    Currency = rate.SourceCurrency,
                    Date = rate.RateDate.ToString("yyyy-MM-dd"),
                    BuyRate = rate.BuyRate,
                    SellRate = rate.SellRate,
                    Source = rate.Source.ToString(),
                    FetchedAt = rate.FetchedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error obteniendo tasa para {Currency}", currency);
            }
        }

        return Ok(new AllRatesResponse
        {
            Rates = rates,
            BaseCurrency = "DOP",
            RetrievedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Obtiene el historial de tasas para una moneda en un rango de fechas
    /// </summary>
    [HttpGet("history/{currency}")]
    [ProducesResponseType(typeof(RateHistoryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRateHistory(
        string currency,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var start = startDate.HasValue 
            ? DateOnly.FromDateTime(startDate.Value) 
            : DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
        
        var end = endDate.HasValue 
            ? DateOnly.FromDateTime(endDate.Value) 
            : DateOnly.FromDateTime(DateTime.Now);

        var rates = await _rateRepository.GetRatesInRangeAsync(
            currency.ToUpperInvariant(), 
            start, 
            end, 
            cancellationToken);

        return Ok(new RateHistoryResponse
        {
            Currency = currency.ToUpperInvariant(),
            StartDate = start.ToString("yyyy-MM-dd"),
            EndDate = end.ToString("yyyy-MM-dd"),
            Rates = rates.Select(r => new ExchangeRateResponse
            {
                Currency = r.SourceCurrency,
                Date = r.RateDate.ToString("yyyy-MM-dd"),
                BuyRate = r.BuyRate,
                SellRate = r.SellRate,
                Source = r.Source.ToString(),
                FetchedAt = r.FetchedAt
            }).ToList()
        });
    }

    /// <summary>
    /// Convierte un monto de moneda extranjera a DOP
    /// Incluye cálculo de ITBIS (18%)
    /// </summary>
    [HttpPost("convert")]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConvertToDop(
        [FromBody] ConvertRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new { error = "El monto debe ser mayor a 0" });
        }

        var result = await _exchangeRateService.ConvertToDopAsync(
            request.Amount,
            request.Currency,
            request.TransactionId,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(new ConversionResponse
        {
            OriginalAmount = result.OriginalAmount,
            OriginalCurrency = result.OriginalCurrency,
            ConvertedAmountDop = result.ConvertedAmountDop,
            AppliedRate = result.AppliedRate,
            RateDate = result.RateDate.ToString("yyyy-MM-dd"),
            RateSource = result.RateSource,
            ItbisDop = result.ItbisDop,
            ItbisRate = 0.18m,
            TotalWithItbisDop = result.TotalWithItbisDop,
            ConversionRecordId = result.ConversionRecordId
        });
    }

    /// <summary>
    /// Obtiene una cotización sin registrar la conversión
    /// Útil para mostrar precios en UI
    /// </summary>
    [HttpGet("quote")]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuote(
        [FromQuery] decimal amount,
        [FromQuery] string currency,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            return BadRequest(new { error = "El monto debe ser mayor a 0" });
        }

        var result = await _exchangeRateService.GetQuoteAsync(amount, currency, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(new ConversionResponse
        {
            OriginalAmount = result.OriginalAmount,
            OriginalCurrency = result.OriginalCurrency,
            ConvertedAmountDop = result.ConvertedAmountDop,
            AppliedRate = result.AppliedRate,
            RateDate = result.RateDate.ToString("yyyy-MM-dd"),
            RateSource = result.RateSource,
            ItbisDop = result.ItbisDop,
            ItbisRate = 0.18m,
            TotalWithItbisDop = result.TotalWithItbisDop,
            IsQuoteOnly = true
        });
    }

    /// <summary>
    /// Fuerza actualización de tasas desde el BCRD (Admin only)
    /// </summary>
    [HttpPost("refresh")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshRates(CancellationToken cancellationToken)
    {
        var success = await _exchangeRateService.RefreshRatesFromBcrdAsync(cancellationToken);

        if (!success)
        {
            return StatusCode(500, new { error = "No se pudieron actualizar las tasas desde el BCRD" });
        }

        return Ok(new { message = "Tasas actualizadas exitosamente", refreshedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Obtiene el registro de conversión de una transacción (para auditoría DGII)
    /// </summary>
    [HttpGet("conversions/{transactionId}")]
    [Authorize]
    [ProducesResponseType(typeof(ConversionRecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversionByTransaction(Guid transactionId, CancellationToken cancellationToken)
    {
        var conversion = await _conversionRepository.GetByTransactionIdAsync(transactionId, cancellationToken);

        if (conversion == null)
        {
            return NotFound(new { error = "No se encontró registro de conversión para esta transacción" });
        }

        return Ok(new ConversionRecordResponse
        {
            Id = conversion.Id,
            TransactionId = conversion.PaymentTransactionId,
            OriginalAmount = conversion.OriginalAmount,
            OriginalCurrency = conversion.OriginalCurrency,
            ConvertedAmountDop = conversion.ConvertedAmountDop,
            AppliedRate = conversion.AppliedRate,
            RateDate = conversion.RateDate.ToString("yyyy-MM-dd"),
            RateSource = conversion.RateSource.ToString(),
            ItbisDop = conversion.ItbisDop,
            TotalWithItbisDop = conversion.TotalWithItbisDop,
            Ncf = conversion.Ncf,
            NcfIssuedAt = conversion.NcfIssuedAt,
            CreatedAt = conversion.CreatedAt
        });
    }

    /// <summary>
    /// Verifica estado del servicio de tasas
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> HealthCheck(CancellationToken cancellationToken)
    {
        var isHealthy = await _exchangeRateService.IsServiceHealthyAsync(cancellationToken);

        if (!isHealthy)
        {
            return StatusCode(503, new { status = "degraded", message = "Servicio de tasas no disponible completamente" });
        }

        return Ok(new { status = "healthy", checkedAt = DateTime.UtcNow });
    }
}

// ==================== DTOs ====================

public class ExchangeRateResponse
{
    public string Currency { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public decimal BuyRate { get; set; }
    public decimal SellRate { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime FetchedAt { get; set; }
}

public class AllRatesResponse
{
    public List<ExchangeRateResponse> Rates { get; set; } = new();
    public string BaseCurrency { get; set; } = "DOP";
    public DateTime RetrievedAt { get; set; }
}

public class RateHistoryResponse
{
    public string Currency { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public List<ExchangeRateResponse> Rates { get; set; } = new();
}

public class ConvertRequest
{
    /// <summary>
    /// Monto en moneda extranjera
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Código de moneda (USD, EUR)
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// ID de transacción para registro de auditoría (opcional)
    /// </summary>
    public Guid? TransactionId { get; set; }
}

public class ConversionResponse
{
    public decimal OriginalAmount { get; set; }
    public string OriginalCurrency { get; set; } = string.Empty;
    public decimal ConvertedAmountDop { get; set; }
    public decimal AppliedRate { get; set; }
    public string RateDate { get; set; } = string.Empty;
    public string RateSource { get; set; } = string.Empty;
    public decimal ItbisDop { get; set; }
    public decimal ItbisRate { get; set; }
    public decimal TotalWithItbisDop { get; set; }
    public Guid? ConversionRecordId { get; set; }
    public bool IsQuoteOnly { get; set; }
}

public class ConversionRecordResponse
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public decimal OriginalAmount { get; set; }
    public string OriginalCurrency { get; set; } = string.Empty;
    public decimal ConvertedAmountDop { get; set; }
    public decimal AppliedRate { get; set; }
    public string RateDate { get; set; } = string.Empty;
    public string RateSource { get; set; } = string.Empty;
    public decimal ItbisDop { get; set; }
    public decimal TotalWithItbisDop { get; set; }
    public string? Ncf { get; set; }
    public DateTime? NcfIssuedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

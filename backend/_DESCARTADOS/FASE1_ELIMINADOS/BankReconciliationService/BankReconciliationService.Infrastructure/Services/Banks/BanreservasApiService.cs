using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Enums;
using BankReconciliationService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace BankReconciliationService.Infrastructure.Services.Banks;

/// <summary>
/// Configuración específica para Banreservas API
/// </summary>
public class BanreservasApiSettings
{
    public const string SectionName = "BankApis:Banreservas";
    
    public string BaseUrl { get; set; } = "https://api.banreservas.com.do/v1";
    public string ApiKeyHeader { get; set; } = "X-API-Key";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Servicio de integración con Banreservas (Banco de Reservas) API
/// 
/// Banreservas es el banco más grande de República Dominicana.
/// Su API usa autenticación por API Key.
/// 
/// Características:
/// - Autenticación: API Key en header
/// - Costo: $30 USD/mes
/// - Límite: 50 requests/minuto
/// - Datos en tiempo real: Sí (actualización cada 15 min)
/// - Formatos: JSON
/// 
/// Contacto API: apibanking@banreservas.com
/// </summary>
public class BanreservasApiService : IBankApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BanreservasApiService> _logger;
    private readonly BanreservasApiSettings _settings;

    public BanreservasApiService(
        HttpClient httpClient, 
        ILogger<BanreservasApiService> logger,
        IOptions<BanreservasApiSettings>? settings = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings?.Value ?? new BanreservasApiSettings();
    }

    #region IBankApiService Implementation

    /// <summary>
    /// Prueba la conexión con la API de Banreservas
    /// </summary>
    public async Task<bool> TestConnectionAsync(BankAccountConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing connection to Banreservas API for account {AccountNumber}", 
                MaskAccountNumber(config.AccountNumber));
            
            SetupApiKey(config);
            
            var response = await _httpClient.GetAsync($"{GetBaseUrl(config)}/health", cancellationToken);
            var isConnected = response.IsSuccessStatusCode;
            
            _logger.LogInformation("Banreservas API connection test result: {Result}", 
                isConnected ? "SUCCESS" : "FAILED");
            
            return isConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection to Banreservas API");
            return false;
        }
    }

    /// <summary>
    /// Obtiene transacciones de Banreservas para el período especificado
    /// </summary>
    public async Task<List<BankStatementLine>> GetTransactionsAsync(
        BankAccountConfig config, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching transactions from Banreservas for {From:yyyy-MM-dd} to {To:yyyy-MM-dd}", 
            from, to);
        
        SetupApiKey(config);

        var url = $"{GetBaseUrl(config)}/accounts/{config.AccountNumber}/movements" +
                  $"?dateFrom={from:yyyy-MM-dd}&dateTo={to:yyyy-MM-dd}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Banreservas API error: {StatusCode} - {Content}", 
                response.StatusCode, errorContent);
            throw new HttpRequestException($"Banreservas API error: {response.StatusCode}");
        }

        var data = await response.Content.ReadFromJsonAsync<BanreservasMovementsResponse>(
            cancellationToken: cancellationToken);
        
        if (data?.Movements == null || data.Movements.Count == 0)
        {
            _logger.LogInformation("No transactions found for the specified period");
            return new List<BankStatementLine>();
        }

        var lines = MapMovementsToLines(data.Movements);
        
        _logger.LogInformation("Successfully imported {Count} transactions from Banreservas", lines.Count);
        return lines;
    }

    /// <summary>
    /// Obtiene el balance actual de la cuenta en Banreservas
    /// </summary>
    public async Task<decimal> GetCurrentBalanceAsync(BankAccountConfig config, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching current balance from Banreservas for account {Account}", 
            MaskAccountNumber(config.AccountNumber));
        
        SetupApiKey(config);

        var url = $"{GetBaseUrl(config)}/accounts/{config.AccountNumber}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<BanreservasAccountResponse>(
            cancellationToken: cancellationToken);
        
        var balance = data?.Balance ?? 0;
        _logger.LogDebug("Current balance: {Balance:N2}", balance);
        
        return balance;
    }

    /// <summary>
    /// Importa un estado de cuenta completo de Banreservas
    /// </summary>
    public async Task<BankStatement> ImportStatementAsync(
        BankAccountConfig config, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Importing full statement from Banreservas for {From:yyyy-MM-dd} to {To:yyyy-MM-dd}", 
            from, to);

        var closingBalance = await GetCurrentBalanceAsync(config, cancellationToken);
        var lines = await GetTransactionsAsync(config, from, to, cancellationToken);

        // Calcular balance de apertura basado en transacciones
        var openingBalance = closingBalance - lines.Sum(l => l.CreditAmount) + lines.Sum(l => l.DebitAmount);

        var statement = new BankStatement
        {
            Id = Guid.NewGuid(),
            BankCode = "BANRESERVAS",
            AccountNumber = config.AccountNumber,
            AccountName = config.AccountName,
            StatementDate = DateTime.UtcNow,
            PeriodFrom = from,
            PeriodTo = to,
            OpeningBalance = openingBalance,
            ClosingBalance = closingBalance,
            TotalDebits = lines.Sum(l => l.DebitAmount),
            TotalCredits = lines.Sum(l => l.CreditAmount),
            Status = ReconciliationStatus.Pending,
            ImportedAt = DateTime.UtcNow,
            ImportSource = "API-BANRESERVAS",
            Lines = lines
        };

        foreach (var line in lines)
        {
            line.BankStatementId = statement.Id;
        }

        _logger.LogInformation("Successfully imported statement with {LineCount} transactions, " +
            "Opening: {Opening:N2}, Closing: {Closing:N2}", 
            lines.Count, openingBalance, closingBalance);

        return statement;
    }

    #endregion

    #region Helper Methods

    private void SetupApiKey(BankAccountConfig config)
    {
        _httpClient.DefaultRequestHeaders.Remove(_settings.ApiKeyHeader);
        _httpClient.DefaultRequestHeaders.Add(_settings.ApiKeyHeader, 
            config.ApiClientSecretEncrypted ?? throw new InvalidOperationException("API Key not configured"));
    }

    private string GetBaseUrl(BankAccountConfig config)
    {
        return !string.IsNullOrEmpty(config.ApiBaseUrl) ? config.ApiBaseUrl : _settings.BaseUrl;
    }

    private List<BankStatementLine> MapMovementsToLines(List<BanreservasMovement> movements)
    {
        var lines = new List<BankStatementLine>();
        int lineNumber = 1;

        foreach (var mov in movements.OrderBy(m => m.Date))
        {
            lines.Add(new BankStatementLine
            {
                Id = Guid.NewGuid(),
                LineNumber = lineNumber++,
                TransactionDate = mov.Date,
                ValueDate = mov.ValueDate,
                ReferenceNumber = mov.TrackingNumber ?? string.Empty,
                Description = mov.Concept ?? string.Empty,
                Type = mov.Amount >= 0 ? TransactionType.Deposit : TransactionType.Withdrawal,
                DebitAmount = mov.Amount < 0 ? Math.Abs(mov.Amount) : 0,
                CreditAmount = mov.Amount >= 0 ? mov.Amount : 0,
                Balance = mov.Balance,
                BankCategory = mov.Category,
                Beneficiary = mov.Beneficiary
            });
        }

        return lines;
    }

    private static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 4)
            return "****";
        return $"****{accountNumber[^4..]}";
    }

    #endregion

    #region Response DTOs (Private)

    private record BanreservasMovementsResponse(
        List<BanreservasMovement>? Movements,
        BanreservasPagination? Pagination,
        string? Message
    );

    private record BanreservasMovement(
        DateTime Date,
        DateTime? ValueDate,
        string? TrackingNumber,
        string? Concept,
        decimal Amount,
        decimal Balance,
        string? Category,
        string? Beneficiary,
        string? CheckNumber,
        string? TransactionCode
    );

    private record BanreservasPagination(
        int CurrentPage,
        int TotalPages,
        int RecordsPerPage,
        int TotalRecords
    );

    private record BanreservasAccountResponse(
        string? AccountNumber,
        string? AccountType,
        string? AccountName,
        decimal Balance,
        decimal AvailableBalance,
        string? Currency,
        string? Status,
        DateTime LastUpdated
    );

    #endregion
}

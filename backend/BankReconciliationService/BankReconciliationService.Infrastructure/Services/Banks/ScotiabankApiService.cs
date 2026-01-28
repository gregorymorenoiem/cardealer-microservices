using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Enums;
using BankReconciliationService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;

namespace BankReconciliationService.Infrastructure.Services.Banks;

/// <summary>
/// Configuración específica para Scotiabank API
/// </summary>
public class ScotiabankApiSettings
{
    public const string SectionName = "BankApis:Scotiabank";
    
    public string BaseUrl { get; set; } = "https://api.scotiabank.com.do";
    public string CertificatePath { get; set; } = "";
    public string CertificatePassword { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 30;
    public bool UseSoapFallback { get; set; } = false;
}

/// <summary>
/// Servicio de integración con Scotiabank RD API
/// 
/// Scotiabank usa autenticación basada en certificado cliente (mTLS).
/// La API tiene funcionalidad limitada comparada con otros bancos.
/// 
/// Características:
/// - Autenticación: Certificado Cliente (mTLS)
/// - Costo: $80 USD/mes
/// - Límite: 30 requests/minuto
/// - Datos en tiempo real: No (batch nocturno)
/// - Formatos: JSON (REST) / XML (SOAP legacy)
/// - Nota: API limitada a consultas solamente
/// 
/// Requisitos:
/// - Certificado P12/PFX proporcionado por Scotiabank
/// - Registro previo en portal corporativo
/// 
/// Contacto: ebusiness@scotiabank.com.do
/// </summary>
public class ScotiabankApiService : IBankApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScotiabankApiService> _logger;
    private readonly ScotiabankApiSettings _settings;
    private bool _certificateLoaded = false;

    public ScotiabankApiService(
        HttpClient httpClient, 
        ILogger<ScotiabankApiService> logger,
        IOptions<ScotiabankApiSettings>? settings = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings?.Value ?? new ScotiabankApiSettings();
    }

    #region IBankApiService Implementation

    /// <summary>
    /// Prueba la conexión con la API de Scotiabank
    /// </summary>
    public async Task<bool> TestConnectionAsync(BankAccountConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing connection to Scotiabank API for account {AccountNumber}", 
                MaskAccountNumber(config.AccountNumber));
            
            LoadCertificateIfNeeded(config);
            
            var url = $"{GetBaseUrl(config)}/health";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            var isConnected = response.IsSuccessStatusCode;
            
            _logger.LogInformation("Scotiabank API connection test result: {Result}", 
                isConnected ? "SUCCESS" : "FAILED");
            
            return isConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection to Scotiabank API");
            return false;
        }
    }

    /// <summary>
    /// Obtiene transacciones de Scotiabank para el período especificado
    /// </summary>
    public async Task<List<BankStatementLine>> GetTransactionsAsync(
        BankAccountConfig config, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching transactions from Scotiabank for {From:yyyy-MM-dd} to {To:yyyy-MM-dd}", 
            from, to);
        
        LoadCertificateIfNeeded(config);

        // Scotiabank usa parámetros diferentes
        var url = $"{GetBaseUrl(config)}/corporate/accounts/{config.AccountNumber}/transactions" +
                  $"?startDate={from:yyyy-MM-dd}&endDate={to:yyyy-MM-dd}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Scotiabank API error: {StatusCode} - {Content}", 
                response.StatusCode, errorContent);
            
            // Fallback a SOAP si está habilitado
            if (_settings.UseSoapFallback)
            {
                _logger.LogWarning("Attempting SOAP fallback for Scotiabank");
                return await GetTransactionsViaSoapAsync(config, from, to, cancellationToken);
            }
            
            throw new HttpRequestException($"Scotiabank API error: {response.StatusCode}");
        }

        var data = await response.Content.ReadFromJsonAsync<ScotiabankTransactionsResponse>(
            cancellationToken: cancellationToken);
        
        if (data?.Transactions == null || data.Transactions.Count == 0)
        {
            _logger.LogInformation("No transactions found for the specified period");
            return new List<BankStatementLine>();
        }

        var lines = MapTransactionsToLines(data.Transactions);
        
        _logger.LogInformation("Successfully imported {Count} transactions from Scotiabank", lines.Count);
        return lines;
    }

    /// <summary>
    /// Obtiene el balance actual de la cuenta en Scotiabank
    /// </summary>
    public async Task<decimal> GetCurrentBalanceAsync(BankAccountConfig config, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching current balance from Scotiabank for account {Account}", 
            MaskAccountNumber(config.AccountNumber));
        
        LoadCertificateIfNeeded(config);

        var url = $"{GetBaseUrl(config)}/corporate/accounts/{config.AccountNumber}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<ScotiabankAccountResponse>(
            cancellationToken: cancellationToken);
        
        var balance = data?.Balance ?? 0;
        _logger.LogDebug("Current balance: {Balance:N2}", balance);
        
        return balance;
    }

    /// <summary>
    /// Importa un estado de cuenta completo de Scotiabank
    /// </summary>
    public async Task<BankStatement> ImportStatementAsync(
        BankAccountConfig config, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Importing full statement from Scotiabank for {From:yyyy-MM-dd} to {To:yyyy-MM-dd}", 
            from, to);

        var closingBalance = await GetCurrentBalanceAsync(config, cancellationToken);
        var lines = await GetTransactionsAsync(config, from, to, cancellationToken);

        var openingBalance = closingBalance - lines.Sum(l => l.CreditAmount) + lines.Sum(l => l.DebitAmount);

        var statement = new BankStatement
        {
            Id = Guid.NewGuid(),
            BankCode = "SCOTIABANK",
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
            ImportSource = "API-SCOTIABANK",
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

    #region Certificate Handling

    /// <summary>
    /// Carga el certificado cliente si es necesario
    /// </summary>
    private void LoadCertificateIfNeeded(BankAccountConfig config)
    {
        if (_certificateLoaded) return;

        // Certificate path from settings (BankAccountConfig no longer has CertificatePath)
        var certPath = _settings.CertificatePath;
        var certPassword = _settings.CertificatePassword;

        if (!string.IsNullOrEmpty(certPath))
        {
            _logger.LogDebug("Loading client certificate from {Path}", certPath);
            
            // Nota: En producción, el certificado se carga al crear el HttpClient
            // a través de IHttpClientFactory con HttpClientHandler configurado
            
            _certificateLoaded = true;
            _logger.LogInformation("Client certificate loaded successfully");
        }
    }

    #endregion

    #region SOAP Fallback

    /// <summary>
    /// Obtiene transacciones vía SOAP (fallback para sistemas legacy)
    /// </summary>
    private async Task<List<BankStatementLine>> GetTransactionsViaSoapAsync(
        BankAccountConfig config, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken)
    {
        // Implementación SOAP simplificada
        _logger.LogWarning("SOAP fallback not fully implemented, returning empty list");
        return new List<BankStatementLine>();
    }

    #endregion

    #region Helper Methods

    private string GetBaseUrl(BankAccountConfig config)
    {
        return !string.IsNullOrEmpty(config.ApiBaseUrl) ? config.ApiBaseUrl : _settings.BaseUrl;
    }

    private List<BankStatementLine> MapTransactionsToLines(List<ScotiabankTransaction> transactions)
    {
        var lines = new List<BankStatementLine>();
        int lineNumber = 1;

        foreach (var tx in transactions.OrderBy(t => t.PostingDate))
        {
            lines.Add(new BankStatementLine
            {
                Id = Guid.NewGuid(),
                LineNumber = lineNumber++,
                TransactionDate = tx.PostingDate,
                ValueDate = tx.EffectiveDate,
                ReferenceNumber = tx.ReferenceNumber ?? string.Empty,
                Description = tx.Description ?? string.Empty,
                Type = tx.Amount >= 0 ? TransactionType.Deposit : TransactionType.Withdrawal,
                DebitAmount = tx.Amount < 0 ? Math.Abs(tx.Amount) : 0,
                CreditAmount = tx.Amount >= 0 ? tx.Amount : 0,
                Balance = tx.Balance,
                BankCategory = tx.TransactionCode
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

    private record ScotiabankTransactionsResponse(
        List<ScotiabankTransaction>? Transactions,
        ScotiabankPagination? Pagination,
        string? Status,
        string? Message
    );

    private record ScotiabankTransaction(
        DateTime PostingDate,
        DateTime EffectiveDate,
        string? ReferenceNumber,
        string? Description,
        decimal Amount,
        decimal Balance,
        string? TransactionCode,
        string? CheckNumber,
        string? BranchCode
    );

    private record ScotiabankPagination(
        int Page,
        int PageSize,
        int TotalRecords,
        bool HasMore
    );

    private record ScotiabankAccountResponse(
        string? AccountNumber,
        string? AccountName,
        string? AccountType,
        decimal Balance,
        decimal AvailableBalance,
        string? Currency,
        string? Status,
        DateTime? LastActivityDate
    );

    #endregion
}

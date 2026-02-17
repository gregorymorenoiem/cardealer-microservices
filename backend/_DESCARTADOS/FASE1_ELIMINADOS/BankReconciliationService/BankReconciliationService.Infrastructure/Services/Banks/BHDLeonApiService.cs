using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Enums;
using BankReconciliationService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BankReconciliationService.Infrastructure.Services.Banks;

/// <summary>
/// Configuración específica para BHD León API
/// </summary>
public class BHDLeonApiSettings
{
    public const string SectionName = "BankApis:BHDLeon";
    
    public string BaseUrl { get; set; } = "https://openbanking.bhd.com.do/api/v1";
    public string TokenEndpoint { get; set; } = "/oauth/token";
    public string Scope { get; set; } = "accounts transactions";
    public int TokenExpiryBufferSeconds { get; set; } = 300;
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Servicio de integración con BHD León API (Open Banking PSD2)
/// 
/// BHD León implementa el estándar Open Banking europeo (PSD2) adaptado para RD.
/// Usa OAuth 2.0 con flujo de autorización completo.
/// 
/// Características:
/// - Autenticación: OAuth 2.0 (Client Credentials / Authorization Code)
/// - Costo: $40 USD/mes
/// - Límite: 100 requests/minuto
/// - Datos en tiempo real: Sí
/// - Formatos: JSON
/// - Estándar: PSD2 / Open Banking
/// 
/// Portal Desarrolladores: https://developers.bhdleon.com.do/
/// Contacto: openbanking@bhdleon.com.do
/// </summary>
public class BHDLeonApiService : IBankApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BHDLeonApiService> _logger;
    private readonly BHDLeonApiSettings _settings;
    
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public BHDLeonApiService(
        HttpClient httpClient, 
        ILogger<BHDLeonApiService> logger,
        IOptions<BHDLeonApiSettings>? settings = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings?.Value ?? new BHDLeonApiSettings();
    }

    #region IBankApiService Implementation

    /// <summary>
    /// Prueba la conexión con la API de BHD León
    /// </summary>
    public async Task<bool> TestConnectionAsync(BankAccountConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing connection to BHD León Open Banking API for account {AccountNumber}", 
                MaskAccountNumber(config.AccountNumber));
            
            await AuthenticateAsync(config, cancellationToken);
            
            var isConnected = !string.IsNullOrEmpty(_accessToken);
            
            _logger.LogInformation("BHD León API connection test result: {Result}", 
                isConnected ? "SUCCESS" : "FAILED");
            
            return isConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection to BHD León API");
            return false;
        }
    }

    /// <summary>
    /// Obtiene transacciones de BHD León para el período especificado
    /// </summary>
    public async Task<List<BankStatementLine>> GetTransactionsAsync(
        BankAccountConfig config, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching transactions from BHD León for {From:yyyy-MM-dd} to {To:yyyy-MM-dd}", 
            from, to);
        
        await EnsureAuthenticatedAsync(config, cancellationToken);

        var url = $"{GetBaseUrl(config)}/accounts/{config.AccountNumber}/transactions" +
                  $"?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("BHD León API error: {StatusCode} - {Content}", 
                response.StatusCode, errorContent);
            throw new HttpRequestException($"BHD León API error: {response.StatusCode}");
        }

        var data = await response.Content.ReadFromJsonAsync<BHDLeonTransactionsResponse>(
            cancellationToken: cancellationToken);
        
        if (data?.Data?.Transactions == null || data.Data.Transactions.Count == 0)
        {
            _logger.LogInformation("No transactions found for the specified period");
            return new List<BankStatementLine>();
        }

        var lines = MapTransactionsToLines(data.Data.Transactions);
        
        _logger.LogInformation("Successfully imported {Count} transactions from BHD León", lines.Count);
        return lines;
    }

    /// <summary>
    /// Obtiene el balance actual de la cuenta en BHD León
    /// </summary>
    public async Task<decimal> GetCurrentBalanceAsync(BankAccountConfig config, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching current balance from BHD León for account {Account}", 
            MaskAccountNumber(config.AccountNumber));
        
        await EnsureAuthenticatedAsync(config, cancellationToken);

        var url = $"{GetBaseUrl(config)}/accounts/{config.AccountNumber}/balance";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<BHDLeonBalanceResponse>(
            cancellationToken: cancellationToken);
        
        var balance = data?.Data?.AvailableBalance ?? 0;
        _logger.LogDebug("Current balance: {Balance:N2}", balance);
        
        return balance;
    }

    /// <summary>
    /// Importa un estado de cuenta completo de BHD León
    /// </summary>
    public async Task<BankStatement> ImportStatementAsync(
        BankAccountConfig config, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Importing full statement from BHD León for {From:yyyy-MM-dd} to {To:yyyy-MM-dd}", 
            from, to);

        var closingBalance = await GetCurrentBalanceAsync(config, cancellationToken);
        var lines = await GetTransactionsAsync(config, from, to, cancellationToken);

        var openingBalance = closingBalance - lines.Sum(l => l.CreditAmount) + lines.Sum(l => l.DebitAmount);

        var statement = new BankStatement
        {
            Id = Guid.NewGuid(),
            BankCode = "BHD",
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
            ImportSource = "API-BHD",
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

    #region Authentication

    /// <summary>
    /// Autentica con BHD León usando OAuth 2.0
    /// </summary>
    private async Task AuthenticateAsync(BankAccountConfig config, CancellationToken cancellationToken)
    {
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            _logger.LogDebug("Authenticating with BHD León Open Banking API");

            var request = new HttpRequestMessage(HttpMethod.Post, 
                $"{GetBaseUrl(config)}{_settings.TokenEndpoint}");
            
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", config.ApiClientId ?? throw new InvalidOperationException("API Client ID not configured") },
                { "client_secret", config.ApiClientSecretEncrypted ?? throw new InvalidOperationException("API Client Secret not configured") },
                { "scope", _settings.Scope }
            });

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Authentication failed: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Authentication failed: {response.StatusCode}");
            }

            var data = await response.Content.ReadFromJsonAsync<BHDLeonTokenResponse>(
                cancellationToken: cancellationToken);
            
            _accessToken = data?.AccessToken ?? throw new InvalidOperationException("No access token received");
            _tokenExpiry = DateTime.UtcNow.AddSeconds((data?.ExpiresIn ?? 3600) - _settings.TokenExpiryBufferSeconds);
            
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _accessToken);

            _logger.LogInformation("Successfully authenticated with BHD León API, token expires at {Expiry:HH:mm:ss}", 
                _tokenExpiry);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task EnsureAuthenticatedAsync(BankAccountConfig config, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow >= _tokenExpiry)
        {
            await AuthenticateAsync(config, cancellationToken);
        }
    }

    #endregion

    #region Helper Methods

    private string GetBaseUrl(BankAccountConfig config)
    {
        return !string.IsNullOrEmpty(config.ApiBaseUrl) ? config.ApiBaseUrl : _settings.BaseUrl;
    }

    private List<BankStatementLine> MapTransactionsToLines(List<BHDLeonTransaction> transactions)
    {
        var lines = new List<BankStatementLine>();
        int lineNumber = 1;

        foreach (var tx in transactions.OrderBy(t => t.TransactionDate))
        {
            lines.Add(new BankStatementLine
            {
                Id = Guid.NewGuid(),
                LineNumber = lineNumber++,
                TransactionDate = tx.TransactionDate,
                ValueDate = tx.ValueDate,
                ReferenceNumber = tx.TransactionId ?? string.Empty,
                Description = tx.Description ?? string.Empty,
                Type = MapTransactionType(tx.TransactionType),
                DebitAmount = tx.DebitAmount ?? 0,
                CreditAmount = tx.CreditAmount ?? 0,
                Balance = tx.RunningBalance,
                BankCategory = tx.TransactionCategory,
                Beneficiary = tx.CounterpartyName,
                OriginAccount = tx.CounterpartyAccount
            });
        }

        return lines;
    }

    private static TransactionType MapTransactionType(string? type)
    {
        return type?.ToUpper() switch
        {
            "CREDIT" or "DEPOSIT" => TransactionType.Deposit,
            "DEBIT" or "WITHDRAWAL" => TransactionType.Withdrawal,
            "TRANSFER" or "TRANSFERENCIA" => TransactionType.Transfer,
            "FEE" or "CHARGE" or "CARGO" => TransactionType.Fee,
            "INTEREST" or "INTERES" => TransactionType.Interest,
            "REVERSAL" or "REVERSO" => TransactionType.Reversal,
            "CHECK" or "CHEQUE" => TransactionType.Other, // Cheque
            _ => TransactionType.Other
        };
    }

    private static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 4)
            return "****";
        return $"****{accountNumber[^4..]}";
    }

    #endregion

    #region Response DTOs (Private)

    private record BHDLeonTokenResponse(
        string? AccessToken,
        string? TokenType,
        int? ExpiresIn,
        string? Scope
    );

    private record BHDLeonTransactionsResponse(
        BHDLeonTransactionData? Data,
        BHDLeonLinks? Links,
        BHDLeonMeta? Meta
    );

    private record BHDLeonTransactionData(
        List<BHDLeonTransaction>? Transactions
    );

    private record BHDLeonTransaction(
        DateTime TransactionDate,
        DateTime? ValueDate,
        string? TransactionId,
        string? Description,
        string? TransactionType,
        decimal? DebitAmount,
        decimal? CreditAmount,
        decimal RunningBalance,
        string? TransactionCategory,
        string? CounterpartyName,
        string? CounterpartyAccount,
        string? TransactionStatus
    );

    private record BHDLeonLinks(
        string? Self,
        string? First,
        string? Last,
        string? Next,
        string? Previous
    );

    private record BHDLeonMeta(
        int TotalPages,
        int TotalRecords,
        int CurrentPage
    );

    private record BHDLeonBalanceResponse(
        BHDLeonBalanceData? Data
    );

    private record BHDLeonBalanceData(
        decimal AvailableBalance,
        decimal CurrentBalance,
        decimal? PendingBalance,
        string? Currency,
        DateTime LastUpdated
    );

    #endregion
}

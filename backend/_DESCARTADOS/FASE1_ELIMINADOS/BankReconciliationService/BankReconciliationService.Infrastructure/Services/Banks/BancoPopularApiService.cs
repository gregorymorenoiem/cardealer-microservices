using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Enums;
using BankReconciliationService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BankReconciliationService.Infrastructure.Services.Banks;

/// <summary>
/// Configuración específica para Banco Popular API
/// </summary>
public class BancoPopularApiSettings
{
    public const string SectionName = "BankApis:BancoPopular";
    
    public string BaseUrl { get; set; } = "https://api.bpd.com.do/v1";
    public string TokenEndpoint { get; set; } = "/oauth/token";
    public string AccountsEndpoint { get; set; } = "/accounts";
    public int TokenExpiryBufferSeconds { get; set; } = 300;
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Servicio de integración con Banco Popular Dominicano API
/// 
/// Banco Popular ofrece una API REST moderna con autenticación OAuth 2.0.
/// 
/// Características:
/// - Autenticación: OAuth 2.0 Client Credentials
/// - Costo: GRATIS
/// - Límite: 100 requests/minuto
/// - Datos en tiempo real: Sí
/// - Formatos: JSON
/// 
/// Documentación oficial: https://developers.bpd.com.do/
/// </summary>
public class BancoPopularApiService : IBankApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BancoPopularApiService> _logger;
    private readonly BancoPopularApiSettings _settings;
    
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public BancoPopularApiService(
        HttpClient httpClient, 
        ILogger<BancoPopularApiService> logger,
        IOptions<BancoPopularApiSettings>? settings = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings?.Value ?? new BancoPopularApiSettings();
    }

    #region IBankApiService Implementation

    /// <summary>
    /// Prueba la conexión con la API de Banco Popular
    /// </summary>
    public async Task<bool> TestConnectionAsync(BankAccountConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing connection to Banco Popular API for account {AccountNumber}", 
                MaskAccountNumber(config.AccountNumber));
            
            await AuthenticateAsync(config, cancellationToken);
            
            var isConnected = !string.IsNullOrEmpty(_accessToken);
            
            _logger.LogInformation("Banco Popular API connection test result: {Result}", 
                isConnected ? "SUCCESS" : "FAILED");
            
            return isConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection to Banco Popular API");
            return false;
        }
    }

    /// <summary>
    /// Obtiene transacciones de Banco Popular para el período especificado
    /// </summary>
    public async Task<List<BankStatementLine>> GetTransactionsAsync(
        BankAccountConfig config, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching transactions from Banco Popular for {From:yyyy-MM-dd} to {To:yyyy-MM-dd}", 
            from, to);
        
        await EnsureAuthenticatedAsync(config, cancellationToken);

        var url = $"{GetBaseUrl(config)}/accounts/{config.AccountNumber}/transactions" +
                  $"?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Banco Popular API error: {StatusCode} - {Content}", 
                response.StatusCode, errorContent);
            throw new HttpRequestException($"Banco Popular API error: {response.StatusCode}");
        }

        var data = await response.Content.ReadFromJsonAsync<BancoPopularTransactionsResponse>(
            cancellationToken: cancellationToken);
        
        if (data?.Transactions == null || data.Transactions.Count == 0)
        {
            _logger.LogInformation("No transactions found for the specified period");
            return new List<BankStatementLine>();
        }

        var lines = MapTransactionsToLines(data.Transactions);
        
        _logger.LogInformation("Successfully imported {Count} transactions from Banco Popular", lines.Count);
        return lines;
    }

    /// <summary>
    /// Obtiene el balance actual de la cuenta en Banco Popular
    /// </summary>
    public async Task<decimal> GetCurrentBalanceAsync(BankAccountConfig config, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching current balance from Banco Popular for account {Account}", 
            MaskAccountNumber(config.AccountNumber));
        
        await EnsureAuthenticatedAsync(config, cancellationToken);

        var url = $"{GetBaseUrl(config)}/accounts/{config.AccountNumber}/balance";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<BancoPopularBalanceResponse>(
            cancellationToken: cancellationToken);
        
        var balance = data?.AvailableBalance ?? 0;
        _logger.LogDebug("Current balance: {Balance:N2}", balance);
        
        return balance;
    }

    /// <summary>
    /// Importa un estado de cuenta completo de Banco Popular
    /// </summary>
    public async Task<BankStatement> ImportStatementAsync(
        BankAccountConfig config, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Importing full statement from Banco Popular for {From:yyyy-MM-dd} to {To:yyyy-MM-dd}", 
            from, to);

        var openingBalance = await GetBalanceAtDateAsync(config, from.AddDays(-1), cancellationToken);
        var closingBalance = await GetCurrentBalanceAsync(config, cancellationToken);
        var lines = await GetTransactionsAsync(config, from, to, cancellationToken);

        var statement = new BankStatement
        {
            Id = Guid.NewGuid(),
            BankCode = "BPD",
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
            ImportSource = "API-BPD",
            Lines = lines
        };

        // Asignar ID del statement a cada línea
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
    /// Autentica con Banco Popular usando OAuth 2.0 Client Credentials
    /// </summary>
    private async Task AuthenticateAsync(BankAccountConfig config, CancellationToken cancellationToken)
    {
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            _logger.LogDebug("Authenticating with Banco Popular API");

            var request = new HttpRequestMessage(HttpMethod.Post, 
                $"{GetBaseUrl(config)}{_settings.TokenEndpoint}");
            
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", config.ApiClientId ?? throw new InvalidOperationException("API Client ID not configured") },
                { "client_secret", config.ApiClientSecretEncrypted ?? throw new InvalidOperationException("API Client Secret not configured") }
            });

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Authentication failed: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Authentication failed: {response.StatusCode}");
            }

            var data = await response.Content.ReadFromJsonAsync<BancoPopularTokenResponse>(
                cancellationToken: cancellationToken);
            
            _accessToken = data?.AccessToken ?? throw new InvalidOperationException("No access token received");
            _tokenExpiry = DateTime.UtcNow.AddSeconds((data?.ExpiresIn ?? 3600) - _settings.TokenExpiryBufferSeconds);
            
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _accessToken);

            _logger.LogInformation("Successfully authenticated with Banco Popular API, token expires at {Expiry:HH:mm:ss}", 
                _tokenExpiry);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    /// <summary>
    /// Asegura que el token de autenticación sea válido
    /// </summary>
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

    private async Task<decimal> GetBalanceAtDateAsync(BankAccountConfig config, DateTime date, CancellationToken cancellationToken)
    {
        // Banco Popular no proporciona balance histórico directo, usamos balance actual
        // En producción, podría calcularse desde las transacciones
        return await GetCurrentBalanceAsync(config, cancellationToken);
    }

    private List<BankStatementLine> MapTransactionsToLines(List<BancoPopularTransaction> transactions)
    {
        var lines = new List<BankStatementLine>();
        int lineNumber = 1;

        foreach (var tx in transactions.OrderBy(t => t.Date))
        {
            lines.Add(new BankStatementLine
            {
                Id = Guid.NewGuid(),
                LineNumber = lineNumber++,
                TransactionDate = tx.Date,
                ValueDate = tx.ValueDate,
                ReferenceNumber = tx.Reference ?? string.Empty,
                Description = tx.Description ?? string.Empty,
                Type = MapTransactionType(tx.Type),
                DebitAmount = tx.Type?.ToUpper() == "DEBIT" ? Math.Abs(tx.Amount) : 0,
                CreditAmount = tx.Type?.ToUpper() == "CREDIT" ? Math.Abs(tx.Amount) : 0,
                Balance = tx.Balance,
                BankCategory = tx.Category,
                Beneficiary = tx.Beneficiary,
                OriginAccount = tx.OriginAccount
            });
        }

        return lines;
    }

    private static TransactionType MapTransactionType(string? type)
    {
        return type?.ToUpper() switch
        {
            "DEPOSIT" or "CREDIT" => TransactionType.Deposit,
            "WITHDRAWAL" or "DEBIT" => TransactionType.Withdrawal,
            "TRANSFER" => TransactionType.Transfer,
            "FEE" => TransactionType.Fee,
            "INTEREST" => TransactionType.Interest,
            "REVERSAL" => TransactionType.Reversal,
            "CHECK" => TransactionType.Other, // Cheque
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

    private record BancoPopularTokenResponse(
        string? AccessToken,
        string? TokenType,
        int? ExpiresIn,
        string? Scope
    );

    private record BancoPopularTransactionsResponse(
        List<BancoPopularTransaction>? Transactions,
        BancoPopularPagination? Pagination
    );

    private record BancoPopularTransaction(
        DateTime Date,
        DateTime? ValueDate,
        string? Reference,
        string? Description,
        string? Type,
        decimal Amount,
        decimal Balance,
        string? Category,
        string? Beneficiary,
        string? OriginAccount,
        string? Channel
    );

    private record BancoPopularPagination(
        int Page,
        int PageSize,
        int TotalPages,
        int TotalRecords
    );

    private record BancoPopularBalanceResponse(
        decimal AvailableBalance,
        decimal CurrentBalance,
        decimal HoldAmount,
        string? Currency,
        DateTime LastUpdated
    );

    #endregion
}

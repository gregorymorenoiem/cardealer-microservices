using BankReconciliationService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BankReconciliationService.Infrastructure.Services.Banks;

/// <summary>
/// Factory para crear servicios bancarios específicos
/// 
/// Esta factory permite obtener el servicio correcto para cada banco
/// basado en el código del banco. Soporta inyección de dependencias
/// y lazy loading de servicios.
/// </summary>
public class BankApiServiceFactory : IBankApiServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BankApiServiceFactory> _logger;

    private static readonly List<BankInfo> _supportedBanks = new()
    {
        new BankInfo(
            Code: "BPD",
            Name: "Banco Popular Dominicano",
            AuthType: "OAuth 2.0",
            FullFeatured: true,
            MonthlyCost: 0,
            ApiBaseUrl: "https://api.bpd.com.do/v1",
            Description: "El banco comercial más grande de RD. API REST moderna con autenticación OAuth 2.0. Datos en tiempo real."
        ),
        new BankInfo(
            Code: "BANRESERVAS",
            Name: "Banco de Reservas de la República Dominicana",
            AuthType: "API Key",
            FullFeatured: true,
            MonthlyCost: 30,
            ApiBaseUrl: "https://api.banreservas.com.do/v1",
            Description: "Banco estatal, el más grande por activos. API con autenticación por API Key. Actualización cada 15 minutos."
        ),
        new BankInfo(
            Code: "BHD",
            Name: "BHD León",
            AuthType: "OAuth 2.0 (Open Banking)",
            FullFeatured: true,
            MonthlyCost: 40,
            ApiBaseUrl: "https://openbanking.bhd.com.do/api/v1",
            Description: "Implementa estándar PSD2/Open Banking. OAuth 2.0 con scopes. API más completa del mercado."
        ),
        new BankInfo(
            Code: "SCOTIABANK",
            Name: "Scotiabank República Dominicana",
            AuthType: "Certificado Cliente (mTLS)",
            FullFeatured: false,
            MonthlyCost: 80,
            ApiBaseUrl: "https://api.scotiabank.com.do",
            Description: "Banco canadiense. Requiere certificado P12/PFX. API limitada a consultas. Datos batch nocturno."
        )
    };

    private static readonly Dictionary<string, string> _bankCodeAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "BPD", "BPD" },
        { "BANCOPOPULAR", "BPD" },
        { "POPULAR", "BPD" },
        { "BANCO POPULAR", "BPD" },
        
        { "BANRESERVAS", "BANRESERVAS" },
        { "RESERVAS", "BANRESERVAS" },
        { "BANCO RESERVAS", "BANRESERVAS" },
        { "BANCO DE RESERVAS", "BANRESERVAS" },
        
        { "BHD", "BHD" },
        { "BHDLEON", "BHD" },
        { "BHD LEON", "BHD" },
        { "BHD LEÓN", "BHD" },
        
        { "SCOTIABANK", "SCOTIABANK" },
        { "SCOTIA", "SCOTIABANK" },
        { "SCOTIABANK RD", "SCOTIABANK" }
    };

    public BankApiServiceFactory(
        IServiceProvider serviceProvider,
        ILogger<BankApiServiceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Crea el servicio apropiado para el banco especificado
    /// </summary>
    /// <param name="bankCode">Código del banco (ej: BPD, BANRESERVAS, BHD, SCOTIABANK)</param>
    /// <returns>Instancia del servicio bancario</returns>
    /// <exception cref="NotSupportedException">Si el banco no está soportado</exception>
    public IBankApiService CreateService(string bankCode)
    {
        var normalizedCode = NormalizeBankCode(bankCode);
        
        _logger.LogDebug("Creating bank API service for {BankCode} (normalized: {NormalizedCode})", 
            bankCode, normalizedCode);

        return normalizedCode switch
        {
            "BPD" => _serviceProvider.GetRequiredService<BancoPopularApiService>(),
            "BANRESERVAS" => _serviceProvider.GetRequiredService<BanreservasApiService>(),
            "BHD" => _serviceProvider.GetRequiredService<BHDLeonApiService>(),
            "SCOTIABANK" => _serviceProvider.GetRequiredService<ScotiabankApiService>(),
            _ => throw new NotSupportedException(
                $"Bank '{bankCode}' is not supported. Supported banks: {string.Join(", ", GetSupportedBanks().Select(b => b.Name))}")
        };
    }

    /// <summary>
    /// Obtiene lista de todos los bancos soportados
    /// </summary>
    public IReadOnlyList<BankInfo> GetSupportedBanks()
    {
        return _supportedBanks.AsReadOnly();
    }

    /// <summary>
    /// Verifica si un banco está soportado
    /// </summary>
    public bool IsBankSupported(string bankCode)
    {
        return _bankCodeAliases.ContainsKey(bankCode?.ToUpperInvariant() ?? "");
    }

    /// <summary>
    /// Obtiene información detallada de un banco específico
    /// </summary>
    public BankInfo? GetBankInfo(string bankCode)
    {
        var normalizedCode = NormalizeBankCode(bankCode);
        return _supportedBanks.FirstOrDefault(b => b.Code == normalizedCode);
    }

    /// <summary>
    /// Obtiene el código normalizado del banco
    /// </summary>
    public string? GetNormalizedBankCode(string bankCode)
    {
        return _bankCodeAliases.TryGetValue(bankCode?.ToUpperInvariant() ?? "", out var normalized)
            ? normalized
            : null;
    }

    /// <summary>
    /// Obtiene todos los alias para un código de banco
    /// </summary>
    public IEnumerable<string> GetBankAliases(string bankCode)
    {
        var normalizedCode = NormalizeBankCode(bankCode);
        return _bankCodeAliases
            .Where(kvp => kvp.Value == normalizedCode)
            .Select(kvp => kvp.Key);
    }

    private static string NormalizeBankCode(string bankCode)
    {
        if (string.IsNullOrWhiteSpace(bankCode))
            throw new ArgumentException("Bank code cannot be empty", nameof(bankCode));

        if (_bankCodeAliases.TryGetValue(bankCode.ToUpperInvariant(), out var normalized))
            return normalized;

        throw new NotSupportedException(
            $"Bank '{bankCode}' is not supported. Supported codes: BPD, BANRESERVAS, BHD, SCOTIABANK");
    }
}

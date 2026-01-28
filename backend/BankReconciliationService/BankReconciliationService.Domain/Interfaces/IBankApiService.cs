using BankReconciliationService.Domain.Entities;

namespace BankReconciliationService.Domain.Interfaces;

/// <summary>
/// Interface para servicios de integración con APIs bancarias
/// </summary>
public interface IBankApiService
{
    /// <summary>
    /// Prueba la conexión con la API del banco
    /// </summary>
    Task<bool> TestConnectionAsync(BankAccountConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las transacciones de un período específico
    /// </summary>
    Task<List<BankStatementLine>> GetTransactionsAsync(
        BankAccountConfig config,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el balance actual de la cuenta
    /// </summary>
    Task<decimal> GetCurrentBalanceAsync(BankAccountConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Importa un estado de cuenta completo del período especificado
    /// </summary>
    Task<BankStatement> ImportStatementAsync(
        BankAccountConfig config,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Información básica de un banco soportado
/// </summary>
public record BankInfo(
    string Code,
    string Name,
    string AuthType,
    bool FullFeatured,
    decimal MonthlyCost,
    string ApiBaseUrl,
    string Description
);

/// <summary>
/// Factory para crear servicios bancarios
/// </summary>
public interface IBankApiServiceFactory
{
    /// <summary>
    /// Crea el servicio apropiado para el banco especificado
    /// </summary>
    IBankApiService CreateService(string bankCode);

    /// <summary>
    /// Obtiene lista de bancos soportados
    /// </summary>
    IReadOnlyList<BankInfo> GetSupportedBanks();

    /// <summary>
    /// Verifica si un banco está soportado
    /// </summary>
    bool IsBankSupported(string bankCode);
}

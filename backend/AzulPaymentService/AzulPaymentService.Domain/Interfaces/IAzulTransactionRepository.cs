using AzulPaymentService.Domain.Entities;

namespace AzulPaymentService.Domain.Interfaces;

/// <summary>
/// Interfaz para operaciones de repositorio de transacciones AZUL
/// </summary>
public interface IAzulTransactionRepository
{
    /// <summary>
    /// Crear una nueva transacción
    /// </summary>
    Task<AzulTransaction> CreateAsync(AzulTransaction transaction, CancellationToken ct = default);

    /// <summary>
    /// Obtener transacción por ID
    /// </summary>
    Task<AzulTransaction?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtener transacción por ID de AZUL
    /// </summary>
    Task<AzulTransaction?> GetByAzulIdAsync(string azulTransactionId, CancellationToken ct = default);

    /// <summary>
    /// Obtener todas las transacciones de un usuario (paginado)
    /// </summary>
    Task<(List<AzulTransaction> Transactions, int Total)> GetByUserIdAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20, 
        CancellationToken ct = default);

    /// <summary>
    /// Obtener transacciones por estado
    /// </summary>
    Task<List<AzulTransaction>> GetByStatusAsync(
        TransactionStatus status, 
        CancellationToken ct = default);

    /// <summary>
    /// Actualizar transacción
    /// </summary>
    Task<AzulTransaction> UpdateAsync(AzulTransaction transaction, CancellationToken ct = default);

    /// <summary>
    /// Obtener transacciones pendientes (para reconciliación)
    /// </summary>
    Task<List<AzulTransaction>> GetPendingTransactionsAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtener transacciones por rango de fechas (admin)
    /// </summary>
    Task<(List<AzulTransaction> Transactions, int Total)> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        int page = 1, 
        int pageSize = 20, 
        CancellationToken ct = default);

    /// <summary>
    /// Obtener monto total de transacciones aprobadas
    /// </summary>
    Task<decimal> GetTotalApprovedAmountAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
}

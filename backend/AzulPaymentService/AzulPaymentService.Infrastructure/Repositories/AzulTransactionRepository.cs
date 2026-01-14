using Microsoft.EntityFrameworkCore;
using AzulPaymentService.Domain.Entities;
using AzulPaymentService.Domain.Enums;
using AzulPaymentService.Domain.Interfaces;
using AzulPaymentService.Infrastructure.Persistence;

namespace AzulPaymentService.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de transacciones AZUL
/// </summary>
public class AzulTransactionRepository : IAzulTransactionRepository
{
    private readonly AzulDbContext _context;

    public AzulTransactionRepository(AzulDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Crea una nueva transacción
    /// </summary>
    public async Task<AzulTransaction> CreateAsync(AzulTransaction transaction, CancellationToken cancellationToken = default)
    {
        _context.AzulTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    /// <summary>
    /// Obtiene transacción por ID interno
    /// </summary>
    public async Task<AzulTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AzulTransactions.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Obtiene transacción por ID de AZUL
    /// </summary>
    public async Task<AzulTransaction?> GetByAzulIdAsync(string azulTransactionId, CancellationToken cancellationToken = default)
    {
        return await _context.AzulTransactions
            .FirstOrDefaultAsync(x => x.AzulTransactionId == azulTransactionId, cancellationToken);
    }

    /// <summary>
    /// Obtiene transacciones de un usuario con paginación
    /// </summary>
    public async Task<(List<AzulTransaction> Transactions, int Total)> GetByUserIdAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.AzulTransactions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var transactions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (transactions, totalCount);
    }

    /// <summary>
    /// Obtiene transacciones por estado
    /// </summary>
    public async Task<List<AzulTransaction>> GetByStatusAsync(
        Domain.Enums.TransactionStatus status, 
        CancellationToken cancellationToken = default)
    {
        return await _context.AzulTransactions
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Actualiza una transacción
    /// </summary>
    public async Task<AzulTransaction> UpdateAsync(AzulTransaction transaction, CancellationToken cancellationToken = default)
    {
        _context.AzulTransactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    /// <summary>
    /// Obtiene transacciones pendientes
    /// </summary>
    public async Task<List<AzulTransaction>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AzulTransactions
            .Where(x => x.Status == Domain.Enums.TransactionStatus.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene transacciones en un rango de fechas con paginación
    /// </summary>
    public async Task<(List<AzulTransaction> Transactions, int Total)> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        int page = 1, 
        int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.AzulTransactions
            .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var transactions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (transactions, totalCount);
    }

    /// <summary>
    /// Obtiene monto total aprobado en un rango de fechas
    /// </summary>
    public async Task<decimal> GetTotalApprovedAmountAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.AzulTransactions
            .Where(x => x.Status == TransactionStatus.Approved);

        if (startDate.HasValue)
            query = query.Where(x => x.CreatedAt >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(x => x.CreatedAt <= endDate.Value);

        return await query.SumAsync(x => x.Amount, cancellationToken);
    }
}

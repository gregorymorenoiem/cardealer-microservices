namespace BankReconciliationService.Domain.Enums;

public enum ReconciliationStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    RequiresReview = 3,
    Approved = 4,
    Cancelled = 5
}

public enum TransactionType
{
    Deposit = 0,      // Depósito
    Withdrawal = 1,   // Retiro
    Transfer = 2,     // Transferencia
    Fee = 3,          // Comisión bancaria
    Interest = 4,     // Intereses
    Adjustment = 5,   // Ajuste
    Reversal = 6,     // Reversión
    Other = 99
}

public enum MatchType
{
    Exact = 0,           // Match exacto (fecha, monto, referencia)
    AmountAndDate = 1,   // Match por monto y fecha (sin referencia)
    AmountOnly = 2,      // Solo coincide el monto (diferente fecha)
    Partial = 3,         // Match parcial (pagos divididos)
    Manual = 4,          // Match manual por usuario
    ML = 5               // Match sugerido por Machine Learning
}

public enum DiscrepancyType
{
    MissingInBank = 0,       // Transacción interna sin match en banco
    MissingInSystem = 1,     // Transacción bancaria sin match en sistema
    AmountDifference = 2,    // Montos no coinciden
    DateDifference = 3,      // Fechas muy diferentes
    DuplicateBank = 4,       // Duplicado en estado de cuenta
    DuplicateInternal = 5,   // Duplicado en sistema
    BankFee = 6,             // Comisión bancaria no registrada
    UnknownDeposit = 7,      // Depósito sin origen identificado
    UnknownWithdrawal = 8,   // Retiro sin registro
    Other = 99
}

public enum DiscrepancyStatus
{
    Pending = 0,
    UnderReview = 1,
    Resolved = 2,
    RequiresAdjustment = 3,
    Ignored = 4
}

public enum BankProvider
{
    BancoPopular = 0,
    Banreservas = 1,
    BHDLeon = 2,
    Scotiabank = 3,
    BancoSantaDomingo = 4,
    BancoLopezDeHaro = 5,
    BancoMultiple = 6,
    Other = 99
}

public enum ReconciliationMethod
{
    Automatic = 0,    // ML + rules-based matching
    SemiAutomatic = 1, // Suggestions + manual approval
    Manual = 2        // Completamente manual
}

public enum ImportMethod
{
    BankAPI = 0,      // API directa del banco
    CSV = 1,          // CSV exportado del banco
    Excel = 2,        // Excel exportado
    Manual = 3,       // Entrada manual
    Aggregator = 4    // Agregador como Fygaro
}
